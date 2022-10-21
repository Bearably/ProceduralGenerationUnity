using UnityEngine;

public class Displacement : MonoBehaviour
{
    public int resolution = 256;
    private Texture2D texture;
    private GameObject DisplaceMesh;
    static public bool meshDisplaced = false;


    private void Displace()
    {
        DisplaceMesh = GameObject.Find("TriangleMesh");
        texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, true);
        texture.name = "Procedural Texture";
        DisplaceMesh.GetComponent<MeshRenderer>().material.mainTexture = texture;
        FillTexture();
    }
    private void FillTexture()
    {
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                texture.SetPixel(x, y, Color.red);
            }
        }
        texture.Apply();
    }
}

