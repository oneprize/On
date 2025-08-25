using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyWeapon : MonoBehaviour
{
    public MonsterAI monsterAI;

    public bool damageWindow { get; private set; } = false;
    public bool parriedThisSwing { get; private set; } = false;
    public bool isStrongAttack = false;

    // 새로운 함수 추가: MonsterAttackEvents에서 호출하여 공격 유형을 설정합니다.
    public void SetAttackType(bool isStrong)
    {
        this.isStrongAttack = isStrong;
    }

    public void BeginAttackWindow()
    {
        damageWindow = true;
        parriedThisSwing = false;
    }

    public void EndAttackWindow()
    {
        damageWindow = false;
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