using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Texture2DArray mips;
    public Texture2DArray debugArray;
    public Texture2D[] textures;
    public Texture2D debug;

    private void Start()
    {
        Debug.Log(debugArray.format);
        // LoadTexture2DArray();
        // Test Resize
        // debug = ResizeTextureWithBilinear(textures[0], textures[0].width / 4, textures[0].height / 4);
        // debug = ResizeTextureWithMitchell(textures[0], textures[0].width / 4, textures[0].height / 4);
        // Test Convert Texture Format
        // debug = ConvertTextureToFormat(textures[0], GraphicsFormat.R8G8B8A8_SRGB);
        // mips = ConvertTextureArrayToFormat(mips, GraphicsFormat.R8G8B8A8_SRGB);
        // Debug.Log(debugArray.isReadable ? "Readable" : "Not Readable");
        // Debug.Log(debugArray.anisoLevel);
        // Debug.Log(debugArray.wrapMode);
        // Debug.Log(debugArray.filterMode);
    }

    // private Texture2DArray ConvertTextureArrayToFormat(Texture2DArray sourceArray, GraphicsFormat format)
    // {
    //     var width = sourceArray.width;
    //     var height = sourceArray.height;
    //     var depth = sourceArray.depth;
    //     var mipCount = sourceArray.mipmapCount;
    //
    //     // Create a new Texture2DArray with the target format and mipmaps
    //     var creationFlags = sourceArray.mipmapCount > 1 ? TextureCreationFlags.MipChain : TextureCreationFlags.None;
    //     var newArray = new Texture2DArray(width, height, depth, format, creationFlags);
    //
    //     // Copy the textures from the source array to the new array
    //     for (var i = 0; i < depth; i++)
    //     {
    //         var sourceTexture = sourceArray.ToTexture2D(i);
    //
    //         for (var mip = 0; mip < mipCount; mip++)
    //         {
    //             var mipWidth = Mathf.Max(width >> mip, 1);
    //             var mipHeight = Mathf.Max(height >> mip, 1);
    //
    //             var convertedMipTexture = ConvertTextureToFormat(sourceTexture, format, mipWidth, mipHeight);
    //             Graphics.CopyTexture(convertedMipTexture, 0, 0, newArray, i, mip);
    //         }
    //     }
    //
    //     return newArray;
    // }
    //
    // private Texture2D ConvertTextureToFormat(Texture2D sourceTexture, GraphicsFormat format)
    // {
    //     var width = sourceTexture.width;
    //     var height = sourceTexture.height;
    //
    //     return ConvertTextureToFormat(sourceTexture, format, width, height);
    // }
    //
    // private Texture2D ConvertTextureToFormat(Texture2D sourceTexture, GraphicsFormat format, int width, int height)
    // {
    //     // Create a new Texture2D with the target format
    //     var creationFlags = sourceTexture.mipmapCount > 1 ? TextureCreationFlags.MipChain : TextureCreationFlags.None;
    //     var convertedTexture = new Texture2D(width, height, format, creationFlags);
    //     // Get the source texture pixels
    //     var sourcePixels = sourceTexture.GetPixels();
    //
    //     // Set the pixels of the converted texture
    //     convertedTexture.SetPixels(sourcePixels);
    //     convertedTexture.Apply();
    //
    //     return convertedTexture;
    // }

    // private void LoadTexture2DArray()
    // {
    //     if (textures.Length == 0)
    //     {
    //         Debug.LogError("No textures provided.");
    //         return;
    //     }
    //
    //     var width = textures[0].width;
    //     var height = textures[0].height;
    //     var format = textures[0].format;
    //     var mipmapEnabled = textures[0].mipmapCount > 1;
    //
    //     mips = new Texture2DArray(width, height, textures.Length, format, mipmapEnabled);
    //     for (var i = 0; i < textures.Length; i++)
    //     {
    //         if (textures[i].width != width || textures[i].height != height || textures[i].format != format)
    //         {
    //             Debug.LogError("All textures must have the same dimensions and format.");
    //             return;
    //         }
    //
    //         mips.SetPixels(textures[i].GetPixels(), i);
    //     }
    //
    //     mips.Apply();
    //
    //     var outputPath = "Assets/Textures/2.asset";
    //     AssetDatabase.CreateAsset(mips, outputPath);
    // }
    //
    // #region Resize Algorithm
    //
    // // Resize Algorithm
    // public static Texture2D ResizeTexture(
    //     Texture2D sourceTexture,
    //     int width, int height,
    //     TextureResizeAlgorithm resizeAlgo)
    // {
    //     return resizeAlgo switch
    //     {
    //         TextureResizeAlgorithm.Mitchell => ResizeTextureWithMitchell(sourceTexture, width, height),
    //         TextureResizeAlgorithm.Bilinear => ResizeTextureWithBilinear(sourceTexture, width, height),
    //         _ => throw new ArgumentOutOfRangeException(nameof(resizeAlgo), resizeAlgo, null)
    //     };
    // }
    //
    // private static Texture2D ResizeTextureWithBilinear(Texture2D sourceTexture, int width, int height)
    // {
    //     // Create a new Texture2D with mipmaps
    //     var resizedTexture = new Texture2D(width, height, sourceTexture.format, sourceTexture.mipmapCount > 1);
    //
    //     var mipCount = Mathf.Min(sourceTexture.mipmapCount, resizedTexture.mipmapCount);
    //
    //     for (var mip = 0; mip < mipCount; mip++)
    //     {
    //         var mipWidth = Mathf.Max(width >> mip, 1);
    //         var mipHeight = Mathf.Max(height >> mip, 1);
    //
    //         var sourceMipWidth = Mathf.Max(sourceTexture.width >> mip, 1);
    //         var sourceMipHeight = Mathf.Max(sourceTexture.height >> mip, 1);
    //
    //         var scaleX = (float)sourceTexture.width / mipWidth;
    //         var scaleY = (float)sourceTexture.height / mipHeight;
    //
    //         var sourceMipPixels = sourceTexture.GetPixels(mip);
    //         var resizedMipPixels = new Color[mipWidth * mipHeight];
    //
    //         for (var y = 0; y < mipHeight; y++)
    //         for (var x = 0; x < mipWidth; x++)
    //         {
    //             var u = (x + 0.5f) * scaleX - 0.5f;
    //             var v = (y + 0.5f) * scaleY - 0.5f;
    //
    //             var x1 = Mathf.Clamp(Mathf.FloorToInt(u), 0, sourceTexture.width - 1);
    //             var x2 = Mathf.Clamp(Mathf.CeilToInt(u), 0, sourceTexture.width - 1);
    //             var y1 = Mathf.Clamp(Mathf.FloorToInt(v), 0, sourceTexture.height - 1);
    //             var y2 = Mathf.Clamp(Mathf.CeilToInt(v), 0, sourceTexture.height - 1);
    //
    //             var p1 = GetPixel(sourceMipPixels, Mathf.Clamp(x1, 0, sourceMipWidth - 1),
    //                 Mathf.Clamp(y1, 0, sourceMipHeight - 1), sourceMipWidth);
    //             var p2 = GetPixel(sourceMipPixels, Mathf.Clamp(x2, 0, sourceMipWidth - 1),
    //                 Mathf.Clamp(y1, 0, sourceMipHeight - 1), sourceMipWidth);
    //             var p3 = GetPixel(sourceMipPixels, Mathf.Clamp(x1, 0, sourceMipWidth - 1),
    //                 Mathf.Clamp(y2, 0, sourceMipHeight - 1), sourceMipWidth);
    //             var p4 = GetPixel(sourceMipPixels, Mathf.Clamp(x2, 0, sourceMipWidth - 1),
    //                 Mathf.Clamp(y2, 0, sourceMipHeight - 1), sourceMipWidth);
    //
    //             var dx = u - x1;
    //             var dy = v - y1;
    //
    //             var pixelColor = Color.Lerp(Color.Lerp(p1, p2, dx), Color.Lerp(p3, p4, dx), dy);
    //             resizedMipPixels[y * mipWidth + x] = pixelColor;
    //         }
    //
    //         resizedTexture.SetPixels(resizedMipPixels, mip);
    //     }
    //
    //     resizedTexture.Apply();
    //     return resizedTexture;
    // }
    //
    // private static Color GetPixel(Color[] pixels, int x, int y, int width)
    // {
    //     return pixels[y * width + x];
    // }
    //
    // private static Texture2D ResizeTextureWithMitchell(Texture2D sourceTexture, int width, int height)
    // {
    //     // Create a new Texture2D with mipmaps
    //     var resizedTexture = new Texture2D(width, height, sourceTexture.format, sourceTexture.mipmapCount > 1);
    //
    //     var mipCount = Mathf.Min(sourceTexture.mipmapCount, resizedTexture.mipmapCount);
    //
    //     // Mitchell filter parameters
    //     var B = 1 / 3f;
    //     var C = 1 / 3f;
    //
    //     for (var mip = 0; mip < mipCount; mip++)
    //     {
    //         var mipWidth = Mathf.Max(width >> mip, 1);
    //         var mipHeight = Mathf.Max(height >> mip, 1);
    //
    //         var scaleX = (float)sourceTexture.width / mipWidth;
    //         var scaleY = (float)sourceTexture.height / mipHeight;
    //
    //         var sourceMipPixels = sourceTexture.GetPixels(mip);
    //         var resizedMipPixels = new Color[mipWidth * mipHeight];
    //
    //         for (var y = 0; y < mipHeight; y++)
    //         for (var x = 0; x < mipWidth; x++)
    //         {
    //             var u = (x + 0.5f) * scaleX - 0.5f;
    //             var v = (y + 0.5f) * scaleY - 0.5f;
    //
    //             var pixelColor = SampleWithMitchell(sourceTexture, u, v, B, C);
    //             resizedMipPixels[y * mipWidth + x] = pixelColor;
    //         }
    //
    //         resizedTexture.SetPixels(resizedMipPixels, mip);
    //     }
    //
    //     resizedTexture.Apply();
    //     return resizedTexture;
    // }
    //
    // private static Color SampleWithMitchell(Texture2D texture, float u, float v, float B, float C)
    // {
    //     var x1 = Mathf.FloorToInt(u);
    //     var y1 = Mathf.FloorToInt(v);
    //
    //     var result = Color.clear;
    //
    //     for (var j = -1; j <= 2; j++)
    //     {
    //         var y = y1 + j;
    //
    //         for (var i = -1; i <= 2; i++)
    //         {
    //             var x = x1 + i;
    //
    //             var pixel = texture.GetPixel(Mathf.Clamp(x, 0, texture.width - 1),
    //                 Mathf.Clamp(y, 0, texture.height - 1));
    //             var weight = Mitchell1D(u - x, B, C) * Mitchell1D(v - y, B, C);
    //
    //             result += pixel * weight;
    //         }
    //     }
    //
    //     return result;
    // }
    //
    // private static float Mitchell1D(float x, float B, float C)
    // {
    //     x = Mathf.Abs(x);
    //
    //     if (x < 1)
    //         return ((12 - 9 * B - 6 * C) * x * x * x + (-18 + 12 * B + 6 * C) * x * x + (6 - 2 * B)) / 6;
    //     else if (x < 2)
    //         return ((-B - 6 * C) * x * x * x + (6 * B + 30 * C) * x * x + (-12 * B - 48 * C) * x + (8 * B + 24 * C)) /
    //                6;
    //
    //     return 0;
    // }

    // #endregion
}