using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;

    private NavMeshAgent agent;
    private Animator animator;
    private ParryManager parryManager;

    private enum State { Idle, Chase, Attack, Groggy } // 상태 움직임
    private State currentState = State.Idle;

    private float lastAttackTime = -999f;

    private float groggyTime = 2f; // 그로기 지속 시간
    private bool isGroggy = false;
    private bool isWalking = false;
    private bool canMove = true;
    public bool isAttacking=false;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        parryManager = GetComponent<ParryManager>();
    }

    void Update()
    {
        // 현재 애니메이션 상태를 가져옵니다.
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // 공격 태그가 있는 애니메이션이 재생 중이거나 그로기 상태일 때
        if (stateInfo.IsTag("Attack") || isGroggy)
        {
            // NavMeshAgent의 이동을 중지합니다.
            agent.isStopped = true;
        }
        // 그 외의 상황에서는 이동을 허용합니다.
        else
        {
            agent.isStopped = false;
        }

        // 그로기 상태일 때는 다른 행동 로직을 실행하지 않고 함수를 종료합니다.
        if (isGroggy) return;

        float distance = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Idle:
                HandleIdle(distance);
                break;
            case State.Chase:
                HandleChase(distance);
                break;
            case State.Attack:
                HandleAttack(distance);
                break;
        }
    }

    void HandleIdle(float distance)
    {
        animator.SetBool("isWalking", false);

        if (distance < detectionRange)
        {
            currentState = State.Chase;
            animator.SetBool("isWalking", true);
        }
    }

    void HandleChase(float distance)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            isWalking = false;            
        }
        if (distance > detectionRange)
        {
            currentState = State.Idle;
            agent.ResetPath();
            return;
        }

        // 플레이어를 향해 회전
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0f;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);

        if (distance > attackRange)
        {
            agent.SetDestination(player.position);
            animator.SetBool("isWalking", true);
        }
        else
        {
            agent.ResetPath();
            animator.SetBool("isWalking", false);
            currentState = State.Attack;
        }
    }

    void HandleAttack(float distance)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {          
            isWalking = false;
        }
        
        // 공격 애니메이션이 재생 중이라면 다른 상태로 전환하지 않고 종료
        if (isAttacking)
        {
            // 공격 중에는 플레이어를 향하도록 계속 회전
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
            return;
        }

        // 공격 애니메이션이 끝난 후의 로직
        if (distance > attackRange)
        {
            // 공격 범위 밖으로 벗어나면 추적 상태로 전환
            currentState = State.Chase;
        }
        else
        {
            // 공격 범위 내에 있으면 쿨다운 확인 후 다시 공격
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                animator.SetTrigger("attack");
                lastAttackTime = Time.time;
            }
        }
    }
    public void EnterGroggy()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Groggy"))
        {
            isWalking = false;
        }
        if (isGroggy) return;

        isGroggy = true;
        currentState = State.Groggy;
        agent.ResetPath(); // 이동 중지
        animator.SetTrigger("groggy"); // groggy 애니메이션 트리거
        StartCoroutine(GroggyRecover());
    }

    private System.Collections.IEnumerator GroggyRecover()
    {
        yield return new WaitForSeconds(groggyTime);
        isGroggy = false;
        currentState = State.Idle; // 다시 idle 상태로 복귀
    }


}
