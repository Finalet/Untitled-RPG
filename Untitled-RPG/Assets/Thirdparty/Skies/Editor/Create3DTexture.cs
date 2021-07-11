using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Create3DTexture : MonoBehaviour
{
    [MenuItem("Assets/Create/Skies/Create 3D Noise Texture (32px)")]
    static void Make3DTexture32(MenuCommand command)
    {
        
        int size = 32;
        CreateTex(size);
    }

    [MenuItem("Assets/Create/Skies/Create 3D Noise Texture (128px)")]
    static void Make3DTexture128(MenuCommand command)
    {
        int size = 128;
        CreateTex(size);
    }

    static void CreateTex(int size)
    {
        Texture3D newTexture3D = new Texture3D(size, size, size, TextureFormat.ARGB32, true);

        List<Color> colors = new List<Color>();
        for (int z = 0; z < size; z++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int valueR = Random.Range(0f, 1f) > 0.3 ? 0 : 1;
                    int valueG = Random.Range(0f, 1f) > 0.5 ? 0 : 1;
                    int valueB = Random.Range(0f, 1f) > 0.7 ? 0 : 1;
                    Color newColor = new Color(valueR, valueG, valueB);
                    colors.Add(newColor);
                }
            }
        }

        newTexture3D.SetPixels(colors.ToArray());
        newTexture3D.Apply();
        ProjectWindowUtil.CreateAsset(newTexture3D, "New Cloud Texture.asset");
    }
}
