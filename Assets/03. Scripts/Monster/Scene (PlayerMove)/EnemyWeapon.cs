using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyWeapon : MonoBehaviour
{
    public MonsterAI monsterAI;

    public bool damageWindow { get; private set; } = false;
    public bool parriedThisSwing { get; private set; } = false;
    public bool isStrongAttack = false;

    // ���ο� �Լ� �߰�: MonsterAttackEvents���� ȣ���Ͽ� ���� ������ �����մϴ�.
    public void SetAttackType(bool isStrong)
    {
        this.isStrongAttack = isStrong;
    }

    public void BeginAttackWindow()
    {
        damageWindow = true;
        parriedThisSwing = false;

        if (monsterAI != null)
            monsterAI.isAttacking = true;
    }

    public void EndAttackWindow()
    {
        damageWindow = false;

        if (monsterAI != null)
            monsterAI.isAttacking = false;
    }

    public void OnParried()
    {
        if (parriedThisSwing) return;
        parriedThisSwing = true;
        damageWindow = false;
        if (monsterAI != null)
            monsterAI.EnterGroggy();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!damageWindow || parriedThisSwing) return;

        var hitRecv = other.GetComponentInParent<PlayerHitReceiver>();
        bool isPlayer = other.CompareTag("Player") || (hitRecv != null);
        if (!isPlayer || hitRecv == null) return;

        if (ParryManager.Instance != null && ParryManager.Instance.IsParryInvulnerable())
            return;

        if (ParryManager.Instance != null && ParryManager.Instance.isParryWindow)
        {
            ParryManager.Instance.FailParryDueToHit();
            damageWindow = false;
            return;
        }

        if (isStrongAttack)
        {
            hitRecv.TriggerKnockdown2();
        }
        else
        {
            hitRecv.TriggerKnockdown();
        }
        damageWindow = false;
    }
}