using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyWeapon : MonoBehaviour
{
    public MonsterAI monsterAI;

    // �ִϸ��̼� �̺�Ʈ�� �Ѱ� ���� ��ȿ ��Ʈ ����
    public bool damageWindow { get; private set; } = false;

    // �̹� ������ �̹� �и����� ����ȭ�ƴ���
    public bool parriedThisSwing { get; private set; } = false;

    // �� ������ '�и� �Ұ� �Ϲ� ����(knockdown2)' �������
    public bool useKnockdown2 = false;

    public void BeginAttackWindow()
    {
        damageWindow = true;
        parriedThisSwing = false;
        // useKnockdown2�� MonsterAttackEvents���� �������� ����
    }

    public void EndAttackWindow()
    {
        damageWindow = false;
    }

    // �и� ���� �� �÷��̾�/�и� �Ŵ��� �ʿ��� ȣ��
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

        // �÷��̾� ����(�ڽ� �ݶ��̴� ����)
        var hitRecv = other.GetComponentInParent<PlayerHitReceiver>();
        bool isPlayer = other.CompareTag("Player") || (hitRecv != null);
        if (!isPlayer) return;
        if (hitRecv == null) return;

        // -------- �и� �Ұ�/�Ϲ� ����(= knockdown2) ��� --------
        if (useKnockdown2)
        {
            hitRecv.TriggerKnockdown2(); // �и�/���� �˻� ���� ���
            damageWindow = false;
            return;
        }

        // -------- �и� ���� ���� ��� --------

        // �и� ���� ������ ª�� �����̸� ����
        if (ParryManager.Instance != null && ParryManager.Instance.IsParryInvulnerable())
            return;

        // �и� â�� ���� �ִµ� �Է� ���� ���� �� ���� ó��(�Ѿ���)
        if (ParryManager.Instance != null && ParryManager.Instance.isParryWindow)
        {
            ParryManager.Instance.FailParryDueToHit();
            damageWindow = false;
            return;
        }

        // �и� â�� �ƴϸ� �Ϲ� �Ѿ���
        hitRecv.TriggerKnockdown();
        damageWindow = false;
    }
}
