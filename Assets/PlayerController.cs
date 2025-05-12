using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Hover Flight Settings")]
    public float maxFlyTime = 2f;           // Maximum hover time in seconds
    public float flyRechargeRate = 1f;      // Recharge rate when grounded
    public float hoverLift = 0.5f;          // Vertical velocity when hovering
    public float hoverGravityFactor = 0.2f; // Reduces gravity while hovering

    private float currentFlyTime;
    private bool isFlying;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentFlyTime = maxFlyTime;
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;

            // Recharge fly meter
            currentFlyTime = Mathf.Min(currentFlyTime + flyRechargeRate * Time.deltaTime, maxFlyTime);
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Hovering logic
        if (Input.GetButton("Jump"))
        {
            if (isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                isFlying = false;
            }
            else if (currentFlyTime > 0)
            {
                // Apply hover lift gently
                velocity.y = Mathf.Lerp(velocity.y, hoverLift, 10f * Time.deltaTime);
                currentFlyTime -= Time.deltaTime;
                isFlying = true;
            }
            else
            {
                isFlying = false;
            }
        }
        else
        {
            isFlying = false;
        }

        // Apply gravity
        if (isFlying)
        {
            velocity.y += gravity * hoverGravityFactor * Time.deltaTime;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    // Optional: UI access for fly meter bar
    public float GetFlyMeterPercent()
    {
        return currentFlyTime / maxFlyTime;
    }
}
