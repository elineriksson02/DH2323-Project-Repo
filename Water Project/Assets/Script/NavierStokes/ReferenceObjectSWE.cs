using UnityEngine;
public class ReferenceObjectSWE : MonoBehaviour
{
    public ShallowWaterSimulator water; 
    private Rigidbody rb;
    private bool isUnderWater = false;

    void Start() => rb = GetComponent<Rigidbody>();

    void FixedUpdate()
    {
        if (water == null || rb == null) return;

        float waterHeight = water.GetHeight(transform.position.x, transform.position.z);
        float depth = waterHeight - transform.position.y;

        if (depth > 0f)
        {
            rb.AddForce(Vector3.up * depth * 20f, ForceMode.Acceleration);
            rb.linearDamping = 2f;
        }
        else
        {
            rb.linearDamping = 0f;
        }
    }
}