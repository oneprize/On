using UnityEngine;

public class MonsterParryWindow : MonoBehaviour
{
    public float parryDuration = 0.3f;
    public EnemyParryReceiver enemyReceiver; // 그로기 담당
    public EnemyWeapon enemyWeapon;          // 이번 공격의 무기

    void Awake()
    {
        if (enemyReceiver == null)
            enemyReceiver = GetComponentInParent<EnemyParryReceiver>();
        if (enemyWeapon == null)
            enemyWeapon = GetComponentInChildren<EnemyWeapon>();
    }

    // 애니메이션 이벤트에서 호출
    public void AllowParryTiming()
    {
        if (!ParryManager.Instance || enemyReceiver == null || enemyWeapon == null)
        {
            Debug.LogWarning("[MonsterParryWindow] 참조 누락");
            return;
        }

        ParryManager.Instance.StartParryWindow(enemyReceiver.gameObject, enemyWeapon, parryDuration);
    }
}
