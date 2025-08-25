using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 720f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    public float groundCheckDistance = 0.2f; // �ٴ� üũ�� �Ÿ�

    public Animator animator;
    public Transform cam;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isJumping = false;
    private bool isGrounded = false;
    private Vector3 lastMoveDirection; // ���� �̵� ������ ������ ����

    [SerializeField] private bool isAttacking = false;
    [SerializeField] private bool isMoving = false;
    [SerializeField] private bool isDefending = false;
    [SerializeField] private bool isTurning = false; // ���� ��ȯ ���� ���� �߰�

    // ���� ������ ���� ����
    private int AttackCount = 0;
    [SerializeField] private float lastInputTime = 0f;
    [SerializeField] private float resetDelay = 1f; // �Է� ���� �� �ʱ�ȭ������ �ð�

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
        HandleAttackAndDefense(); // ���� �� ��� ������ ������ �Լ� ȣ��
    }

    void HandleMovement()
    {
        // ���� �Ǵ� ��� ���� ���� �̵��� ����
        if (isAttacking || isDefending) return;

        Vector3 inputDir = GetInputDirection();
        bool isMoving = inputDir.magnitude > 0.1f;
        bool isRunning = isMoving && Input.GetKey(KeyCode.LeftShift);

        // ���� �̵� ������ ����մϴ�.
        Vector3 currentMoveDirection = cam.forward * Input.GetAxisRaw("Vertical") + cam.right * Input.GetAxisRaw("Horizontal");
        currentMoveDirection.y = 0f;
        currentMoveDirection.Normalize();

        // 180�� ���� ��ȯ ���� ����
        // ���� �̵� ����� ���� �̵� ������ ���Ͽ� �ݴ� �������� �Է��ߴ��� Ȯ��
        // `isMoving` ���°� Ȱ��ȭ�Ǿ� �־�� �մϴ�.
        if (lastMoveDirection.magnitude > 0.1f && currentMoveDirection.magnitude > 0.1f)
        {
            // �� ������ ����(Dot product)�� -0.95f �����̸� ���� �ݴ� ����
            if (Vector3.Dot(lastMoveDirection.normalized, currentMoveDirection.normalized) < -0.95f)
            {
                isTurning = true;
            }
            else
            {
                isTurning = false;
            }
        }
        else
        {
            isTurning = false;
        }

        if (isMoving)
        {
            Quaternion targetRot = Quaternion.LookRotation(currentMoveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            float speed = isRunning ? runSpeed : walkSpeed;
            controller.Move(currentMoveDirection * speed * Time.deltaTime);
        }

        // �ִϸ����� �Ķ���� ������Ʈ
        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isTurning", isTurning); // �߰��� isTurning �Ķ����

        Vector2 animInput = TransformToLocalInput(inputDir);
        animator.SetFloat("moveX", animInput.x);
        animator.SetFloat("moveY", animInput.y);

        this.isMoving = isMoving;

        // ���� �̵� ������ ���� �������� ���� ����
        lastMoveDirection = currentMoveDirection;
    }

    void HandleJump()
    {
        // Raycast�� �ٴ� üũ
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance + 0.1f);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // CharacterController���� �ٴڿ� �ٰ�
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

        // �߷� ����
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // ���� �� ��� ������ ����ϴ� ���ο� �Լ�
    void HandleAttackAndDefense()
    {
        if (isMoving) return;
        // ���� ����
        if (Input.GetButtonDown("Fire1"))
        {
            animator.SetTrigger("Attack");
            animator.SetInteger("AttackCount", AttackCount);
            AttackCount++;
            lastInputTime = Time.time;
            isAttacking = true; // ���� ���� ���� ������Ʈ
        }

        // ���� �ð� ���� �Է��� ������ ���� ī��Ʈ �ʱ�ȭ
        if (Time.time - lastInputTime > resetDelay && AttackCount != 0)
        {
            Debug.Log(Time.time - lastInputTime);
            AttackCount = 0;
            lastInputTime = 0;
            animator.SetInteger("AttackCount", AttackCount);
        }

        // ��� ����
        isDefending = Input.GetMouseButton(1);
        animator.SetBool("isDefending", isDefending);
    }

    // �ִϸ��̼� �̺�Ʈ���� ȣ���� �Լ�
    public void ResetAttack()
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