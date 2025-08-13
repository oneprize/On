using UnityEngine;

public class MonsterAttackEvents : MonoBehaviour
{
    public EnemyWeapon weapon;

    // �и� ���� ����(�⺻ ����) ����/����
    // �ʿ��ϸ� �ִϸ��̼ǿ� �� �̺�Ʈ�� ��ġ
    public void Ev_StartParryableWindow()
    {
        if (weapon == null) return;
        weapon.useKnockdown2 = false;   // �и� ���� ���
        weapon.BeginAttackWindow();
    }

    public void Ev_EndParryableWindow()
    {
        if (weapon == null) return;
        weapon.EndAttackWindow();
    }

    // �и� �Ұ� �Ϲ� ����(������ �ٷ� knockdown2) ����/����
    // �̹� �� �� �̺�Ʈ�� ���� ��
    public void Ev_StartKnockdown2Window()
    {
        if (weapon == null) return;
        weapon.useKnockdown2 = true;    // �и� �Ұ� ���
        weapon.BeginAttackWindow();
    }

    public void Ev_EndKnockdown2Window()
    {
        if (weapon == null) return;
        weapon.EndAttackWindow();
        weapon.useKnockdown2 = false;   // ���� ������ ���� ����(����)
    }
}
