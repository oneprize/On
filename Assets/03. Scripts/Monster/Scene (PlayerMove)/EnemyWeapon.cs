using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyWeapon : MonoBehaviour
{
    public MonsterAI monsterAI;

    [Header("공격 유효 범위 (몬스터 정면 기준)")]
    public float hitRange = 2.5f;
    public float hitHalfAngle = 70f;

    public bool damageWindow { get; private set; } = false;
    public bool parriedThisSwing { get; private set; } = false;
    public bool isStrongAttack = false;

    // 새로운 함수 추가: MonsterAttackEvents에서 호출하여 공격 유형을 지정합니다.
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

        // 콜라이더가 닿았어도 몬스터 정면 유효 범위 밖이면(무기 궤적이 우연히 스친 경우 등) 무시한다.
        Transform monsterTransform = monsterAI != null ? monsterAI.transform : transform;
        if (!CombatRange.IsInFrontArc(monsterTransform, hitRecv.transform, hitRange, hitHalfAngle))
            return;

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