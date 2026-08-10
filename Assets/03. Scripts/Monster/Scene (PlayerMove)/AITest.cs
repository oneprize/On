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
    [SerializeField] private float chaseStopDistance = 5f; // 접근하다 멈출 때 Idle
    [SerializeField] private float attackRange = 2f;       // 공격 사거리

    [Header("Timings")]
    [SerializeField] private float idleWaitSeconds = 5f;   // Idle 대기 시간
    [SerializeField] private float attackDuration = 1.2f;  // 애니 이벤트가 안 올 때 대비 시간

    [Header("Options")]
    [SerializeField] private LayerMask playerMask = ~0;    // 필요 시 시야 체크 용
    [SerializeField] private bool requireLineOfSight = false;
    [SerializeField] private float losCheckHeightOffset = 1.6f;

    [SerializeField] private float chaseRingTolerance = 0.2f; // 5f 링에 도달 허용 오차
    [SerializeField] private bool forceChaseRingOnReturn = true; // Attack 후 5f 재고정
    [SerializeField] private float navSampleMaxDist = 1.5f; // NavMesh 샘플 최대 거리
    private bool _needRepositionToChaseRing = false;
    private Vector3 _chaseRingTarget;

    // 애니메이터 파라미터명(프로젝트에 맞게 수정 가능)
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
        // 플레이어를 놓치면 Detect로
        if (!IsPlayerDetected())
        {
            ChangeState(State.Detect);
            return;
        }

        // 1) Chase 진입시 재정렬: 링을 5f 밖으로 탈출
        if (_needRepositionToChaseRing)
        {
            agent.isStopped = false;

            // 목표 링까지 실제 도달했는지 확인
            float ringDist = Vector3.Distance(FlatPos(transform.position), FlatPos(_chaseRingTarget));
            if (ringDist <= chaseRingTolerance)
            {
                _needRepositionToChaseRing = false;
                agent.ResetPath();
                agent.isStopped = true;
                UpdateAnimatorSpeed(0f);
                // 링에 도달하면 Idle로 전환
                ChangeState(State.Idle);
                return;
            }

            // 아직 이동 중이면 계속 유지
            UpdateAnimatorSpeed();
            // 혹시 경로가 끊겼다면 목표를 재설정
            if (!agent.hasPath || agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                _chaseRingTarget = ComputeChaseRingPoint();
                MoveAgentTo(_chaseRingTarget);
            }
            return;
        }

        // 2) 일반 추격 단계
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
        // 제자리 대기
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
                // [테스트를 위해 임시 주석 처리] 공격이 끝났으니 NavMeshAgent의 자동 회전을 다시 켠다
                //if (agent) agent.updateRotation = true;
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

                // [테스트를 위해 임시 주석 처리] 공격 후 5f보다 가까우면 원래 링(거리)까지 뒤로 물러나는 기능
                //if (forceChaseRingOnReturn)
                //{
                //    float dist = DistanceToPlayerXZ();
                //    if (dist < chaseStopDistance - chaseRingTolerance)
                //    {
                //        _needRepositionToChaseRing = true;
                //        _chaseRingTarget = ComputeChaseRingPoint();
                //        MoveAgentTo(_chaseRingTarget);
                //    }
                //    else
                //    {
                //        _needRepositionToChaseRing = false;
                //    }
                //}
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

        // 1) 사거리 진입까지 추적
        while (true)
        {
            float dist = DistanceToPlayerXZ();
            if (dist <= attackRange) break;

            agent.isStopped = false;
            agent.SetDestination(player.position);
            UpdateAnimatorSpeed();

            // 플레이어를 놓치면 Detect로
            if (!IsPlayerDetected())
            {
                ChangeState(State.Detect);
                yield break;
            }

            yield return null;
        }

        // 2) 공격 시작
        agent.isStopped = true;
        agent.ResetPath();
        // [테스트를 위해 임시 주석 처리] 루트모션이 회전을 전담하도록 NavMeshAgent의 자동 회전을 끄는 기능
        //agent.updateRotation = false;
        UpdateAnimatorSpeed(0f);

        // 플레이어를 바라보게 회전시키는 기능
        FaceTarget(player.position);

        // 애니메이션 트리거
        if (animator) animator.SetTrigger(AnimAttack);

        // 애니메이션 이벤트가 올 때까지 대기: OnAttackAnimationFinished() 호출을 기다림
        float elapsed = 0f;
        while (!_attackFinishedByEvent && elapsed < attackDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // [테스트를 위해 임시 주석 처리] NavMeshAgent 자동 회전 복구
        //if (agent) agent.updateRotation = true;

        // 3) 공격 종료 후 다시 Chase로
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
        // 플레이어 기준 링 방향으로 5f 지점
        Vector3 p = FlatPos(player.position);
        Vector3 m = FlatPos(transform.position);
        Vector3 dir = (m - p);
        if (dir.sqrMagnitude < 0.0001f)
            dir = transform.forward; // 같은 위치일 때 임시 방향

        Vector3 raw = p + dir.normalized * chaseStopDistance;

        // NavMesh 보정
        if (NavMesh.SamplePosition(raw, out NavMeshHit hit, navSampleMaxDist, NavMesh.AllAreas))
            return hit.position;

        // 보정 실패 시, 약간 각도를 돌려서 탐색
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

        // 전부 다 실패하면 현재 위치 반환(안전장치)
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
            // 허공에 스윙이 나가지 않도록, 공격 시작 시점에 플레이어를 정확히 한 번에 조준한다.
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    private void UpdateAnimatorSpeed()
    {
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

    // 공격 애니메이션 끝쪽에 이벤트로 호출하면, 조기 종료를 즉시 반영
    // 애니메이션 클립에 이 함수를 이벤트로 연결하세요.
    public void OnAttackAnimationFinished()
    {
        _attackFinishedByEvent = true;
    }

    // 각 콤보(Combo1/2/3)의 BeginAttackWindow 이벤트에서 호출됨 - 콤보가 이어지는 동안
    // 플레이어가 움직여도 매 타격 시작 시점마다 다시 조준하도록 한다.
    public void BeginAttackWindow()
    {
        if (player != null)
        {
            FaceTarget(player.position);
        }
    }

    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // 감지/정지/공격 범위 표시
        Gizmos.color = new Color(0.1f, 0.7f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = new Color(1f, 0.9f, 0.1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, chaseStopDistance);

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
#endif
}
