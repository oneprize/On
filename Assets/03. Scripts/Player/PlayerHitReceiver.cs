using UnityEngine;

public class PlayerHitReceiver : MonoBehaviour
{
    [Header("애니메이터 참조")]
    public Animator playerAnimator;
    public string knockdownTriggerName = "knockdown";
    private PlayerMovement playerMovement;

    [Header("연속 피격 방지 시간")]
    public float rehitLockTime = 0.2f;

    private bool hitLocked = false;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    // 공격 애니메이션이 재생 중인지 isAttacking 플래그와 애니메이터 태그 둘 다로 확인한다.
    // (isAttacking 플래그만으로는 타이밍이 어긋나 콤보 도중 잘못 풀리는 경우가 있어 애니메이터 상태로 한 번 더 검증)
    private bool IsPlayerAttacking()
    {
        bool flagSaysAttacking = playerMovement != null && playerMovement.IsAttacking();
        bool animSaysAttacking = playerAnimator != null && playerAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
        return flagSaysAttacking || animSaysAttacking;
    }

    // 무기 콜라이더로 몸에 맞았을 때 실행
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root == transform.root) return;

        var weapon = other.GetComponent<EnemyWeapon>();
        if (weapon == null) return;

        if (weapon.damageWindow && !weapon.parriedThisSwing)
        {
            if (weapon.isStrongAttack)
            {
                // 강공격은 공격 중이라도 무시하지 않고 다운
                TriggerKnockdown2();
            }
            else
            {
                // 일반 공격은 공격 중일 때 무시
                if (IsPlayerAttacking())
                {
                    Debug.Log("[HitReceiver] 공격 중이므로 일반 Knockdown 무시");
                    return;
                }
                TriggerKnockdown();
            }
        }
    }

    public void TriggerKnockdown()
    {
        TryKnockdownOnce();
    }

    public void TriggerKnockdown2()
    {
        if (playerAnimator != null)
            playerAnimator.SetTrigger("knockdown2");
    }

    private void TryKnockdownOnce()
    {
        // 공격 중이라면 knockdown 무시
        if (IsPlayerAttacking())
        {
            Debug.Log("[HitReceiver] 공격 중이라 Knockdown 무시");
            return;
        }

        if (hitLocked) return;
        hitLocked = true;

        if (playerAnimator != null && !string.IsNullOrEmpty(knockdownTriggerName))
            playerAnimator.SetTrigger(knockdownTriggerName);

        Invoke(nameof(UnlockHit), rehitLockTime);
    }

    private void UnlockHit()
    {
        hitLocked = false;
    }
}
