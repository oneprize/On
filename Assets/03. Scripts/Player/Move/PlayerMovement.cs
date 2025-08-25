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
    private Vector3 lastMoveDirection; // 이전 이동 방향을 저장할 변수

    [SerializeField] private bool isAttacking = false;
    [SerializeField] private bool isMoving = false;
    [SerializeField] private bool isDefending = false;
    [SerializeField] private bool isTurning = false; // 방향 전환 상태 변수 추가

    // 공격 로직을 위한 변수
    private int AttackCount = 0;
    [SerializeField] private float lastInputTime = 0f;
    [SerializeField] private float resetDelay = 1f; // 입력 없을 때 초기화까지의 시간

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
        HandleAttackAndDefense(); // 공격 및 방어 로직을 통합한 함수 호출
    }

    void HandleMovement()
    {
        // 공격 또는 방어 중일 때는 이동을 막음
        if (isAttacking || isDefending) return;

        Vector3 inputDir = GetInputDirection();
        bool isMoving = inputDir.magnitude > 0.1f;
        bool isRunning = isMoving && Input.GetKey(KeyCode.LeftShift);

        // 현재 이동 방향을 계산합니다.
        Vector3 currentMoveDirection = cam.forward * Input.GetAxisRaw("Vertical") + cam.right * Input.GetAxisRaw("Horizontal");
        currentMoveDirection.y = 0f;
        currentMoveDirection.Normalize();

        // 180도 방향 전환 감지 로직
        // 현재 이동 방향과 이전 이동 방향을 비교하여 반대 방향으로 입력했는지 확인
        // `isMoving` 상태가 활성화되어 있어야 합니다.
        if (lastMoveDirection.magnitude > 0.1f && currentMoveDirection.magnitude > 0.1f)
        {
            // 두 벡터의 내적(Dot product)이 -0.95f 이하이면 거의 반대 방향
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

        // 애니메이터 파라미터 업데이트
        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isTurning", isTurning); // 추가된 isTurning 파라미터

        Vector2 animInput = TransformToLocalInput(inputDir);
        animator.SetFloat("moveX", animInput.x);
        animator.SetFloat("moveY", animInput.y);

        this.isMoving = isMoving;

        // 현재 이동 방향을 다음 프레임을 위해 저장
        lastMoveDirection = currentMoveDirection;
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

    // 공격 및 방어 로직을 담당하는 새로운 함수
    void HandleAttackAndDefense()
    {
        if (isMoving) return;
        // 공격 로직
        if (Input.GetButtonDown("Fire1"))
        {
            animator.SetTrigger("Attack");
            animator.SetInteger("AttackCount", AttackCount);
            AttackCount++;
            lastInputTime = Time.time;
            isAttacking = true; // 공격 상태 변수 업데이트
        }

        // 일정 시간 동안 입력이 없으면 어택 카운트 초기화
        if (Time.time - lastInputTime > resetDelay && AttackCount != 0)
        {
            Debug.Log(Time.time - lastInputTime);
            AttackCount = 0;
            lastInputTime = 0;
            animator.SetInteger("AttackCount", AttackCount);
        }

        // 방어 로직
        isDefending = Input.GetMouseButton(1);
        animator.SetBool("isDefending", isDefending);
    }

    // 애니메이션 이벤트에서 호출할 함수
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