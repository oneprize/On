using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyWeapon : MonoBehaviour
{
    public MonsterAI monsterAI;

    // 애니메이션 이벤트로 켜고 끄는 유효 히트 구간
    public bool damageWindow { get; private set; } = false;

    // 이번 스윙이 이미 패링으로 무력화됐는지
    public bool parriedThisSwing { get; private set; } = false;

    // 이 스윙이 '패링 불가 일반 공격(knockdown2)' 모드인지
    public bool useKnockdown2 = false;

    public void BeginAttackWindow()
    {
        damageWindow = true;
        parriedThisSwing = false;
        // useKnockdown2는 MonsterAttackEvents에서 스윙별로 설정
    }

    public void EndAttackWindow()
    {
        damageWindow = false;
    }

    // 패링 성공 시 플레이어/패링 매니저 쪽에서 호출
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

        // 플레이어 판정(자식 콜라이더 대응)
        var hitRecv = other.GetComponentInParent<PlayerHitReceiver>();
        bool isPlayer = other.CompareTag("Player") || (hitRecv != null);
        if (!isPlayer) return;
        if (hitRecv == null) return;

        // -------- 패링 불가/일반 공격(= knockdown2) 경로 --------
        if (useKnockdown2)
        {
            hitRecv.TriggerKnockdown2(); // 패링/무적 검사 없이 즉시
            damageWindow = false;
            return;
        }

        // -------- 패링 가능 공격 경로 --------

        // 패링 성공 직후의 짧은 무적이면 무시
        if (ParryManager.Instance != null && ParryManager.Instance.IsParryInvulnerable())
            return;

        // 패링 창이 열려 있는데 입력 없이 맞음 → 실패 처리(넘어짐)
        if (ParryManager.Instance != null && ParryManager.Instance.isParryWindow)
        {
            ParryManager.Instance.FailParryDueToHit();
            damageWindow = false;
            return;
        }

        // 패링 창이 아니면 일반 넘어짐
        hitRecv.TriggerKnockdown();
        damageWindow = false;
    }
}
