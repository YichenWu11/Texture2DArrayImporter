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

        [Tooltip("Enable to be able to access the raw pixel data from code.")] [SerializeField]
        private bool m_IsReadable;

        [Tooltip("Texture2DArray contents is stored in gamma space.")] [SerializeField]
        private bool m_sRGB = true;

        #region Platform Settings

        [Serializable]
        public class Texture2DArrayImporterPlatformSettings
        {
            public int maxSize = 8192; // max texture size
            public TextureResizeAlgorithm resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
            public TextureFormat format = TextureFormat.RGBA32;
            public TextureCompressionQuality compression = TextureCompressionQuality.Normal; // compression quality
            public bool isOverrideEnabled;
        }

        // 默认设置
        [SerializeField] private Texture2DArrayImporterPlatformSettings defaultSettings;

        // For Windows, Mac, Linux
        [SerializeField] private Texture2DArrayImporterPlatformSettings platformSettingsForStandalone; // WIN,OSX,Linux

        // For Dedicated Server
        [SerializeField] private Texture2DArrayImporterPlatformSettings platformSettingsForIOS; // IOS

        // For Android
        [SerializeField] private Texture2DArrayImporterPlatformSettings platformSettingsForAndroid; // Android

        // Helper For Texture2DArrayImporterInspector
        public enum MaxTextureSize
        {
            Size32x32 = 32,
            Size64x64 = 64,
            Size128x128 = 128,
            Size256x256 = 256,
            Size512x512 = 512,
            Size1024x1024 = 1024,
            Size2048x2048 = 2048,
            Size4096x4096 = 4096,
            Size8192x8192 = 8192,
            Size16384x16384 = 16384
        }

        public MaxTextureSize maxSizeDefault = MaxTextureSize.Size8192x8192;
        public TextureResizeAlgorithm resizeAlgorithmDefault = TextureResizeAlgorithm.Mitchell;
        public TextureFormat formatDefault = TextureFormat.RGBA32;
        public TextureCompressionQuality compressionDefault = TextureCompressionQuality.Normal;

        public MaxTextureSize maxSizeStandalone = MaxTextureSize.Size8192x8192;
        public TextureResizeAlgorithm resizeAlgorithmStandalone = TextureResizeAlgorithm.Mitchell;
        public TextureFormat formatStandalone = TextureFormat.RGBA32;
        public TextureCompressionQuality compressionStandalone = TextureCompressionQuality.Normal;
        public bool isOverrideEnabledStandalone = false;

        public MaxTextureSize maxSizeAndroid = MaxTextureSize.Size8192x8192;
        public TextureResizeAlgorithm resizeAlgorithmAndroid = TextureResizeAlgorithm.Mitchell;
        public TextureFormat formatAndroid = TextureFormat.RGBA32;
        public TextureCompressionQuality compressionAndroid = TextureCompressionQuality.Normal;
        public bool isOverrideEnabledAndroid = false;

        public MaxTextureSize maxSizeIOS = MaxTextureSize.Size8192x8192;
        public TextureResizeAlgorithm resizeAlgorithmIOS = TextureResizeAlgorithm.Mitchell;
        public TextureFormat formatIOS = TextureFormat.RGBA32;
        public TextureCompressionQuality compressionIOS = TextureCompressionQuality.Normal;
        public bool isOverrideEnabledIOS = false;

        #endregion

        private void ConfigurePlatformSettings()
        {
            defaultSettings.maxSize = (int)maxSizeDefault;
            defaultSettings.resizeAlgorithm = resizeAlgorithmDefault;
            defaultSettings.format = formatDefault;
            defaultSettings.compression = compressionDefault;

            platformSettingsForStandalone.maxSize = (int)maxSizeStandalone;
            platformSettingsForStandalone.resizeAlgorithm = resizeAlgorithmStandalone;
            platformSettingsForStandalone.format = formatStandalone;
            platformSettingsForStandalone.compression = compressionStandalone;
            platformSettingsForStandalone.isOverrideEnabled = isOverrideEnabledStandalone;

            platformSettingsForAndroid.maxSize = (int)maxSizeAndroid;
            platformSettingsForAndroid.resizeAlgorithm = resizeAlgorithmAndroid;
            platformSettingsForAndroid.format = formatAndroid;
            platformSettingsForAndroid.compression = compressionAndroid;
            platformSettingsForAndroid.isOverrideEnabled = isOverrideEnabledAndroid;

            platformSettingsForIOS.maxSize = (int)maxSizeIOS;
            platformSettingsForIOS.resizeAlgorithm = resizeAlgorithmIOS;
            platformSettingsForIOS.format = formatIOS;
            platformSettingsForIOS.compression = compressionIOS;
            platformSettingsForIOS.isOverrideEnabled = isOverrideEnabledIOS;
        }

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

                    for (var n = 0; n < errorTex2DArray.depth; ++n)
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

            ConfigurePlatformSettings();
            // Process Texture2DArray asset
            var tex2DArray = ProcessTexture2DArrayBySettings();

            ctx.AddObjectToAsset("Texture2DArray", tex2DArray);
            ctx.SetMainObject(tex2DArray);
        }

        private Texture2DArray ProcessTexture2DArrayBySettings()
        {
            var maxSize = defaultSettings.maxSize;
            var resizeAlgo = defaultSettings.resizeAlgorithm;
            var format = defaultSettings.format;
            var compression = defaultSettings.compression;

#if UNITY_STANDALONE
            if (platformSettingsForStandalone.isOverrideEnabled)
            {
                maxSize = platformSettingsForStandalone.maxSize;
                resizeAlgo = platformSettingsForStandalone.resizeAlgorithm;
                format = platformSettingsForStandalone.format;
                compression = platformSettingsForStandalone.compression;
            }
#elif UNITY_ANDROID
            if (platformSettingsForAndroid.isOverrideEnabled)
            {
                maxSize = platformSettingsForAndroid.maxSize;
                resizeAlgo = platformSettingsForAndroid.resizeAlgorithm;
                format = platformSettingsForAndroid.format;
                compression = platformSettingsForAndroid.compression;
            }
#elif UNITY_IOS
            if (platformSettingsForIOS.isOverrideEnabled)
            {
                maxSize = platformSettingsForIOS.maxSize;
                resizeAlgo = platformSettingsForIOS.resizeAlgorithm;
                format = platformSettingsForIOS.format;
                compression = platformSettingsForIOS.compression;
            }
#endif

            var tex2DArray =
                Texture2DArrayImporterUtil.ProcessTexture2DArray(
                    m_Tex2DArray, format, compression, maxSize, resizeAlgo, m_sRGB);
            tex2DArray.wrapMode = m_WrapMode;
            tex2DArray.filterMode = m_FilterMode;
            tex2DArray.anisoLevel = m_AnisoLevel;

            tex2DArray.name = "MyTestArrayForImporter";

            tex2DArray.Apply(false, !m_IsReadable);

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

    public static class Texture2DArrayImporterUtil
    {
        public static Texture2DArray ProcessTexture2DArray(
            Texture2DArray sourceArray,
            TextureFormat format,
            TextureCompressionQuality compressionQuality,
            int maxSize,
            TextureResizeAlgorithm resizeAlgo,
            bool srgb)
        {
            Texture2DArray tex2DArray = null;
            // Resize
            if (sourceArray.width > maxSize || sourceArray.height > maxSize)
            {
                if (IsCompressedFormat(sourceArray.format))
                {
                    // 如果是压缩格式，先解压成 RGBA32，再做 Resize 操作，然后再压缩回原来的格式
                    tex2DArray = CompressTexture2DArray(
                        sourceArray, TextureFormat.RGBA32, compressionQuality, srgb);
                    tex2DArray = ResizeTexture2DArray(
                        tex2DArray, maxSize, maxSize, resizeAlgo, srgb);
                    tex2DArray = CompressTexture2DArray(
                        tex2DArray, sourceArray.format, compressionQuality, srgb);
                }
                else
                {
                    tex2DArray = ResizeTexture2DArray(
                        sourceArray, maxSize, maxSize, resizeAlgo, srgb);
                }
            }

            // Compress
            tex2DArray = CompressTexture2DArray(
                tex2DArray == null ? sourceArray : tex2DArray, format, compressionQuality, srgb);
            return tex2DArray;
        }

        private static Texture2DArray CompressTexture2DArray(
            Texture2DArray sourceArray,
            TextureFormat format,
            TextureCompressionQuality compressionQuality,
            bool srgb)
        {
            var width = sourceArray.width;
            var height = sourceArray.height;
            var depth = sourceArray.depth;
            var mipmapEnabled = sourceArray.mipmapCount > 1;

            var newArray = new Texture2DArray(width, height, depth, format, mipmapEnabled, !srgb);

            for (var i = 0; i < depth; i++)
            for (var mip = 0; mip < sourceArray.mipmapCount; mip++)
            {
                var sourceMipTexture = sourceArray.ToTexture2D(i, srgb);
                EditorUtility.CompressTexture(sourceMipTexture, format, compressionQuality);
                Graphics.CopyTexture(sourceMipTexture, 0, newArray, i);
            }

            return newArray;
        }

        private static Texture2DArray ResizeTexture2DArray(
            Texture2DArray sourceArray,
            int width, int height,
            TextureResizeAlgorithm resizeAlgo,
            bool srgb)
        {
            var depth = sourceArray.depth;
            var mipmapEnabled = sourceArray.mipmapCount > 1;

            var newArray = new Texture2DArray(width, height, depth, sourceArray.format, mipmapEnabled, !srgb);

            for (var i = 0; i < depth; i++)
            {
                var sourceTexture = sourceArray.ToTexture2D(i, srgb);
                var resizedTexture = ResizeTexture(sourceTexture, width, height, resizeAlgo, srgb);
                Graphics.CopyTexture(resizedTexture, 0, newArray, i);
            }

            return newArray;
        }

        #region Resize Algorithm

        private static Texture2D ResizeTexture(
            Texture2D sourceTexture,
            int width, int height,
            TextureResizeAlgorithm resizeAlgo, bool srgb)
        {
            return resizeAlgo switch
            {
                TextureResizeAlgorithm.Mitchell => ResizeTextureWithMitchell(sourceTexture, width, height, srgb),
                TextureResizeAlgorithm.Bilinear => ResizeTextureWithBilinear(sourceTexture, width, height, srgb),
                _ => throw new ArgumentOutOfRangeException(nameof(resizeAlgo), resizeAlgo, null)
            };
        }

        private static Texture2D ResizeTextureWithBilinear(
            Texture2D sourceTexture,
            int width, int height,
            bool srgb)
        {
            var mipmapEnabled = sourceTexture.mipmapCount > 1;

            var rtTemp = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height);
            Graphics.Blit(sourceTexture, rtTemp);

            var rt = RenderTexture.GetTemporary(width, height);
            rt.filterMode = sourceTexture.filterMode;

            Graphics.Blit(rtTemp, rt);

            var resizedTexture = new Texture2D(width, height, sourceTexture.format, mipmapEnabled, !srgb);
            RenderTexture.active = rt;
            resizedTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            resizedTexture.Apply();

            RenderTexture.ReleaseTemporary(rtTemp);
            RenderTexture.ReleaseTemporary(rt);
            RenderTexture.active = null;

            return resizedTexture;
        }

        private static Texture2D ResizeTextureWithMitchell(
            Texture2D sourceTexture,
            int width, int height,
            bool srgb)
        {
            // Create a new Texture2D with mipmaps
            var mipmapEnabled = sourceTexture.mipmapCount > 1;
            var resizedTexture = new Texture2D(width, height, sourceTexture.format, mipmapEnabled, !srgb);

            var mipCount = Mathf.Min(sourceTexture.mipmapCount, resizedTexture.mipmapCount);

            // Mitchell filter parameters
            var B = 1 / 3f;
            var C = 1 / 3f;

            for (var mip = 0; mip < mipCount; mip++)
            {
                var mipWidth = Mathf.Max(width >> mip, 1);
                var mipHeight = Mathf.Max(height >> mip, 1);

                var scaleX = (float)sourceTexture.width / mipWidth;
                var scaleY = (float)sourceTexture.height / mipHeight;

                var sourceMipPixels = sourceTexture.GetPixels(mip);
                var resizedMipPixels = new Color[mipWidth * mipHeight];

                for (var y = 0; y < mipHeight; y++)
                for (var x = 0; x < mipWidth; x++)
                {
                    var u = (x + 0.5f) * scaleX - 0.5f;
                    var v = (y + 0.5f) * scaleY - 0.5f;

                    var pixelColor = SampleWithMitchell(sourceTexture, u, v, B, C);
                    resizedMipPixels[y * mipWidth + x] = pixelColor;
                }

                resizedTexture.SetPixels(resizedMipPixels, mip);
            }

            return resizedTexture;
        }

        private static Color SampleWithMitchell(Texture2D texture, float u, float v, float B, float C)
        {
            var x1 = Mathf.FloorToInt(u);
            var y1 = Mathf.FloorToInt(v);

            var result = Color.clear;

            for (var j = -1; j <= 2; j++)
            {
                var y = y1 + j;
                for (var i = -1; i <= 2; i++)
                {
                    var x = x1 + i;
                    var pixel = texture.GetPixel(Mathf.Clamp(x, 0, texture.width - 1),
                        Mathf.Clamp(y, 0, texture.height - 1));
                    var weight = Mitchell1D(u - x, B, C) * Mitchell1D(v - y, B, C);
                    result += pixel * weight;
                }
            }

            return result;
        }

        private static float Mitchell1D(float x, float B, float C)
        {
            x = Mathf.Abs(x);

            return x switch
            {
                < 1 => ((12 - 9 * B - 6 * C) * x * x * x + (-18 + 12 * B + 6 * C) * x * x + (6 - 2 * B)) / 6,
                < 2 => ((-B - 6 * C) * x * x * x + (6 * B + 30 * C) * x * x + (-12 * B - 48 * C) * x +
                        (8 * B + 24 * C)) / 6,
                _ => 0
            };
        }

        #endregion

        private static bool IsCompressedFormat(TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.DXT1:
                case TextureFormat.DXT5:
                case TextureFormat.BC4:
                case TextureFormat.BC5:
                case TextureFormat.BC6H:
                case TextureFormat.BC7:
                case TextureFormat.RGBA4444:
                case TextureFormat.RGBA32:
                case TextureFormat.PVRTC_RGB2:
                case TextureFormat.PVRTC_RGBA2:
                case TextureFormat.PVRTC_RGB4:
                case TextureFormat.PVRTC_RGBA4:
                case TextureFormat.ETC_RGB4:
                case TextureFormat.EAC_R:
                case TextureFormat.EAC_R_SIGNED:
                case TextureFormat.EAC_RG:
                case TextureFormat.EAC_RG_SIGNED:
                case TextureFormat.ETC2_RGB:
                case TextureFormat.ETC2_RGBA1:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.ASTC_4x4:
                case TextureFormat.ASTC_5x5:
                case TextureFormat.ASTC_6x6:
                case TextureFormat.ASTC_8x8:
                case TextureFormat.ASTC_10x10:
                case TextureFormat.ASTC_12x12:
                    return true;

                default:
                    return false;
            }
        }
    }

    public static class Texture2DArrayExtensions
    {
        public static Texture2D ToTexture2D(this Texture2DArray textureArray, int index, bool srgb)
        {
            var texture = new Texture2D(textureArray.width, textureArray.height, textureArray.format,
                textureArray.mipmapCount > 1, !srgb);
            Graphics.CopyTexture(textureArray, index, texture, 0);
            return texture;
        }
    }
}