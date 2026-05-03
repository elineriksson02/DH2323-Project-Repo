using UnityEngine;

[System.Serializable]
public class Wave
{
    public float amplitude  = 0.5f;
    public float wavelength = 10f;
    public float speed      = 1.5f;
    public float steepness  = 0.5f;   // 0 = sine, 1 = sharp Gerstner peak
    public Vector2 direction = new Vector2(1f, 0f);
}