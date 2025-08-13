using UnityEngine;

public class MonsterAttackEvents : MonoBehaviour
{
    public EnemyWeapon weapon;

    // 패링 가능 스윙(기본 공격) 시작/종료
    // 필요하면 애니메이션에 이 이벤트를 배치
    public void Ev_StartParryableWindow()
    {
        if (weapon == null) return;
        weapon.useKnockdown2 = false;   // 패링 가능 모드
        weapon.BeginAttackWindow();
    }

    public void Ev_EndParryableWindow()
    {
        if (weapon == null) return;
        weapon.EndAttackWindow();
    }

    // 패링 불가 일반 공격(맞으면 바로 knockdown2) 시작/종료
    // 이번 건 이 이벤트를 쓰면 됨
    public void Ev_StartKnockdown2Window()
    {
        if (weapon == null) return;
        weapon.useKnockdown2 = true;    // 패링 불가 모드
        weapon.BeginAttackWindow();
    }

    public void Ev_EndKnockdown2Window()
    {
        if (weapon == null) return;
        weapon.EndAttackWindow();
        weapon.useKnockdown2 = false;   // 다음 스윙을 위해 리셋(선택)
    }
}
