using UnityEngine;

public class PlayerHitReceiver : MonoBehaviour
{
    [Header("�ִϸ����� ����")]
    public Animator playerAnimator;
    public string knockdownTriggerName = "knockdown";

    [Header("���� ���� ���")]
    public float rehitLockTime = 0.2f;

    private bool hitLocked = false;

    // ���� �浹�ε� �ٿ��� �ɰ� ���� �� ����
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
        // Ÿ�̹� ���� �� �ܺο��� ���� �ٿ��� �� �� ȣ��
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
