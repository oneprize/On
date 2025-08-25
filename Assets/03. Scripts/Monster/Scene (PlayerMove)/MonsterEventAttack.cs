using UnityEngine;

public class MonsterAttackEvents : MonoBehaviour
{
    public EnemyParryReceiver enemyReceiver;
    public EnemyWeapon weapon;

    public void Ev_StartParryableWindow()
    {
        if (ParryManager.Instance == null || weapon == null || enemyReceiver == null) return;

        // 1. EnemyWeapon에 일반 공격임을 알립니다.
        weapon.SetAttackType(false);
        // 2. 패리 타이밍을 시작합니다.
        ParryManager.Instance.StartParryWindow(enemyReceiver.gameObject, weapon, 0.4f, false);
    }

    public void Ev_StartKnockdown2Window()
    {
        if (ParryManager.Instance == null || weapon == null || enemyReceiver == null) return;

        // 1. EnemyWeapon에 강한 공격임을 알립니다.
        weapon.SetAttackType(true);
        // 2. 패리 타이밍을 시작합니다.
        ParryManager.Instance.StartParryWindow(enemyReceiver.gameObject, weapon, 0.4f, true);
    }

    public void Ev_EndAttackWindow()
    {
        if (weapon == null) return;
        weapon.EndAttackWindow();
    }
}