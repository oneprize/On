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
    private float turningTimer = 0f;
    private float turningHoldTime = 0.2f;

    int jumpCount = 0; // 플레이어가 점프한 횟수
    const int maxJumps = 2; // 최대 연속 점프 횟수

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
    [SerializeField] private bool isTurning = false; // 방향 전환 감지 변수 추가

    // 콤보 관련 변수 정의
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
        HandleAttack();
        HandleDefense();
    }

    void HandleMovement()
    {
        // 공격 또는 방어 중일 때는 이동을 막는다
        if (isAttacking || isDefending) return;

        Vector3 inputDir = GetInputDirection();
        bool isMoving = inputDir.magnitude > 0.1f;
        bool isRunning = isMoving && Input.GetKey(KeyCode.LeftShift);

        // 현재 이동 방향을 계산합니다.
        Vector3 currentMoveDirection = cam.forward * Input.GetAxisRaw("Vertical") + cam.right * Input.GetAxisRaw("Horizontal");
        currentMoveDirection.y = 0f;
        currentMoveDirection.Normalize();

        if (isMoving && lastMoveDirection.magnitude > 0.1f)
        {
            float dot = Vector3.Dot(lastMoveDirection.normalized,
                                    currentMoveDirection.normalized);
            if (dot < -0.8f)
                turningTimer = turningHoldTime; // 반전 시 타이머 시작
        }

        // 타이머가 남아있으면 isTurning 유지
        if (turningTimer > 0f)
        {
            turningTimer -= Time.deltaTime;
            isTurning = true;
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
            if (isTurning) speed *= 0.3f;
            controller.Move(currentMoveDirection * speed * Time.deltaTime);

            lastMoveDirection = currentMoveDirection;
        }

        // 애니메이터 파라미터 업데이트
        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isTurning", isTurning); // 추가된 isTurning 파라미터

        Vector2 animInput = TransformToLocalInput(inputDir);
        animator.SetFloat("moveX", animInput.x);
        animator.SetFloat("moveY", animInput.y);


        this.isMoving = isMoving;
    }


    void HandleJump()
    {
        // Raycast를 사용하여 바닥에 닿았는지 확인
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance + 0.1f);

        if (isGrounded)
        {
            // 착지 시점에 점프 횟수를 초기화
            jumpCount = 0;
            isJumping = false;
            animator.SetBool("isJumping", false);

            animator.SetInteger("jumpCount", jumpCount);
            // CharacterController가 바닥에 잘 붙도록 y축 속도 보정
            if (velocity.y < 0)
            {
                velocity.y = -2f;
            }
        }

        animator.SetBool("isGrounded", isGrounded);

        // 공격 또는 방어 중일 때는 새로운 점프를 시작하지 못하게 막는다
        if (!isAttacking && !isDefending && Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
        {
            velocity.y = jumpForce;
            isJumping = true;
            animator.SetBool("isJumping", true);
            jumpCount++; // 점프 횟수 1 증가
            animator.SetInteger("jumpCount", jumpCount);
        }

        // 중력 적용
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }

    public bool IsDefending()
    {
        return isDefending;
    }
    // 공격 및 방어 로직을 담당하는 함수
    void HandleAttack()
    {
        if (isMoving || isDefending ) return;
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            isAttacking = false;
        }
        // 공격 실행
        if (Input.GetButtonDown("Fire1"))
        {
            animator.SetTrigger("Attack");
            animator.SetInteger("AttackCount", AttackCount);
            AttackCount++;
            lastInputTime = Time.time;
            isAttacking = true; // 공격 상태 진입 업데이트
        }

        // 일정 시간 동안 입력이 없으면 콤보 카운트 초기화
        if (Time.time - lastInputTime > resetDelay && AttackCount != 0)
        {
            Debug.Log(Time.time - lastInputTime);
            AttackCount = 0;
            lastInputTime = 0;
            animator.SetInteger("AttackCount", AttackCount);
        }
    }

    public void HandleDefense()
    {
        if (isAttacking) return;
        // 방어 실행
        isDefending = Input.GetMouseButton(1);
        animator.SetBool("isDefending", isDefending);
        // isDefending = true;
    }
    // 애니메이션 이벤트에서 호출하는 함수
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
