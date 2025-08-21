using UnityEngine;

public class Attack : MonoBehaviour
{
    public Animator animator;
    int hashAttackCount = Animator.StringToHash("AttackCount");
    

    private int AttackCount = 0;
    private float lastInputTime = 0f;
    private float resetDelay = 0.4f; // �Է� ���� �� �ʱ�ȭ������ �ð�

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        if (Input.GetButtonDown("Fire1")) // ���ϴ� Ű�� ���� ����
        {
            animator.SetTrigger("Attack");
            animator.SetInteger("AttackCount", AttackCount);
            AttackCount++;
            lastInputTime = Time.time; // ������ �Է� �ð� ����
        }

        // ���� �ð� ���� �Է��� ������ ���� ī��Ʈ �ʱ�ȭ
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
