using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Texture2DArray mips;
    public Texture2D[] textures;
    public Texture2D debug;

    private void Start()
    {
        LoadTexture2DArray();
    }

    private void LoadTexture2DArray()
    {
        if (textures.Length == 0)
        {
            Debug.LogError("No textures provided.");
            return;
        }

        int width = textures[0].width;
        int height = textures[0].height;
        TextureFormat format = textures[0].format;

        mips = new Texture2DArray(width, height, textures.Length, format, true);
        for (int i = 0; i < textures.Length; i++)
        {
            if (textures[i].width != width || textures[i].height != height || textures[i].format != format)
            {
                Debug.LogError("All textures must have the same dimensions and format.");
                return;
            }

            mips.SetPixels(textures[i].GetPixels(), i);
        }

        mips.Apply();

        var outputPath = "Assets/Textures/1.asset";
        AssetDatabase.CreateAsset(mips, outputPath);
    }
}
