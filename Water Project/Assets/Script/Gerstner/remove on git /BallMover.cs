using UnityEngine;

public class BallMover : MonoBehaviour
{
    public float speed = 5f;
    public float bobAmplitude = 0.3f;   // vertical oscillation
    public float bobFrequency = 1.2f;

    private float baseY;

    void Start()
    {
        baseY = transform.position.y;
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0, v) * speed * Time.deltaTime;
        transform.position += move;

        float newY = baseY + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}