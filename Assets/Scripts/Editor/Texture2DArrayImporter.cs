using System;
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
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

        public Texture2D debug;

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

        public TextureWrapMode wrapMode
        {
            get => m_WrapMode;
            set => m_WrapMode = value;
        }

        public FilterMode filterMode
        {
            get => m_FilterMode;
            set => m_FilterMode = value;
        }

        public int anisoLevel
        {
            get => m_AnisoLevel;
            set => m_AnisoLevel = value;
        }

        public bool isReadable
        {
            get => m_IsReadable;
            set => m_IsReadable = value;
        }

        #region Platform Settings

        [Serializable]
        public class Texture2DArrayImporterPlatformSettings
        {
            public int maxSize = 8192; // max texture size
            public TextureResizeAlgorithm resizeAlgorithm;
            public TextureFormat format;
            public TextureCompressionQuality compression; // compression quality
            public bool isOverrideEnabled;
        }

        // 默认设置
        [SerializeField] private Texture2DArrayImporterPlatformSettings defaultSettings;

        // For Windows, Mac, Linux
        [SerializeField] private Texture2DArrayImporterPlatformSettings platformSettingsForWindows;

        // For Dedicated Server
        [SerializeField] private Texture2DArrayImporterPlatformSettings platformSettingsForDedicatedServer;

        // For Android
        [SerializeField] private Texture2DArrayImporterPlatformSettings platformSettingsForAndroid;

        #endregion

        public override void OnImportAsset(AssetImportContext ctx)
        {
            #region Initial Texture2DArray Null Handling

            if (m_Tex2DArray == null)
            {
                ctx.LogImportWarning("The Initial Texture2DArray asset Can not be null!!!");
                var errorTex2DArray = new Texture2DArray(256, 256, 1,
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
                        Graphics.CopyTexture(errorTexture, 0, errorTex2DArray, n);
                }
                finally
                {
                    DestroyImmediate(errorTexture);
                }

                ctx.AddObjectToAsset("Texture2DArray", errorTex2DArray);
                ctx.SetMainObject(errorTex2DArray);

                return;
            }

            #endregion

            var path = AssetDatabase.GetAssetPath(m_Tex2DArray);
            ctx.DependsOnSourceAsset(path);

            // Process Texture2DArray asset
            var tex2DArray = ProcessTexture2DArrayBySettings();

            ctx.AddObjectToAsset("Texture2DArray", tex2DArray);
            ctx.SetMainObject(tex2DArray);
        }

        private Texture2DArray ProcessTexture2DArrayBySettings()
        {
            var srgb = true;

            var tex2DArray = Texture2DArrayImporterUtil.CloneTexture2DArray(m_Tex2DArray, m_IsReadable, srgb);
            tex2DArray.wrapMode = m_WrapMode;
            tex2DArray.filterMode = m_FilterMode;
            tex2DArray.anisoLevel = m_AnisoLevel;

            tex2DArray =
                Texture2DArrayImporterUtil.CompressTexture2DArray(
                    tex2DArray, TextureFormat.DXT5,
                    TextureCompressionQuality.Normal);

            return tex2DArray;
        }

        [MenuItem("Assets/Create/Texture2D Array", priority = 310)]
        private static void CreateTexture2DArrayMenuItem()
        {
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