using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridGeneratorSWE : MonoBehaviour
{
    public int gridSize = 64;
    public float cellSize = 0.5f;
    public float gravity = 9.81f;
    public float restDepth = 2.0f;

    // water simulation values
    private float[] h;  // Water depth at cell centers 
    private float[] H;  // Terrain height at cell centers 
    private float[] u;  // X-velocity on vertical faces 
    private float[] w;  // Z-velocity on horizontal faces 

    private Mesh waterMesh;
    private Vector3[] vertices; 

    int C(int i, int j) => j * gridSize + i;
    int U(int i, int j) => j * (gridSize + 1) + i;
    int W(int i, int j) => j * gridSize + i;

    void Start()
    {
        // initialize arrays
        h = new float[gridSize * gridSize];
        H = new float[gridSize * gridSize];
        u = new float[(gridSize + 1) * gridSize];
        w = new float[gridSize * (gridSize + 1)];

        for (int i = 0; i < h.Length; i++) h[i] = restDepth;

        // add wave in center
        ApplyDisturbance(gridSize / 2, gridSize / 2, 1.0f);

        GenerateMesh(); 
    }

    void Update()
    {
        // split into smaller steps for stability
        float dt = Time.deltaTime;
        float maxWaveSpeed = Mathf.Sqrt(gravity * restDepth);
        float stableDt = 0.4f * cellSize / maxWaveSpeed;
        int substeps = Mathf.CeilToInt(dt / stableDt);
        float subDt = dt / substeps;

        for (int s = 0; s < substeps; s++)
            StepSWE(subDt);

        UpdateMesh();
    }

    void StepSWE(float dt)
    {
        // update velocities
        for (int j = 0; j < gridSize; j++)
        {
            for (int i = 1; i < gridSize; i++)
            {
                float etaR = H[C(i, j)]     + h[C(i, j)];
                float etaL = H[C(i - 1, j)] + h[C(i - 1, j)];
                u[U(i, j)] += dt * (-gravity / cellSize) * (etaR - etaL);
            }
        }

        for (int j = 1; j < gridSize; j++)
        {
            for (int i = 0; i < gridSize; i++)
            {
                float etaU = H[C(i, j)]     + h[C(i, j)];
                float etaD = H[C(i, j - 1)] + h[C(i, j - 1)];
                w[W(i, j)] += dt * (-gravity / cellSize) * (etaU - etaD);
            }
        }

        // stop water at edges
        for (int j = 0; j < gridSize; j++)
        {
            u[U(0, j)]        = 0f;
            u[U(gridSize, j)] = 0f;
        }
        for (int i = 0; i < gridSize; i++)
        {
            w[W(i, 0)]        = 0f;
            w[W(i, gridSize)] = 0f;
        }

        // limit velocity 
        float maxSpeed = 0.5f * cellSize / dt;
        for (int k = 0; k < u.Length; k++) u[k] = Mathf.Clamp(u[k], -maxSpeed, maxSpeed);
        for (int k = 0; k < w.Length; k++) w[k] = Mathf.Clamp(w[k], -maxSpeed, maxSpeed);

        // update water height
        float[] hNew = new float[h.Length];
        for (int j = 0; j < gridSize; j++)
        {
            for (int i = 0; i < gridSize; i++)
            {
                float hR = u[U(i+1,j)] <= 0 ? h[C(Mathf.Min(i+1,gridSize-1),j)] : h[C(i,j)];
                float hL = u[U(i,  j)] <= 0 ? h[C(i,j)] : h[C(Mathf.Max(i-1,0),j)];
                float hU = w[W(i,j+1)] <= 0 ? h[C(i,Mathf.Min(j+1,gridSize-1))] : h[C(i,j)];
                float hD = w[W(i,j  )] <= 0 ? h[C(i,j)] : h[C(i,Mathf.Max(j-1,0))];

                float divHv = (hR * u[U(i+1,j)] - hL * u[U(i,j)]) / cellSize
                            + (hU * w[W(i,j+1)] - hD * w[W(i,j)]) / cellSize;

                hNew[C(i,j)] = Mathf.Max(0f, h[C(i,j)] - dt * divHv);
            }
        }
        System.Array.Copy(hNew, h, h.Length);
    }

    public void ApplyDisturbance(int i, int j, float deltaH)
    {
        if (i >= 0 && i < gridSize && j >= 0 && j < gridSize)
            h[C(i, j)] += deltaH;
    }

    void UpdateMesh()
    {
        for (int j = 0; j <= gridSize; j++)
        {
            for (int i = 0; i <= gridSize; i++)
            {
                int vi = j * (gridSize + 1) + i;
                vertices[vi].y = GetCornerHeight(i, j);
            }
        }
        waterMesh.vertices = vertices;
        waterMesh.RecalculateNormals();
    }

    float GetCornerHeight(int i, int j)
    {
        float sum = 0f; int count = 0;
        if (i > 0        && j > 0)        { sum += h[C(i-1, j-1)]; count++; }
        if (i < gridSize && j > 0)        { sum += h[C(i,   j-1)]; count++; }
        if (i > 0        && j < gridSize) { sum += h[C(i-1, j  )]; count++; }
        if (i < gridSize && j < gridSize) { sum += h[C(i,   j  )]; count++; }
        return count > 0 ? sum / count : restDepth;
    }

    void GenerateMesh()
    {
        waterMesh = new Mesh();
        waterMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        
        vertices = new Vector3[(gridSize + 1) * (gridSize + 1)];
        Vector2[] uvs = new Vector2[(gridSize + 1) * (gridSize + 1)];

        for (int j = 0; j <= gridSize; j++)
        {
            for (int i = 0; i <= gridSize; i++)
            {
                int vi = j * (gridSize + 1) + i;
                vertices[vi] = new Vector3(i * cellSize, 0f, j * cellSize);
                uvs[vi] = new Vector2((float)i / gridSize, (float)j / gridSize);
            }
        }

        int[] triangles = new int[gridSize * gridSize * 6];
        int t = 0;
        for (int j = 0; j < gridSize; j++)
        {
            for (int i = 0; i < gridSize; i++)
            {
                int vi = j * (gridSize + 1) + i;
                triangles[t++] = vi;
                triangles[t++] = vi + (gridSize + 1);
                triangles[t++] = vi + 1;
                triangles[t++] = vi + 1;
                triangles[t++] = vi + (gridSize + 1);
                triangles[t++] = vi + (gridSize + 1) + 1;
            }
        }

        waterMesh.vertices = vertices;
        waterMesh.triangles = triangles;
        waterMesh.uv = uvs;
        waterMesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = waterMesh;
    }
}
