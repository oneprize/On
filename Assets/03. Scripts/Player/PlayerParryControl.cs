using UnityEngine;

public class PlayerParryControl : MonoBehaviour
{
    public Animator animator;                     // Slash 애니메이션용
    public GameObject CurrentParryTarget;         // 현재 패링 타겟 (패링 입력 감지용)
    public bool isParryAvailable = false;         // 패링 타이밍 플래그
    public GameObject breakEffectPrefab;          // 파괴 이펙트

    [Header("패링 성공 시 파괴할 돌 오브젝트")]
    public GameObject stoneToBreak;               // 패링 성공 후 부술 돌 (Inspector에서 지정)

    [Header("패링 성공 후 활성화할 오브젝트")]
    public GameObject objectToActivate;           // 돌 부순 후 활성화할 오브젝트

    void Update()
    {
        if (isParryAvailable && Input.GetMouseButtonDown(0))
        {
            Parry();
        }
    }

    void Parry()
    {
        Debug.Log("패링 입력됨! Slash 애니메이션 재생");

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        animator.SetTrigger("Slash"); // Slash 애니메이션 재생

        // stoneToBreak 파괴
        if (stoneToBreak != null)
        {
            Vector3 pos = stoneToBreak.transform.position;

            if (breakEffectPrefab != null)
                Instantiate(breakEffectPrefab, pos, Quaternion.identity);

            // 돌에게 파괴 명령
            ParryTrigger trigger = stoneToBreak.GetComponent<ParryTrigger>();
            if (trigger != null)
            {
                trigger.Break();
            }

            Debug.Log(stoneToBreak.name + " 파괴 완료");
        }
        else
        {
            Debug.LogWarning("stoneToBreak가 지정되지 않음!");
        }

        // 패링 타겟 초기화
        CurrentParryTarget = null;
        isParryAvailable = false;

        // 패링 성공 후 오브젝트 활성화
        if (objectToActivate != null && !objectToActivate.activeSelf)
        {
            objectToActivate.SetActive(true);
            Debug.Log(objectToActivate.name + " 오브젝트 활성화됨!");
        }
    }
}
