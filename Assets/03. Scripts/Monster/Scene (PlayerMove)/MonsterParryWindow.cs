using UnityEngine;

public class MonsterParryWindow : MonoBehaviour
{
    public float parryDuration = 0.3f;
    public EnemyParryReceiver enemyReceiver; // �׷α� ���
    public EnemyWeapon enemyWeapon;          // �̹� ������ ����

    void Awake()
    {
        if (enemyReceiver == null)
            enemyReceiver = GetComponentInParent<EnemyParryReceiver>();
        if (enemyWeapon == null)
            enemyWeapon = GetComponentInChildren<EnemyWeapon>();
    }

    // �ִϸ��̼� �̺�Ʈ���� ȣ��
    public void AllowParryTiming()
    {
        if (!ParryManager.Instance || enemyReceiver == null || enemyWeapon == null)
        {
            Debug.LogWarning("[MonsterParryWindow] ���� ����");
            return;
        }

        ParryManager.Instance.StartParryWindow(enemyReceiver.gameObject, enemyWeapon, parryDuration);
    }
}
