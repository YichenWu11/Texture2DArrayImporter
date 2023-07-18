using System;
using UnityEditor;
using UnityEngine;

namespace Util
{
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