using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace KFramework.Editor.Atlas
{
    /// <summary>
    /// 类似 TexturePacker 的图集打包插件。
    ///
    /// - <see cref="Export_K_Atlas"/>                : 选中文件夹（含所有子文件夹），把所有小图打包为一张大图，返回结果（不写文件）。
    /// - <see cref="UnitySprite_Export_K_Atlas(string)"/> : 调用上面的打包方法，输出"图集 png" + "json"（json 通过 JsonTool 序列化）。
    /// - <see cref="UnitySprite_Export_K_Atlas(Texture2D)"/> : 针对已有的 Unity（Multiple Sprite）纹理，直接导出图集 png + json。
    ///
    /// 输出兼容 TexturePacker 的 JSON（hash / array）格式，Y 轴已翻转，pivot 默认中心 (0.5,0.5)。
    /// 装箱复用同目录的 <see cref="MaxRectsPacker"/>（BSSF 最适短边优先）。
    /// </summary>
    public static class KAtlasPacker
    {
        // 是否输出 hash（frames 为命名对象，默认）或 array（frames 为数组）格式
        private static bool s_UseArrayFormat;

        // 图集最大边长。超过则打包失败。
        private const int MaxAtlasSize = 4096;

        // 图集内 sprite 之间的留白（像素），避免采样渗色。默认 1。
        private const int Padding = 1;

        #region 公共 API

        /// <summary>
        /// 把 folderPath 下（递归）的所有小图打包为一张大图。返回包含合成大图与每帧位置的 <see cref="AtlasResult"/>。
        /// </summary>
        public static AtlasResult Export_K_Atlas(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError("[KAtlasPacker] 不是有效文件夹: " + folderPath);
                return null;
            }

            // 1. 收集文件夹下所有图片纹理
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
            var entries = new List<ImageEntry>();
            var seen = new HashSet<string>();
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;
                if (!IsSupportedImage(path)) continue;
                string name = Path.GetFileNameWithoutExtension(path);
                // 同名去重（不同子目录同名小图仅取第一个，避免图集 key 冲突）
                if (!seen.Add(name)) continue;

                var tex = LoadSourceTexture(path);
                if (tex == null) continue;
                entries.Add(new ImageEntry { name = name, texture = tex, width = tex.width, height = tex.height });
            }

            if (entries.Count == 0)
            {
                Debug.LogWarning("[KAtlasPacker] 文件夹下未找到任何可用小图: " + folderPath);
                return null;
            }

            // 2. 按面积从大到小排序，提升装箱率
            entries.Sort((a, b) => b.width * b.height - a.width * a.height);

            // 3. 从小到大尝试候选尺寸，取能放下全部图的最小尺寸
            int[] sizes = { 256, 512, 1024, 2048, 4096 };
            foreach (int size in sizes)
            {
                if (size > MaxAtlasSize) break;
                var packer = new MaxRectsPacker(size, size);
                var placements = new List<Placement>();
                bool ok = true;
                foreach (var e in entries)
                {
                    if (packer.Insert(e.width + Padding, e.height + Padding, out var r))
                    {
                        placements.Add(new Placement
                        {
                            entry = e,
                            x = r.x,
                            y = r.y,
                            w = e.width,
                            h = e.height
                        });
                    }
                    else
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok)
                {
                    Debug.Log(string.Format("[KAtlasPacker] 打包完成：{0} 张小图，图集尺寸 {1}x{2}",
                        entries.Count, size, size));
                    return Composite(size, placements);
                }
            }

            Debug.LogError(string.Format(
                "[KAtlasPacker] 无法在 {0}x{0} 内放下全部 {1} 张小图（存在超过最大尺寸的图？）。",
                MaxAtlasSize, entries.Count));
            // 释放已加载纹理，避免泄漏
            foreach (var e in entries) Object.DestroyImmediate(e.texture);
            return null;
        }

        /// <summary>
        /// 打包文件夹并输出图集 png + json（json 通过 JsonTool 序列化）。
        /// </summary>
        public static void UnitySprite_Export_K_Atlas(string folderPath)
        {
            var result = Export_K_Atlas(folderPath);
            if (result == null) return;

            string folderName = Path.GetFileName(folderPath.TrimEnd('/', '\\'));
            if (string.IsNullOrEmpty(folderName)) folderName = "KAtlas";
            string atlasName = folderName + "_atlas";

            string pngPath = Path.Combine(folderPath, atlasName + ".png").Replace('\\', '/');
            string jsonPath = Path.Combine(folderPath, atlasName + ".json").Replace('\\', '/');

            WriteAtlasPng(result, pngPath);
            WriteAtlasJson(result.frames, Path.GetFileName(pngPath), result.width, result.height, jsonPath);

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("K Atlas Packer",
                string.Format("已导出图集：\n{0}\n{1}", pngPath, jsonPath), "确定");
        }

        /// <summary>
        /// 针对已有的 Unity（Multiple Sprite）纹理，导出图集 png（复制原图）+ json（通过 JsonTool）。
        /// </summary>
        public static void UnitySprite_Export_K_Atlas(Texture2D texture)
        {
            if (texture == null) return;
            string texPath = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(texPath))
            {
                Debug.LogError("[KAtlasPacker] 该纹理不是项目内资源，无法导出。");
                return;
            }

            var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (importer != null && importer.spriteImportMode != SpriteImportMode.Multiple)
                Debug.LogWarning("[KAtlasPacker] 该纹理不是 Multiple Sprite 模式，按整图导出。");

            int texW = texture.width;
            int texH = texture.height;

            var frames = new List<KAtlasFrame>();
            var sprites = AssetDatabase.LoadAllAssetsAtPath(texPath).OfType<Sprite>().ToList();
            if (sprites.Count > 0)
            {
                sprites.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
                foreach (var sprite in sprites)
                {
                    // textureRect 位于实际导入纹理像素空间，已含 Max Size 缩放校正
                    Rect r = sprite.textureRect;
                    int x = Mathf.RoundToInt(r.x);
                    int y = Mathf.RoundToInt(r.y);
                    int w = Mathf.RoundToInt(r.width);
                    int h = Mathf.RoundToInt(r.height);
                    int tpY = texH - y - h; // TexturePacker Y 轴朝上

                    frames.Add(MakeFrame(sprite.name, x, tpY, w, h, w, h, sprite.pivot.x, sprite.pivot.y));
                }
            }
            else
            {
                frames.Add(MakeFrame(texture.name, 0, 0, texW, texH, texW, texH, 0.5f, 0.5f));
            }

            string dir = Path.GetDirectoryName(texPath).Replace('\\', '/');
            string atlasName = texture.name + "_atlas";
            string pngPath = dir + "/" + atlasName + ".png";
            string jsonPath = dir + "/" + atlasName + ".json";

            // 图集图片即原纹理本身：直接复制源文件字节，保证与 json 矩形严格一致
            File.Copy(texPath, pngPath, true);
            WriteAtlasJson(frames, Path.GetFileName(pngPath), texW, texH, jsonPath);

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("K Atlas Packer",
                string.Format("已导出图集：\n{0}\n{1}", pngPath, jsonPath), "确定");
        }

        #endregion

        #region 菜单入口

        [MenuItem("Assets/KFramework/Atlas/Pack Folder to K-Atlas", false, 2000)]
        private static void MenuPackFolder()
        {
            string folder = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!AssetDatabase.IsValidFolder(folder))
            {
                EditorUtility.DisplayDialog("K Atlas Packer", "请选中一个文件夹。", "确定");
                return;
            }
            UnitySprite_Export_K_Atlas(folder);
        }

        [MenuItem("Assets/KFramework/Atlas/Pack Folder to K-Atlas", true)]
        private static bool MenuPackFolderValidate()
        {
            string folder = AssetDatabase.GetAssetPath(Selection.activeObject);
            return AssetDatabase.IsValidFolder(folder);
        }

        [MenuItem("Assets/KFramework/Atlas/Export Unity Sprite K-Atlas", false, 2001)]
        private static void MenuExportSprite()
        {
            var tex = Selection.activeObject as Texture2D;
            if (tex == null)
            {
                EditorUtility.DisplayDialog("K Atlas Packer", "请选中一个纹理资源。", "确定");
                return;
            }
            UnitySprite_Export_K_Atlas(tex);
        }

        [MenuItem("Assets/KFramework/Atlas/Export Unity Sprite K-Atlas", true)]
        private static bool MenuExportSpriteValidate()
        {
            return Selection.activeObject is Texture2D;
        }

        [MenuItem("KFramework/Atlas/Toggle Output Format (Hash <-> Array)")]
        private static void ToggleFormat()
        {
            s_UseArrayFormat = !s_UseArrayFormat;
            Debug.Log("[KAtlasPacker] 当前输出格式: " + (s_UseArrayFormat ? "array" : "hash"));
        }

        #endregion

        #region 打包实现

        private static bool IsSupportedImage(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            return ext == ".png" || ext == ".jpg" || ext == ".jpeg";
        }

        /// <summary>
        /// 从文件字节加载可读纹理，得到与源图完全一致的像素，不受导入设置（readable / Max Size）影响。
        /// </summary>
        private static Texture2D LoadSourceTexture(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(bytes))
            {
                Object.DestroyImmediate(tex);
                return null;
            }
            return tex;
        }

        /// <summary>
        /// 把若干小图按 placement 合成到 size×size 大图（左上为原点）。返回结果。
        /// </summary>
        private static AtlasResult Composite(int size, List<Placement> placements)
        {
            var atlas = new Texture2D(size, size, TextureFormat.RGBA32, false);
            atlas.name = "KAtlas";

            var frames = new List<KAtlasFrame>();
            foreach (var p in placements)
            {
                Color32[] pixels = p.entry.texture.GetPixels32();
                // Unity 纹理原点在左下，放到 atlas 对应区域（留出 Padding 偏移）
                int dstX = p.x;
                int dstY = size - p.y - p.h;
                atlas.SetPixels32(dstX, dstY, p.w, p.h, pixels);

                // TexturePacker 用左上原点，y 翻转
                frames.Add(MakeFrame(p.entry.name, p.x, p.y, p.w, p.h, p.w, p.h, 0.5f, 0.5f));

                Object.DestroyImmediate(p.entry.texture);
            }

            atlas.Apply();
            return new AtlasResult { atlas = atlas, width = size, height = size, frames = frames };
        }

        private static KAtlasFrame MakeFrame(string name, int x, int y, int w, int h,
            int srcW, int srcH, float pivotX, float pivotY)
        {
            return new KAtlasFrame
            {
                filename = name,
                frame = new KRect { x = x, y = y, w = w, h = h },
                rotated = false,
                trimmed = false,
                spriteSourceSize = new KRect { x = 0, y = 0, w = srcW, h = srcH },
                sourceSize = new KSize { w = srcW, h = srcH },
                pivot = new KPivot
                {
                    x = float.Parse(pivotX.ToString("0.000", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture),
                    y = float.Parse(pivotY.ToString("0.000", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)
                }
            };
        }

        private static void WriteAtlasPng(AtlasResult result, string pngPath)
        {
            byte[] png = result.atlas.EncodeToPNG();
            File.WriteAllBytes(pngPath, png);
            Object.DestroyImmediate(result.atlas);
        }

        private static void WriteAtlasJson(List<KAtlasFrame> frames, string imageName,
            int texW, int texH, string jsonPath)
        {
            object data = s_UseArrayFormat
                ? (object)new KAtlasDataArray
                {
                    frames = frames,
                    meta = MakeMeta(imageName, texW, texH)
                }
                : new KAtlasDataHash
                {
                    frames = frames.ToDictionary(f => f.filename, f => f),
                    meta = MakeMeta(imageName, texW, texH)
                };

            // 通过 KFramework 的 JsonTool（Newtonsoft.Json）序列化
            string json = global::JsonTool.ToJson(data);
            File.WriteAllText(jsonPath, json, System.Text.Encoding.UTF8);
            Debug.Log("[KAtlasPacker] 已写入 json: " + jsonPath);
        }

        private static KAtlasMeta MakeMeta(string imageName, int texW, int texH)
        {
            return new KAtlasMeta
            {
                app = "KFramework Atlas Packer",
                version = "1.0",
                image = imageName,
                format = "RGBA8888",
                size = new KSize { w = texW, h = texH },
                scale = "1"
            };
        }

        #endregion

        #region 数据结构

        public class AtlasResult
        {
            public Texture2D atlas;
            public int width;
            public int height;
            public List<KAtlasFrame> frames;
        }

        private class ImageEntry
        {
            public string name;
            public Texture2D texture;
            public int width;
            public int height;
        }

        private class Placement
        {
            public ImageEntry entry;
            public int x, y, w, h; // 左上原点
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class KAtlasDataHash
        {
            [JsonProperty("frames")] public Dictionary<string, KAtlasFrame> frames;
            [JsonProperty("meta")] public KAtlasMeta meta;
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class KAtlasDataArray
        {
            [JsonProperty("frames")] public List<KAtlasFrame> frames;
            [JsonProperty("meta")] public KAtlasMeta meta;
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class KAtlasFrame
        {
            [JsonProperty("filename")] public string filename;
            [JsonProperty("frame")] public KRect frame;
            [JsonProperty("rotated")] public bool rotated;
            [JsonProperty("trimmed")] public bool trimmed;
            [JsonProperty("spriteSourceSize")] public KRect spriteSourceSize;
            [JsonProperty("sourceSize")] public KSize sourceSize;
            [JsonProperty("pivot")] public KPivot pivot;
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class KRect
        {
            [JsonProperty("x")] public int x;
            [JsonProperty("y")] public int y;
            [JsonProperty("w")] public int w;
            [JsonProperty("h")] public int h;
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class KSize
        {
            [JsonProperty("w")] public int w;
            [JsonProperty("h")] public int h;
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class KPivot
        {
            [JsonProperty("x")] public float x;
            [JsonProperty("y")] public float y;
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class KAtlasMeta
        {
            [JsonProperty("app")] public string app;
            [JsonProperty("version")] public string version;
            [JsonProperty("image")] public string image;
            [JsonProperty("format")] public string format;
            [JsonProperty("size")] public KSize size;
            [JsonProperty("scale")] public string scale;
        }

        #endregion
    }
}
