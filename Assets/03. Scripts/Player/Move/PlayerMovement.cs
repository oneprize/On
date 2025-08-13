using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 720f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    public float groundCheckDistance = 0.2f; // 바닥 체크용 거리

    public Animator animator;
    public Transform cam;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isJumping = false;
    private bool isGrounded = false;
    private bool isAttacking = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
        HandleAttack();
    }

    void HandleMovement()
    {
        if (isAttacking) return;

        Vector3 inputDir = GetInputDirection();
        bool isMoving = inputDir.magnitude > 0.1f;
        bool isRunning = isMoving && Input.GetKey(KeyCode.LeftShift);

        Vector3 moveDir = cam.forward * Input.GetAxisRaw("Vertical") + cam.right * Input.GetAxisRaw("Horizontal");
        moveDir.y = 0f;
        moveDir.Normalize();

        if (isMoving)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            float speed = isRunning ? runSpeed : walkSpeed;
            controller.Move(moveDir * speed * Time.deltaTime);
        }

        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isRunning", isRunning);

        Vector2 animInput = TransformToLocalInput(inputDir);
        animator.SetFloat("moveX", animInput.x);
        animator.SetFloat("moveY", animInput.y);
    }

    void HandleJump()
    {
        // Raycast로 바닥 체크
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance + 0.1f);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // CharacterController에서 바닥에 붙게
            isJumping = false;
            animator.SetBool("isJumping", false);
        }

        animator.SetBool("isGrounded", isGrounded);

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = jumpForce;
            isJumping = true;
            animator.SetBool("isJumping", true);
        }

        // 중력 적용
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleAttack()
    {
        if (isAttacking) return;

        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("attack");
            isAttacking = true;
        }
    }

    void ResetAttack()
    {
        isAttacking = false;
    }


    Vector3 GetInputDirection()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        return new Vector3(h, 0, v).normalized;
    }

    Vector2 TransformToLocalInput(Vector3 input)
    {
        Vector3 camForward = cam.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = cam.right;
        camRight.y = 0;
        camRight.Normalize();

        float localX = Vector3.Dot(input, camRight);
        float localY = Vector3.Dot(input, camForward);

        return new Vector2(localX, localY);
    }
}