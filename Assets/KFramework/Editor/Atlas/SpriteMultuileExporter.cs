using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FC.Editor.Tools
{
    /// <summary>
    /// 将 Unity 中 "Sprite Mode = Multiple" 的纹理拆分为许多独立的小图（PNG）。
    /// 每个 Sprite 会被裁切并导出为一张单独的 PNG，命名规则为 "原图名_精灵名.png"。
    /// </summary>
    public static class SpriteAtlasExporter
    {
        private const string MenuPath = "Assets/Split Sprite (Multiple) into Images";

        [MenuItem(MenuPath, false, 2000)]
        private static void ExportFromSelection()
        {
            var textures = Selection.objects
                .OfType<Texture2D>()
                .Select(AssetDatabase.GetAssetPath)
                .ToArray();

            if (textures.Length == 0)
            {
                EditorUtility.DisplayDialog("Sprite Splitter",
                    "请选中一个或多个使用 Multiple Sprite 模式的纹理（.png/.jpg 等）。", "确定");
                return;
            }

            int ok = 0;
            foreach (var path in textures)
            {
                if (Export(path)) ok++;
            }

            if (ok > 0)
            {
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Sprite Splitter",
                    string.Format("成功拆分 {0}/{1} 个纹理。", ok, textures.Length), "确定");
            }
        }

        [MenuItem(MenuPath, true)]
        private static bool ExportFromSelectionValidate()
        {
            return Selection.objects.OfType<Texture2D>().Any();
        }

        /// <summary>
        /// 执行拆分。返回是否成功拆分出至少一张小图。
        /// </summary>
        public static bool Export(string texturePath)
        {
            if (string.IsNullOrEmpty(texturePath)) return false;

            var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning("[SpriteSplitter] 不是有效纹理: " + texturePath);
                return false;
            }

            if (importer.spriteImportMode != SpriteImportMode.Multiple)
            {
                Debug.LogWarning("[SpriteSplitter] 仅支持 Multiple Sprite 模式: " + texturePath);
                return false;
            }

            // 裁切需要读取像素，确保纹理可读（Read/Write Enabled）。
            bool wasReadable = importer.isReadable;
            if (!wasReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            try
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (texture == null) return false;

                int texW = texture.width;
                int texH = texture.height;

                // sprite.rect 定义在源图分辨率上；若导入时被 Max Size 等缩放，
                // 需按比例换算到当前纹理分辨率，否则坐标越界或错位。
                // 从文件头读取源图真实尺寸（支持 PNG / GIF）。
                int srcW = texW, srcH = texH;
                if (TryGetSourceSize(texturePath, out int sW, out int sH) && sW > 0 && sH > 0)
                {
                    srcW = sW;
                    srcH = sH;
                }
                float scaleX = (float)texW / srcW;
                float scaleY = (float)texH / srcH;

                var sprites = AssetDatabase.LoadAllAssetsAtPath(texturePath)
                    .OfType<Sprite>()
                    .OrderBy(s => s.name)
                    .ToArray();

                if (sprites.Length == 0)
                {
                    Debug.LogWarning("[SpriteSplitter] 未找到任何 Sprite: " + texturePath);
                    return false;
                }

                // 统一输出到工程根目录/AAA/Temp
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                string dir = Path.Combine(projectRoot, "AAA", "Temp");
                Directory.CreateDirectory(dir);
                string baseName = Path.GetFileNameWithoutExtension(texturePath);

                int ok = 0;
                foreach (var sprite in sprites)
                {
                    // sprite.rect 为源图像素坐标（左上原点），按导入缩放换算，
                    // 再转为 Texture2D.GetPixels 的左下原点坐标。
                    Rect r = sprite.rect;
                    float sx = r.x * scaleX;
                    float syTop = r.y * scaleY;
                    float sw = r.width * scaleX;
                    float sh = r.height * scaleY;

                    int bx = Mathf.Clamp(Mathf.RoundToInt(sx), 0, texW);
                    int rightX = Mathf.Clamp(Mathf.RoundToInt(sx + sw), 0, texW);
                    int byBottom = Mathf.Clamp(Mathf.RoundToInt(texH - (syTop + sh)), 0, texH);
                    int topBottom = Mathf.Clamp(Mathf.RoundToInt(texH - syTop), 0, texH);
                    int w = rightX - bx;
                    int h = topBottom - byBottom;
                    if (w <= 0 || h <= 0) continue;

                    Color[] pixels = texture.GetPixels(bx, byBottom, w, h);
                    var crop = new Texture2D(w, h, TextureFormat.RGBA32, false);
                    crop.SetPixels(pixels);
                    crop.Apply();

                    string outPath = Path.Combine(dir, baseName + "_" + sprite.name + ".png");
                    File.WriteAllBytes(outPath, crop.EncodeToPNG());
                    Object.DestroyImmediate(crop);
                    ok++;
                }

                Debug.Log("[SpriteSplitter] 已从 " + texturePath + " 拆分出 " + ok + " 张小图。");
                return ok > 0;
            }
            finally
            {
                // 还原可读性设置，避免影响运行时内存占用
                if (!wasReadable)
                {
                    importer.isReadable = false;
                    importer.SaveAndReimport();
                }
            }
        }

        /// <summary>
        /// 从图片文件头读取源图尺寸（支持 PNG / GIF），用于校正导入缩放。
        /// 读取失败或格式不支持时返回 false。
        /// </summary>
        private static bool TryGetSourceSize(string path, out int width, out int height)
        {
            width = height = -1;
            try
            {
                byte[] data = File.ReadAllBytes(path);
                if (data.Length < 24) return false;

                // PNG: 签名 89 50 4E 47，宽/高位于偏移 16 / 20（大端）
                if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
                {
                    width = (data[16] << 24) | (data[17] << 16) | (data[18] << 8) | data[19];
                    height = (data[20] << 24) | (data[21] << 16) | (data[22] << 8) | data[23];
                    return true;
                }

                // GIF: 签名 "GIF"，宽/高位于偏移 6 / 8（小端）
                if (data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46)
                {
                    width = data[6] | (data[7] << 8);
                    height = data[8] | (data[9] << 8);
                    return true;
                }
            }
            catch
            {
                // 忽略异常，交给调用方回退到导入尺寸
            }
            return false;
        }
    }
}
