using System;
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Object = UnityEngine.Object;

namespace Util
{
    [CanEditMultipleObjects]
    [HelpURL("https://docs.unity3d.com/Manual/SL-TextureArrays.html")]
    [ScriptedImporter(k_VersionNumber, k_FileExtension)]
    public class Texture2DArrayImporter : ScriptedImporter
    {
        // File Extension For Texture2DArray assets
        public const string k_FileExtension = "tex2darray";

        // Version Number For Importer
        private const int k_VersionNumber = 0;

        [Tooltip("The Initial Texture2DArray asset.")] [SerializeField]
        private Texture2DArray m_Tex2DArray;

        // Properties
        [Tooltip("Selects how the Texture behaves when tiled.")] [SerializeField]
        private TextureWrapMode m_WrapMode = TextureWrapMode.Repeat;

        [Tooltip("Selects how the Texture is filtered when it gets stretched by 3D transformations.")] [SerializeField]
        private FilterMode m_FilterMode = FilterMode.Bilinear;

        [Tooltip(
            "Increases Texture quality when viewing the texture at a steep angle.\n0 = Disabled for all textures\n1 = Enabled for all textures in Quality Settings\n2..16 = Anisotropic filtering level")]
        [Range(0, 16)]
        [SerializeField]
        private int m_AnisoLevel = 1;

        [SerializeField] private bool m_IsReadable;

        public TextureWrapMode wrapMode { get; set; }

        public FilterMode filterMode { get; set; }

        public int anisoLevel { get; set; }

        public bool isReadable { get; set; }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            // var assetPath = Path.ChangeExtension(ctx.assetPath, ".asset");

            // Object assetObject = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            // Texture2D myObject = assetObject as Texture2D;

            if (m_Tex2DArray == null)
            {
                ctx.LogImportWarning("The Initial Texture2DArray asset Can not be null!!!");
                m_Tex2DArray = new Texture2DArray(256, 256, 1,
                    GraphicsFormat.B8G8R8A8_UNorm,
                    TextureCreationFlags.None);
                var errorTexture =
                    new Texture2D(256, 256, GraphicsFormat.B8G8R8A8_UNorm, TextureCreationFlags.None);
                try
                {
                    var errorPixels = errorTexture.GetPixels32();
                    for (var n = 0; n < errorPixels.Length; ++n)
                        errorPixels[n] = Color.magenta;
                    errorTexture.SetPixels32(errorPixels);
                    errorTexture.Apply();

                    for (var n = 0; n < m_Tex2DArray.depth; ++n)
                        Graphics.CopyTexture(errorTexture, 0, m_Tex2DArray, n);
                }
                finally
                {
                    DestroyImmediate(errorTexture);
                }

                ctx.AddObjectToAsset("Texture2DArray", m_Tex2DArray);
                ctx.SetMainObject(m_Tex2DArray);

                return;
            }

            var path = AssetDatabase.GetAssetPath(m_Tex2DArray);
            ctx.DependsOnSourceAsset(path);

            // Process Texture2DArray asset
            ProcessTexture2DArrayBySettings();

            ctx.AddObjectToAsset("Texture2DArray", m_Tex2DArray);
            ctx.SetMainObject(m_Tex2DArray);
        }

        // 获取指定平台的纹理导入设置
        private TextureImporterPlatformSettings GetPlatformTexture2DArraySetting(string platform)
        {
            throw new NotImplementedException();
        }

        private void ProcessTexture2DArrayBySettings()
        {
            var width = m_Tex2DArray.width;
            var height = m_Tex2DArray.height;
            var mipmapEnabled = true;
            var textureFormat = m_Tex2DArray.format;
            var srgbTexture = true;

            throw new NotImplementedException();
        }

        private Texture2DArray ConvertTextureArrayToFormat(Texture2DArray sourceArray, GraphicsFormat format)
        {
            var width = sourceArray.width;
            var height = sourceArray.height;
            var depth = sourceArray.depth;

            // Create a new Texture2DArray with the specified format
            var newArray = new Texture2DArray(width, height, depth, format, TextureCreationFlags.MipChain);

            // Copy the textures from the source array to the new array
            for (var i = 0; i < depth; i++) Graphics.CopyTexture(sourceArray, i, 0, newArray, i, 0);

            // Apply the changes
            newArray.Apply(false, true);

            return newArray;
        }

        [MenuItem("Assets/Create/Texture2D Array", priority = 310)]
        private static void CreateTexture2DArrayMenuItem()
        {
            // https://forum.unity.com/threads/how-to-implement-create-new-asset.759662/
            var directoryPath = "Assets";
            foreach (var obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                directoryPath = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(directoryPath) && File.Exists(directoryPath))
                {
                    directoryPath = Path.GetDirectoryName(directoryPath);
                    break;
                }
            }

            directoryPath = directoryPath.Replace("\\", "/");
            if (directoryPath.Length > 0 && directoryPath[directoryPath.Length - 1] != '/')
                directoryPath += "/";
            if (string.IsNullOrEmpty(directoryPath))
                directoryPath = "Assets/";

            var fileName = $"New Texture2DArray.{k_FileExtension}";
            directoryPath = AssetDatabase.GenerateUniqueAssetPath(directoryPath + fileName);
            ProjectWindowUtil.CreateAssetWithContent(directoryPath,
                "This file represents a Texture2DArray asset for Unity.");
        }
    }
}