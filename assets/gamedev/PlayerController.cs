using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float rotationSpeed = 10f;
    public Transform cameraTransform;
    public Animator animator;
    public bool allowMovement = true;

    private CharacterController controller;
    private Vector3 velocity;
    private float gravity = 9.8f;
    private bool isRunning = false;
    private float moveSpeed;

    // Inertia add stop anim
    private Vector3 lastMoveDirection;
    private bool isStopping = false;
    private float stopTimer = 0f;
    public float stopDuration = 0.3f;
    public float stopDistance = 0.5f;

    private ComboAttack comboAttack; // Reference to combo system

    void Start()
    {
        controller = GetComponent<CharacterController>();
        moveSpeed = walkSpeed;
        comboAttack = GetComponent<ComboAttack>(); // Find combo script
    }

    void Update()
    {
        if (!allowMovement) return;

        bool isAttacking = comboAttack != null && comboAttack.IsAttacking;

        // Toggle walk/run
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isRunning = !isRunning;
            moveSpeed = isRunning ? runSpeed : walkSpeed;
        }

        if (!isAttacking)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            bool hasMovement = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;

            animator.SetBool("isMoving", hasMovement);

            Vector3 camForward = cameraTransform.forward;
            camForward.y = 0;
            camForward.Normalize();

            Vector3 camRight = cameraTransform.right;
            camRight.y = 0;
            camRight.Normalize();

            Vector3 inputDirection = new Vector3(horizontal, 0, vertical);
            Vector3 moveDirection = camForward * vertical + camRight * horizontal;
            moveDirection.Normalize();

            // Start drift 
            if (inputDirection.magnitude < 0.1f && !isStopping && lastMoveDirection.magnitude > 0.1f)
            {
                isStopping = true;
                stopTimer = stopDuration;
            }

            // Move normally
            if (moveDirection.magnitude >= 0.1f)
            {
                lastMoveDirection = moveDirection;
                isStopping = false;

                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                Vector3 move = moveDirection * moveSpeed * Time.deltaTime;
                controller.Move(move);
            }
            else if (isStopping)
            {
                stopTimer -= Time.deltaTime;

                if (stopTimer > 0f)
                {
                    float t = stopTimer / stopDuration;
                    Vector3 drift = lastMoveDirection * stopDistance * t * Time.deltaTime;
                    controller.Move(drift);
                }
                else
                {
                    isStopping = false;
                    lastMoveDirection = Vector3.zero;
                }
            }

            float animSpeed = Mathf.Clamp01(inputDirection.magnitude * (isRunning ? 1f : 0.5f));
            animator.SetFloat("Speed", animSpeed);
        }
        else
        {
            animator.SetFloat("Speed", 0f); // Stop animation while attacking
        }

        // Gravity
        if (!controller.isGrounded)
        {
            velocity.y -= gravity * Time.deltaTime;
        }
        else
        {
            velocity.y = -0.1f;
        }

        controller.Move(velocity * Time.deltaTime);
    }
}
