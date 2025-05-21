using UnityEngine;

public class SetProjectileGravity : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    private float gravityScale = 1f; // Default gravity scale
    private void Start()
    {
        
    }

    private void FixedUpdate()
    {
        rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
    }

    public void SetGravityScale(float s )
    {
        gravityScale = s;  
    }
}
