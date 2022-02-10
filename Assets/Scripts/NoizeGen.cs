using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoizeGen : MonoBehaviour
{
    [Range(8, 256)]
    [SerializeField] private int textureSize;

    [SerializeField] private int step = 100;

    [SerializeField] private float minHeight;
    [SerializeField] private float maxHeight;


    [SerializeField] private float xOrg = 0;
    [SerializeField] private float yOrg = 0;
    [SerializeField] private float scale = 0.1f;

    [Range(1, 100)]
    [SerializeField] private int mapQuality = 100;



    [SerializeField] private GameObject go;
    [SerializeField] private Material mat;


    private int TextureSize => (int)textureSize / mapQuality;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            go.GetComponent<MeshFilter>().sharedMesh = BuildMesh();
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            //StartCoroutine("BuildTris", Vertices());
        }
    }





    private Vector3[] Vertices()
    {
        Texture2D texture = GenerateNoize();
        List<Vector3> verts = new List<Vector3>();
        int order = 0;
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                if (order % mapQuality != 0)
                {
                    continue;
                }
                float texValue = texture.GetPixel(x, y).grayscale * (maxHeight - minHeight);
                verts.Add(new Vector3(y * step, texValue, x * step));
                order++;
            }
        }

        return verts.ToArray();
    }


    private Mesh BuildMesh()
    {
        Vector3[] verts = Vertices();
        Mesh meshFinal = new Mesh();
        meshFinal.vertices = verts;
        meshFinal.triangles = Triangulate(verts);
        meshFinal.uv = UVs(verts);
        meshFinal.RecalculateNormals();
        meshFinal.RecalculateBounds();

        return meshFinal;
    }

    private Vector2[] UVs(Vector3[] verts)
    {
        Vector2[] uvs = new Vector2[verts.Length];

        float xOffset = 0;
        float yOffset = 0;
        int yCount = 0;
        int xCount = 0;
        for (int i = 0; i < verts.Length; i++)
        {
            if (i != 0 && (i + 1) % textureSize == 0)
            {
                xOffset = 0;
                yCount++;
                xCount = 0;
                yOffset += yCount / textureSize - 1;
            }
            uvs[i] = new Vector2(xOffset, yOffset);
            xCount++;
            xOffset += xCount / textureSize - 1;
        }


        return uvs;
    }
    private int[] Triangulate(Vector3[] verts)
    {
        List<int> trisList = new List<int>();

        for (int i = 0; i < verts.Length; i++)
        {
            //Build first tris
            if (i < verts.Length - textureSize)
            {
                if ((i + 1) % textureSize == 0 && i != 0)
                {
                    continue;
                }
                trisList.Add(i);
                trisList.Add(i + 1);
                int nextRowIndex = i + textureSize;
                trisList.Add(nextRowIndex);
                //build flipped tris
                trisList.Add(i + 1);
                trisList.Add(nextRowIndex + 1);
                trisList.Add(nextRowIndex);
            }
            else
            {
                break;
            }
        }
        return trisList.ToArray();
    }
    private Texture2D GenerateNoize()
    {
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, 0, true);

        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                float xCoord = xOrg + (float)x / (float)texture.width * scale;
                float yCoord = yOrg + (float)y / (float)texture.height * scale;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                Color pointColor = new Color(sample, sample, sample);
                texture.SetPixel(x, y, pointColor);
            }
        }
        texture.Apply();
        return texture;
    }



}
