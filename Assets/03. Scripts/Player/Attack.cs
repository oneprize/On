using UnityEngine;

public class Attack : MonoBehaviour
{
    public Animator animator;
    int hashAttackCount = Animator.StringToHash("AttackCount");
    

    private int AttackCount = 0;
    private float lastInputTime = 0f;
    private float resetDelay = 0.4f; // 입력 없을 때 초기화까지의 시간

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        if (Input.GetButtonDown("Fire1")) // 원하는 키로 변경 가능
        {
            animator.SetTrigger("Attack");
            animator.SetInteger("AttackCount", AttackCount);
            AttackCount++;
            lastInputTime = Time.time; // 마지막 입력 시간 저장
        }

        // 일정 시간 동안 입력이 없으면 어택 카운트 초기화
        if (Time.time - lastInputTime > resetDelay && AttackCount != 0)
        {
            AttackCount = 0;
            animator.SetInteger("AttackCount", AttackCount);


        }

        if (Input.GetMouseButton(1))
        {
            animator.SetBool("isDefending", true);
        }
        else
        {
            animator.SetBool("isDefending", false);
        }
    }
}
