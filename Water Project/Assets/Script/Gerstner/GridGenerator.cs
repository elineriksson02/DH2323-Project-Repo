using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

public class GridGenerator : MonoBehaviour
{
    public int gridSize = 64; 
    public float cellSize = 0.5f; 

    void Awake()
    {
        GenerateMesh();
    }
    
    void GenerateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        Vector3[] vertices = new Vector3[(gridSize + 1) * (gridSize + 1)];
        int[] triangles = new int[gridSize * gridSize * 6];

        for (int i = 0, z = 0; z <= gridSize; z++)
        {
            for (int x = 0; x <= gridSize; x++)
            {
                vertices[i] = new Vector3(x * cellSize, 0, z * cellSize);
                i++;
            }
        }

        int vert = 0;
        int tris = 0;
        for (int z = 0; z < gridSize; z++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + gridSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + gridSize + 1;
                triangles[tris + 5] = vert + gridSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
    }
}


