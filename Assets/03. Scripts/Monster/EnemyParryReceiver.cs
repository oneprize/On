using UnityEngine;

public class EnemyParryReceiver : MonoBehaviour
{
    public Animator animator;
    public bool isGroggy = false;
    public float groggyDuration = 2f;

    public void EnterGroggyState()
    {
        if (isGroggy) return;

        isGroggy = true;
        Debug.Log("그로기 시작!");
        animator.SetTrigger("Groggy");

        Invoke(nameof(ExitGroggyState), groggyDuration);
    }

    private void ExitGroggyState()
    {
        isGroggy = false;
        Debug.Log("그로기 종료!");
        animator.SetTrigger("Recover");
    }
}
