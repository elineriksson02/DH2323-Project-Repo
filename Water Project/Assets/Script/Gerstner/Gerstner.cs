using UnityEngine;
using System.Collections.Generic;

public class Gerstner : MonoBehaviour
{
    [System.Serializable]
    public class Ripple
    {
        public Vector2 position;
        public float time;
        public float strength;
    }

    public List<Ripple> ripples = new List<Ripple>();

    public float rippleSpeed = 2f;
    public float rippleDecay = 1f;
    Mesh mesh;
    Vector3[] vertices;
    Vector3[] initialVertices;
    public Wave[] waves;
    public float amplitude = 0.5f;
    public float steepness = 1f;
    public float speed = 1f;
    public Vector2 direction = new Vector2(1, 0);

    public List<float> fpsSamples = new List<float>();
    public float timer = 0f;

    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        mesh = mf.mesh; 
        initialVertices = mesh.vertices; 
        vertices = new Vector3[initialVertices.Length]; 
    }
 
   
    void Update()
    {
        GerstnerWave();

        float currentFPS = 1.0f / Time.deltaTime;
        fpsSamples.Add(currentFPS);

        timer += Time.deltaTime;

        if (timer >= 5f)
        {
            float avg = 0f;

            foreach (float f in fpsSamples)
                avg += f;

            avg /= fpsSamples.Count;

            Debug.Log("Average FPS (5s): " + avg);

            fpsSamples.Clear();
            timer = 0f;
        }
    }

    float GerstnerWaveHeight(Vector2 position, float time, float amplitude, float steepness, Vector2 direction, float speed)
    {
        float frequency = 2f;
        return amplitude * Mathf.Sin(Vector2.Dot(direction.normalized, position) * frequency + time * speed);
    }

    void GerstnerWave()
    {
        if (waves == null || waves.Length == 0)
    {
        Debug.LogWarning("No waves assigned!");
        return;
    }

    if (vertices == null || initialVertices == null)
    {
        Debug.LogWarning("Vertices not initialized!");
        return;
    }
    
        if (waves == null || waves.Length == 0)
        {
            Debug.LogWarning("No waves assigned!");
            return;
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = initialVertices[i];
            Vector3 newPos = v;
            for (int j = 0; j < waves.Length; j++)
            {
                Wave w = waves[j];
                float k = 2 * Mathf.PI / w.wavelength;
                Vector2 d = w.direction.normalized;
                float phase = k * Vector2.Dot(d, new Vector2(v.x, v.z)) - w.speed * Time.time;
                float a = w.amplitude;
                float Q = w.steepness / (k * a * waves.Length);
                float cos = Mathf.Cos(phase);
                float sin = Mathf.Sin(phase);
                newPos.x += Q * a * d.x * cos;
                newPos.z += Q * a * d.y * cos;
                newPos.y += a * sin;
            }

            

            vertices[i] = newPos;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    public float GetHeight(float x, float z)
{
    float height = 0f;

    if (waves == null || waves.Length == 0) return 0f;

   // base waves
    foreach (Wave w in waves)
    {
        float k = 2 * Mathf.PI / Mathf.Max(w.wavelength, 0.1f);
        float phase = k * (Vector2.Dot(w.direction.normalized, new Vector2(x, z)) - w.speed * Time.time);

        height += w.amplitude * Mathf.Sin(phase);
    }

    // add ripple effects
    float rippleHeight = 0f;
    for (int i = ripples.Count - 1; i >= 0; i--)
    {
        Ripple r = ripples[i];

        float t = Time.time - r.time;

        if (t > 4f)
        {
            ripples.RemoveAt(i);
            continue;
        }

        float dist = Vector2.Distance(new Vector2(x, z), r.position);

        float wave = Mathf.Sin((dist - t * 6f) * rippleSpeed);

        float envelope = Mathf.Exp(-t * rippleDecay) * Mathf.Exp(-dist * 0.2f);

        rippleHeight += wave * r.strength * envelope * 2.5f;
    }

    height += rippleHeight;

    return height;
}
    float GerstnerHelper(float x, float z, float amplitude, float wavelength, float speed, Vector2 direction)
    {
        float k = 2 * Mathf.PI / wavelength;
        float c = Mathf.Sqrt(9.8f / k);
        float f = k * (Vector2.Dot(direction.normalized, new Vector2(x, z)) - c * Time.time * speed);
        return amplitude * Mathf.Sin(f);
    }

    public Vector3 GetNormal(float x, float z)
    {
        float eps = 0.01f; 
        float hL = GetHeight(x - eps, z);
        float hR = GetHeight(x + eps, z);
        float hD = GetHeight(x, z - eps);
        float hU = GetHeight(x, z + eps);
     
        Vector3 normal = new Vector3(hL - hR, 2 * eps, hD - hU).normalized;
        return normal;
    }

    public void AddRipple(Vector3 worldPos, float strength)
{
    Debug.Log("Ripple created: " + strength);
    ripples.Add(new Ripple
    {
        position = new Vector2(worldPos.x, worldPos.z),
        time = Time.time,
        strength = strength
    });
}

}
