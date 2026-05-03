using UnityEngine;
public class ReferenceObjectG : MonoBehaviour
{
    public GerstnerWater water;
    private Rigidbody rb;
    private bool isUnderWater = false; 

    void Start() => rb = GetComponent<Rigidbody>();

    void FixedUpdate()
    {
        if (water == null || rb == null) return;

        float waterHeight = water.GetHeight(transform.position.x, transform.position.z);
        float depth = waterHeight - transform.position.y;

        if (depth > 0f) // Under ytan
        {
            if (!isUnderWater) 
            {
                float splashForce = Mathf.Abs(rb.linearVelocity.y);
                water.AddRipple(transform.position, splashForce * 0.8f);
                isUnderWater = true;
            }

            rb.AddForce(Vector3.up * depth * 15f, ForceMode.Acceleration);
            rb.linearDamping = 2f;
        }
        else
        {
            isUnderWater = false;
            rb.linearDamping = 0f;
        }
    }
}