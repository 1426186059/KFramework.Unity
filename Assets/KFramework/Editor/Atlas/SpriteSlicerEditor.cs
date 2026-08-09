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

                Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(texturePath)
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
                string textureDir = Path.GetFileNameWithoutExtension(texturePath);
                string dir = Path.Combine(projectRoot, "AAA/Atlas/", textureDir);
                Directory.CreateDirectory(dir);

                int ok = 0;
                foreach (var sprite in sprites)
                {
                    Rect r = sprite.rect;
                    float sx = r.x;
                    float syTop = r.y;
                    float sw = r.width;
                    float sh = r.height;

                    if (sw <= 0 || sh <= 0) continue;

                    Color[] pixels = texture.GetPixels(Mathf.RoundToInt(r.x), Mathf.RoundToInt(r.y), Mathf.RoundToInt(sw), Mathf.RoundToInt(sh));
                    var crop = new Texture2D(Mathf.RoundToInt(sw), Mathf.RoundToInt(sh), TextureFormat.RGBA32, false);
                    crop.SetPixels(pixels);
                    crop.Apply();

                    string outPath = Path.Combine(dir, sprite.name + ".png");
                    File.WriteAllBytes(outPath, crop.EncodeToPNG());
                    Object.DestroyImmediate(crop);
                    ok++;
                }

                Debug.Log("[SpriteSplitter] 已从 " + texturePath + " 拆分出 " + ok + " 张小图");
                Debug.Log("输出目录: " + dir);
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
    }
}
