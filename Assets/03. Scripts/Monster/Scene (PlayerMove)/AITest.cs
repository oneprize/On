using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AITest : MonoBehaviour
{
    public enum State
    {
        Detect,
        Chase,
        Idle,
        Attack
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    [Header("Ranges")]
    [SerializeField] private float detectionRadius = 15f; // 플레이어 발견 범위
    [SerializeField] private float chaseStopDistance = 5f; // 여기까지 추적 후 Idle
    [SerializeField] private float attackRange = 2f;       // 공격 사거리

    [Header("Timings")]
    [SerializeField] private float idleWaitSeconds = 5f;   // Idle 대기 시간
    [SerializeField] private float attackDuration = 1.2f;  // 애니 이벤트를 안 쓸 때 공격 시간

    [Header("Options")]
    [SerializeField] private LayerMask playerMask = ~0;    // 필요 시 시야 체크 용
    [SerializeField] private bool requireLineOfSight = false;
    [SerializeField] private float losCheckHeightOffset = 1.6f;

    [SerializeField] private float chaseRingTolerance = 0.2f; // 5f 도착 판정 허용 오차
    [SerializeField] private bool forceChaseRingOnReturn = true; // Attack 후 5f 맞추기
    [SerializeField] private float navSampleMaxDist = 1.5f; // NavMesh 보정 최대 거리
    private bool _needRepositionToChaseRing = false;
    private Vector3 _chaseRingTarget;

    // Animator 파라미터명(프로젝트에 맞게 변경 가능)
    private static readonly int AnimSpeed = Animator.StringToHash("Speed");
    private static readonly int AnimAttack = Animator.StringToHash("Attack");

    public State CurrentState { get; private set; } = State.Detect;

    // 내부 상태
    private float _idleTimer;
    private bool _isAttacking;
    private bool _attackFinishedByEvent;

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        if (agent) agent.updateRotation = true;
    }

    private void Update()
    {
        if (player == null || agent == null)
            return;

        switch (CurrentState)
        {
            case State.Detect:
                TickDetect();
                break;

            case State.Chase:
                TickChase();
                break;

            case State.Idle:
                TickIdle();
                break;

            case State.Attack:
                // 공격 상태에서는 코루틴이 주요 로직을 처리하므로 여기서는 이동 애니메이션만 업데이트
                UpdateAnimatorSpeed();
                break;
        }
    }

    #region State Ticks

    private void TickDetect()
    {
        if (IsPlayerDetected())
        {
            ChangeState(State.Chase);
        }
    }

    private void TickChase()
    {
        // 플레이어를 잃으면 Detect로
        if (!IsPlayerDetected())
        {
            ChangeState(State.Detect);
            return;
        }

        // 1) Chase 재진입 보정: 먼저 5f 링으로 탈출
        if (_needRepositionToChaseRing)
        {
            agent.isStopped = false;

            // 목표 지점까지 거의 도달했는지 확인
            float ringDist = Vector3.Distance(FlatPos(transform.position), FlatPos(_chaseRingTarget));
            if (ringDist <= chaseRingTolerance)
            {
                _needRepositionToChaseRing = false;
                agent.ResetPath();
                agent.isStopped = true;
                UpdateAnimatorSpeed(0f);
                // 링에 섰으니 Idle로 전환
                ChangeState(State.Idle);
                return;
            }

            // 아직 이동 중이면 계속 진행
            UpdateAnimatorSpeed();
            // 혹시 경로가 끊겼다면 목표를 재계산
            if (!agent.hasPath || agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                _chaseRingTarget = ComputeChaseRingPoint();
                MoveAgentTo(_chaseRingTarget);
            }
            return;
        }

        // 2) 일반 추적 단계
        agent.isStopped = false;
        agent.SetDestination(player.position);
        UpdateAnimatorSpeed();

        float dist = DistanceToPlayerXZ();

        // 5f에 도달하면 Idle로
        if (dist <= chaseStopDistance)
        {
            ChangeState(State.Idle);
        }
    }

    private void TickIdle()
    {
        Debug.Log("Idle");
        // 정지 및 대기
        agent.isStopped = true;
        agent.ResetPath();
        UpdateAnimatorSpeed(0f);

        _idleTimer += Time.deltaTime;
        if (_idleTimer >= idleWaitSeconds)
        {
            ChangeState(State.Attack);
        }
    }

    #endregion

    #region Transitions

    private void ChangeState(State next)
    {
        // Exit
        switch (CurrentState)
        {
            case State.Attack:
                _isAttacking = false;
                _attackFinishedByEvent = false;
                break;
        }

        // Enter
        CurrentState = next;
        switch (next)
        {
            case State.Detect:
                agent.isStopped = true;
                agent.ResetPath();
                UpdateAnimatorSpeed(0f);
                break;

            case State.Chase:
                _idleTimer = 0f;
                agent.isStopped = false;

                // Attack에서 돌아온 직후, 5f보다 가까우면 먼저 링으로 이동
                if (forceChaseRingOnReturn)
                {
                    float dist = DistanceToPlayerXZ();
                    if (dist < chaseStopDistance - chaseRingTolerance)
                    {
                        _needRepositionToChaseRing = true;
                        _chaseRingTarget = ComputeChaseRingPoint();
                        MoveAgentTo(_chaseRingTarget);
                    }
                    else
                    {
                        _needRepositionToChaseRing = false;
                    }
                }
                break;

            case State.Idle:
                _idleTimer = 0f;
                agent.isStopped = true;
                UpdateAnimatorSpeed(0f);
                break;

            case State.Attack:
                if (!_isAttacking)
                    StartCoroutine(AttackRoutine());
                break;
        }
    }

    private IEnumerator AttackRoutine()
    {
        _isAttacking = true;

        // 1) 공격 사거리까지 접근
        while (true)
        {
            float dist = DistanceToPlayerXZ();
            if (dist <= attackRange) break;

            agent.isStopped = false;
            agent.SetDestination(player.position);
            UpdateAnimatorSpeed();

            // 플레이어를 잃으면 Detect로
            if (!IsPlayerDetected())
            {
                ChangeState(State.Detect);
                yield break;
            }

            yield return null;
        }

        // 2) 공격 수행
        agent.isStopped = true;
        UpdateAnimatorSpeed(0f);

        // 플레이어를 바라보게
        FaceTarget(player.position);

        // 애니메이션 트리거
        if (animator) animator.SetTrigger(AnimAttack);

        // 애니메이션 이벤트를 쓸 경우: OnAttackAnimationFinished() 호출을 기다림
        float elapsed = 0f;
        while (!_attackFinishedByEvent && elapsed < attackDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3) 공격 종료 → 다시 Chase로
        _isAttacking = false;
        _attackFinishedByEvent = false;
        ChangeState(State.Chase);
    }

    #endregion

    #region Helpers

    private bool IsPlayerDetected()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > detectionRadius) return false;

        if (!requireLineOfSight) return true;

        // 간단한 LOS(시야) 체크
        Vector3 origin = transform.position + Vector3.up * losCheckHeightOffset;
        Vector3 target = player.position + Vector3.up * losCheckHeightOffset;
        Vector3 dir = (target - origin).normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, detectionRadius, ~0, QueryTriggerInteraction.Ignore))
        {
            return ((1 << hit.collider.gameObject.layer) & playerMask) != 0;
        }

        return false;
    }

    // --- 보조 함수 추가 ---
    private Vector3 ComputeChaseRingPoint()
    {
        // 플레이어 → 몬스터 방향으로 5f 지점
        Vector3 p = FlatPos(player.position);
        Vector3 m = FlatPos(transform.position);
        Vector3 dir = (m - p);
        if (dir.sqrMagnitude < 0.0001f)
            dir = transform.forward; // 같은 위치일 때 임시 방향

        Vector3 raw = p + dir.normalized * chaseStopDistance;

        // NavMesh 보정
        if (NavMesh.SamplePosition(raw, out NavMeshHit hit, navSampleMaxDist, NavMesh.AllAreas))
            return hit.position;

        // 직선 보정 실패 시, 약간 각도를 돌려서 탐색
        for (int i = 1; i <= 6; i++)
        {
            float angle = 15f * i;
            Vector3 cw = p + Quaternion.Euler(0f, angle, 0f) * dir.normalized * chaseStopDistance;
            Vector3 ccw = p + Quaternion.Euler(0f, -angle, 0f) * dir.normalized * chaseStopDistance;

            if (NavMesh.SamplePosition(cw, out hit, navSampleMaxDist, NavMesh.AllAreas))
                return hit.position;
            if (NavMesh.SamplePosition(ccw, out hit, navSampleMaxDist, NavMesh.AllAreas))
                return hit.position;
        }

        // 정말 안 되면 현재 위치 반환(안전장치)
        return transform.position;
    }

    private void MoveAgentTo(Vector3 target)
    {
        agent.ResetPath();
        agent.isStopped = false;
        agent.SetDestination(target);
    }

    private Vector3 FlatPos(Vector3 v)
    {
        v.y = 0f;
        return v;
    }


    private float DistanceToPlayerXZ()
    {
        Vector3 a = transform.position;
        Vector3 b = player.position;
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private void FaceTarget(Vector3 worldPos)
    {
        Vector3 dir = worldPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 0.3f);
        }
    }

    private void UpdateAnimatorSpeed()
    {
        Debug.Log("Attack");
        if (!animator || agent == null) return;
        float speed = agent.velocity.magnitude;
        animator.SetFloat(AnimSpeed, speed);
    }

    private void UpdateAnimatorSpeed(float overrideSpeed)
    {
        if (!animator) return;
        animator.SetFloat(AnimSpeed, overrideSpeed);
    }

    #endregion

    #region Animation Events

    // 공격 애니메이션 말미에 이벤트로 호출하면, 공격 종료를 즉시 반영
    // 애니메이션 클립에서 이 함수를 이벤트로 연결하세요.
    public void OnAttackAnimationFinished()
    {
        _attackFinishedByEvent = true;
    }

    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // 검출/추적/공격 범위 표시
        Gizmos.color = new Color(0.1f, 0.7f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = new Color(1f, 0.9f, 0.1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, chaseStopDistance);

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
#endif
}
