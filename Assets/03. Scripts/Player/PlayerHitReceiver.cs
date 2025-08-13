using UnityEngine;

public class PlayerHitReceiver : MonoBehaviour
{
    [Header("애니메이터 연동")]
    public Animator playerAnimator;
    public string knockdownTriggerName = "knockdown";

    [Header("연속 판정 잠금")]
    public float rehitLockTime = 0.2f;

    private bool hitLocked = false;

    // 무기 충돌로도 다운을 걸고 싶을 때 유지
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root == transform.root) return;

        var weapon = other.GetComponent<EnemyWeapon>();
        if (weapon == null) return;

        if (weapon.damageWindow && !weapon.parriedThisSwing)
        {
            TryKnockdownOnce();
        }
    }

    public void TriggerKnockdown()
    {
        // 타이밍 실패 등 외부에서 직접 다운을 걸 때 호출
        TryKnockdownOnce();
    }
    public void TriggerKnockdown2()
    {
        if (playerAnimator != null)
            playerAnimator.SetTrigger("knockdown2");
    }

    private void TryKnockdownOnce()
    {
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
