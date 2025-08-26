using UnityEngine;

public class PlayerHitReceiver : MonoBehaviour
{
    [Header("애니메이터 연동")]
    public Animator playerAnimator;
    public string knockdownTriggerName = "knockdown";
    private PlayerMovement playerMovement;

    [Header("연속 판정 잠금")]
    public float rehitLockTime = 0.2f;

    private bool hitLocked = false;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    // 무기 충돌로도 다운을 걸고 싶을 때 유지
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root == transform.root) return;

        var weapon = other.GetComponent<EnemyWeapon>();
        if (weapon == null) return;

        if (weapon.damageWindow && !weapon.parriedThisSwing)
        {
            if (weapon.isStrongAttack)
            {
                // 강공격은 공격 중이라도 무조건 적용
                TriggerKnockdown2();
            }
            else
            {
                // 일반 공격은 공격 중일 때 무시
                if (playerMovement != null && playerMovement.IsAttacking())
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
        if (playerMovement != null && playerMovement.IsAttacking())
        {
            Debug.Log("[HitReceiver] 공격 중이라서 Knockdown 무시");
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