using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Util
{
    public static class Texture2DArrayImporterUtil
    {
        public static Texture2DArray CompressTexture2DArray(
            Texture2DArray sourceArray,
            TextureFormat format,
            TextureCompressionQuality compressionQuality)
        {
            var width = sourceArray.width;
            var height = sourceArray.height;
            var depth = sourceArray.depth;

            // Create a new Texture2DArray with the target format
            var newArray = new Texture2DArray(width, height, depth, format, sourceArray.mipmapCount > 1);

            for (var i = 0; i < depth; i++)
            for (var mip = 0; mip < sourceArray.mipmapCount; mip++)
            {
                var sourceMipTexture = sourceArray.ToTexture2D(i);
                EditorUtility.CompressTexture(sourceMipTexture, format, compressionQuality);
                Graphics.CopyTexture(sourceMipTexture, 0, newArray, i);
            }

            return newArray;
        }

        #region Clone Texture2DArray

        public static Texture2DArray CloneTexture2DArray(
            Texture2DArray sourceArray,
            bool isReadable = true,
            bool srgbTexture = true)
        {
            var width = sourceArray.width;
            var height = sourceArray.height;
            var depth = sourceArray.depth;
            var format = sourceArray.format;
            var mipmapEnabled = sourceArray.mipmapCount > 1;

            // Create a new Texture2DArray with the same dimensions and format as the source array
            var newArray = new Texture2DArray(width, height, depth, format, mipmapEnabled, !srgbTexture);

            // Copy the textures from the source array to the new array
            for (var i = 0; i < depth; i++)
            {
                Graphics.CopyTexture(sourceArray, i, 0, newArray, i, 0);
                if (!mipmapEnabled) continue;
                for (var mip = 1; mip < sourceArray.mipmapCount; ++mip)
                    Graphics.CopyTexture(sourceArray, i, mip, newArray, i, mip);
            }

            // Apply the changes
            newArray.Apply(mipmapEnabled, isReadable);

            return newArray;
        }

        #endregion

        #region Resize Algorithm

        public static Texture2D ResizeTexture(
            Texture2D sourceTexture,
            int width, int height,
            TextureResizeAlgorithm resizeAlgo)
        {
            return resizeAlgo switch
            {
                TextureResizeAlgorithm.Mitchell => ResizeTextureWithMitchell(sourceTexture, width, height),
                TextureResizeAlgorithm.Bilinear => ResizeTextureWithBilinear(sourceTexture, width, height),
                _ => throw new ArgumentOutOfRangeException(nameof(resizeAlgo), resizeAlgo, null)
            };
        }

        private static Texture2D ResizeTextureWithBilinear(Texture2D sourceTexture, int width, int height)
        {
            // Create a new Texture2D with mipmaps
            var resizedTexture = new Texture2D(width, height, sourceTexture.format, sourceTexture.mipmapCount > 1);

            var mipCount = Mathf.Min(sourceTexture.mipmapCount, resizedTexture.mipmapCount);

            for (var mip = 0; mip < mipCount; mip++)
            {
                var mipWidth = Mathf.Max(width >> mip, 1);
                var mipHeight = Mathf.Max(height >> mip, 1);

                var sourceMipWidth = Mathf.Max(sourceTexture.width >> mip, 1);
                var sourceMipHeight = Mathf.Max(sourceTexture.height >> mip, 1);

                var scaleX = (float)sourceTexture.width / mipWidth;
                var scaleY = (float)sourceTexture.height / mipHeight;

                var sourceMipPixels = sourceTexture.GetPixels(mip);
                var resizedMipPixels = new Color[mipWidth * mipHeight];

                for (var y = 0; y < mipHeight; y++)
                for (var x = 0; x < mipWidth; x++)
                {
                    var u = (x + 0.5f) * scaleX - 0.5f;
                    var v = (y + 0.5f) * scaleY - 0.5f;

                    var x1 = Mathf.Clamp(Mathf.FloorToInt(u), 0, sourceTexture.width - 1);
                    var x2 = Mathf.Clamp(Mathf.CeilToInt(u), 0, sourceTexture.width - 1);
                    var y1 = Mathf.Clamp(Mathf.FloorToInt(v), 0, sourceTexture.height - 1);
                    var y2 = Mathf.Clamp(Mathf.CeilToInt(v), 0, sourceTexture.height - 1);

                    var p1 = GetPixel(sourceMipPixels, Mathf.Clamp(x1, 0, sourceMipWidth - 1),
                        Mathf.Clamp(y1, 0, sourceMipHeight - 1), sourceMipWidth);
                    var p2 = GetPixel(sourceMipPixels, Mathf.Clamp(x2, 0, sourceMipWidth - 1),
                        Mathf.Clamp(y1, 0, sourceMipHeight - 1), sourceMipWidth);
                    var p3 = GetPixel(sourceMipPixels, Mathf.Clamp(x1, 0, sourceMipWidth - 1),
                        Mathf.Clamp(y2, 0, sourceMipHeight - 1), sourceMipWidth);
                    var p4 = GetPixel(sourceMipPixels, Mathf.Clamp(x2, 0, sourceMipWidth - 1),
                        Mathf.Clamp(y2, 0, sourceMipHeight - 1), sourceMipWidth);

                    var dx = u - x1;
                    var dy = v - y1;

                    var pixelColor = Color.Lerp(Color.Lerp(p1, p2, dx), Color.Lerp(p3, p4, dx), dy);
                    resizedMipPixels[y * mipWidth + x] = pixelColor;
                }

                resizedTexture.SetPixels(resizedMipPixels, mip);
            }

            resizedTexture.Apply();
            return resizedTexture;
        }

        private static Color GetPixel(Color[] pixels, int x, int y, int width)
        {
            return pixels[y * width + x];
        }

        private static Texture2D ResizeTextureWithMitchell(Texture2D sourceTexture, int width, int height)
        {
            // Create a new Texture2D with mipmaps
            var resizedTexture = new Texture2D(width, height, sourceTexture.format, sourceTexture.mipmapCount > 1);

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

            resizedTexture.Apply();
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
    }

    public static class Texture2DArrayExtensions
    {
        public static Texture2D ToTexture2D(this Texture2DArray textureArray, int index)
        {
            var width = Mathf.Max(textureArray.width, 1);
            var height = Mathf.Max(textureArray.height, 1);
            var format = textureArray.format;

            var texture = new Texture2D(width, height, format, textureArray.mipmapCount > 1);
            Graphics.CopyTexture(textureArray, index, texture, 0);

            return texture;
        }
    }
}