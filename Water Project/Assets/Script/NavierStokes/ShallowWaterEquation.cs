using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ShallowWaterSimulator : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridSize = 64;
    public float cellSize = 0.5f;

    [Header("Simulation Settings")]
    public float gravity = 9.81f;
    public float restDepth = 2.0f;

    [Header("Wave Source")]
    public float waveFrequency = 1.5f;
    public float waveAmplitude = 0.3f;

    [Header("Interactions")]
    public Transform ballObject; 
    public float objectRadius = 1.0f;
    public float interactionStrength = 0.5f;

    private Vector3[] particles;
    private Vector3[] particleVelocities;

    // Simulation arrays
    private float[] h;
    private float[] H;
    private float[] u;
    private float[] w;

    // Mesh
    private Mesh waterMesh;
    private Vector3[] vertices;

    // Index helpers
    int C(int i, int j) => j * gridSize + i;
    int U(int i, int j) => j * (gridSize + 1) + i;
    int W(int i, int j) => j * gridSize + i;


    int frameCount = 0;
    float elapsedTime = 0;
    float fps = 0;
    void CalculateFPS(float deltaTime) 
    {
    frameCount++;
    elapsedTime += deltaTime; // deltaTime is seconds passed since last frame

    if (elapsedTime >= 1.0f) {
        fps = frameCount / elapsedTime;
        frameCount = 0;
        elapsedTime = 0;
        // Output fps variable
        Debug.Log("fps:" + fps);
    }
    }

    void Start()
    {
        // 1. Init simulation arrays FIRST
        h = new float[gridSize * gridSize];
        H = new float[gridSize * gridSize];
        u = new float[(gridSize + 1) * gridSize];
        w = new float[gridSize * (gridSize + 1)];

        for (int i = 0; i < h.Length; i++) h[i] = restDepth;

        // 2. Build mesh AFTER arrays exist
        GenerateMesh();
    }

void Update()
{
    ApplyContinuousWave();
    //ApplyObjectInteraction();

    if (Input.GetKeyDown(KeyCode.Space))
        ApplyDisturbance(gridSize / 2, gridSize / 2, waveAmplitude * 3f);

    float dt = Time.deltaTime;
    float maxWaveSpeed = Mathf.Sqrt(gravity * restDepth);
    float stableDt = 0.4f * cellSize / maxWaveSpeed;
    int substeps = Mathf.CeilToInt(dt / stableDt);
    float subDt = dt / substeps;

    for (int s = 0; s < substeps; s++)
        StepSWE(subDt);

    UpdateMesh();
    
    CalculateFPS(dt);

    // DEBUG: print min and max height every second to confirm simulation is running
    debugTimer += Time.deltaTime;
    if (debugTimer > 1f)
    {
        debugTimer = 0f;
        float minH = float.MaxValue;
        float maxH = float.MinValue;
        for (int i = 0; i < h.Length; i++)
        {
            if (h[i] < minH) minH = h[i];
            if (h[i] > maxH) maxH = h[i];
        }
        Debug.Log($"h min={minH:F3}  max={maxH:F3}  restDepth={restDepth}  amp={waveAmplitude}");
    }
}

private float debugTimer = 0f;
    /*void Update2()
    {
        // Drive wave from edge
        //ApplyContinuousWave();
        ApplyObjectInteraction();

        // Press Space for point disturbance
        if (Input.GetKeyDown(KeyCode.Space))
            ApplyDisturbance(gridSize / 2, gridSize / 2, 1.0f);

        // CFL substeps
        float dt = Time.deltaTime;
        float maxWaveSpeed = Mathf.Sqrt(gravity * restDepth);
        float stableDt = 0.4f * cellSize / maxWaveSpeed;
        int substeps = Mathf.CeilToInt(dt / stableDt);
        float subDt = dt / substeps;

        for (int s = 0; s < substeps; s++)
            StepSWE(subDt);

        UpdateMesh();
    }*/

    void StepSWE(float dt)
    {
        // --- Step 1: Velocity update (uses current h) ---
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

        // --- Step 2: Reflective boundaries ---
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

        // --- Step 3: Velocity clamping ---
        float maxSpeed = 0.5f * cellSize / dt;
        for (int k = 0; k < u.Length; k++) u[k] = Mathf.Clamp(u[k], -maxSpeed, maxSpeed);
        for (int k = 0; k < w.Length; k++) w[k] = Mathf.Clamp(w[k], -maxSpeed, maxSpeed);

        // --- Step 4: Height update ---
        float[] hNew = new float[h.Length];
        for (int j = 0; j < gridSize; j++)
        {
            for (int i = 0; i < gridSize; i++)
            {
                //float hR = u[U(i+1,j)] <= 0 ? h[C(Mathf.Min(i+1,gridSize-1),j)] : h[C(i,j)];
               // float hL = u[U(i,  j)] <= 0 ? h[C(i,j)] : h[C(Mathf.Max(i-1,0),j)];
                float hR = u[U(i+1,j)] >= 0 ? h[C(i,j)] : h[C(Mathf.Min(i+1,gridSize-1),j)];
                float hL = u[U(i,  j)] >= 0 ? h[C(Mathf.Max(i-1,0),j)] : h[C(i,j)];
                float hU = w[W(i,j+1)] <= 0 ? h[C(i,Mathf.Min(j+1,gridSize-1))] : h[C(i,j)];
                float hD = w[W(i,j  )] <= 0 ? h[C(i,j)] : h[C(i,Mathf.Max(j-1,0))];

                float divHv = (hR * u[U(i+1,j)] - hL * u[U(i,j)]) / cellSize
                            + (hU * w[W(i,j+1)] - hD * w[W(i,j)]) / cellSize;

                hNew[C(i,j)] = Mathf.Max(0f, h[C(i,j)] - dt * divHv);
            }
        }
        System.Array.Copy(hNew, h, h.Length);
    }

    void ApplyContinuousWave2()
    {
        float phase = Time.time * waveFrequency * 2f * Mathf.PI;
        for (int i = 0; i < gridSize; i++)
            h[C(i, 0)] = restDepth + waveAmplitude * Mathf.Sin(phase);
    }

    void ApplyContinuousWaveWithObject()
{
    float phase = Time.time * waveFrequency * 2f * Mathf.PI;
    float sourceHeight = restDepth + waveAmplitude * Mathf.Sin(phase);
    
    // Drive entire bottom edge uniformly for clean parallel waves
    for (int i = 0; i < gridSize; i++)
        h[C(i, 0)] = sourceHeight;
}

void ApplyObjectInteraction()
{
    if (ballObject == null) return;

    Vector3 localPos = ballObject.position - transform.position;
    int gridX = Mathf.RoundToInt(localPos.x / cellSize);
    int gridZ = Mathf.RoundToInt(localPos.z / cellSize);

    int radiusInCells = Mathf.CeilToInt(objectRadius / cellSize);

    for (int j = gridZ - radiusInCells; j <= gridZ + radiusInCells; j++)
    {
        for (int i = gridX - radiusInCells; i <= gridX + radiusInCells; i++)
        {
            if (i >= 0 && i < gridSize && j >= 0 && j < gridSize)
            {
                float dist = Vector2.Distance(new Vector2(i, j), new Vector2(gridX, gridZ)) * cellSize;
                if (dist < objectRadius)
                {
                    float waterSurface = h[C(i, j)];   // current height at this cell
                    float immersion = waterSurface - (ballObject.position.y - transform.position.y);
                    
                    float force = Mathf.Clamp(immersion, -1f, 1f) * (1f - dist / objectRadius);
                    h[C(i, j)] += force * interactionStrength * Time.deltaTime;
                }
            }
        }
    }
}

void ApplyContinuousWave()
{
    float phase = Time.time * waveFrequency * 2f * Mathf.PI;
    
    // Bottom edge
    for (int i = 0; i < gridSize; i++)
    {
        float spatialOffset = i * 0.15f;
        h[C(i, 0)] = restDepth + waveAmplitude * 
                     Mathf.Sin(phase + spatialOffset);
    }
    
    // Left edge — different frequency for interference
    for (int j = 0; j < gridSize; j++)
    {
        float spatialOffset = j * 0.1f;
        h[C(0, j)] = restDepth + waveAmplitude * 0.6f * 
                     Mathf.Sin(phase * 1.3f + spatialOffset);
    }
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
        waterMesh.name = "WaterSWE";
        waterMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        // Assign to CLASS-LEVEL vertices field
        vertices = new Vector3[(gridSize + 1) * (gridSize + 1)];
        Vector2[] uvs = new Vector2[(gridSize + 1) * (gridSize + 1)];

        for (int j = 0; j <= gridSize; j++)
        {
            for (int i = 0; i <= gridSize; i++)
            {
                int vi = j * (gridSize + 1) + i;
                vertices[vi] = new Vector3(i * cellSize, restDepth, j * cellSize);
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