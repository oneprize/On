using UnityEngine;

public class MonsterAttackEvents : MonoBehaviour
{
    public EnemyParryReceiver enemyReceiver;
    public EnemyWeapon weapon;

    public void Ev_StartParryableWindow()
    {
        if (ParryManager.Instance == null || weapon == null || enemyReceiver == null) return;

        // 1. EnemyWeapon�� �Ϲ� �������� �˸��ϴ�.
        weapon.SetAttackType(false);
        // 2. �и� Ÿ�̹��� �����մϴ�.
        ParryManager.Instance.StartParryWindow(enemyReceiver.gameObject, weapon, 0.4f, false);
    }

    public void Ev_StartKnockdown2Window()
    {
        if (ParryManager.Instance == null || weapon == null || enemyReceiver == null) return;

        // 1. EnemyWeapon�� ���� �������� �˸��ϴ�.
        weapon.SetAttackType(true);
        // 2. �и� Ÿ�̹��� �����մϴ�.
        ParryManager.Instance.StartParryWindow(enemyReceiver.gameObject, weapon, 0.4f, true);
    }

    public void Ev_EndAttackWindow()
    {
        if (weapon == null) return;
        weapon.EndAttackWindow();
    }
}