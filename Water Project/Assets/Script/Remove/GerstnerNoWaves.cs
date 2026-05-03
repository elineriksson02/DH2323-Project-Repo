using UnityEngine;
using System.Collections.Generic;

public class GerstnerWater : MonoBehaviour
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

    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        mesh = mf.mesh;

        initialVertices = mesh.vertices;
        vertices = new Vector3[initialVertices.Length];
    }

    void Update()
    {
        UpdateWater();
    }

    void UpdateWater()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = initialVertices[i];

            Vector3 newPos = v;
            newPos.y = GetHeight(v.x, v.z);

            vertices[i] = newPos;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    public float GetHeight(float x, float z)
    {
        float height = 0f;

        // Waves (Gerstner baseline)
        if (waves != null)
        {
            foreach (Wave w in waves)
            {
                float k = 2 * Mathf.PI / Mathf.Max(w.wavelength, 0.1f);
                float phase = k * (Vector2.Dot(w.direction.normalized, new Vector2(x, z)) - w.speed * Time.time);

                height += w.amplitude * Mathf.Sin(phase);
            }
        }

        // Ripples
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

            float waveFront = dist - (t * 8f); 
            float rippleWidth = 1.5f; 

            float envelope = Mathf.Exp(-t * rippleDecay) * Mathf.Exp(-(waveFront * waveFront) / rippleWidth);

            height += Mathf.Sin(waveFront * rippleSpeed) * r.strength * envelope;
        }

        return height;

        
    }

    public void AddRipple(Vector3 worldPos, float strength)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);

        ripples.Add(new Ripple
        {
            position = new Vector2(localPos.x, localPos.z),
            time = Time.time,
            strength = strength
        });

    }
}