using UnityEngine;

public class PlayerParryControl : MonoBehaviour
{
    public Animator animator;                     // Slash �ִϸ��̼ǿ�
    public GameObject CurrentParryTarget;         // ���� �и� Ÿ�� (�и� �Է� ������)
    public bool isParryAvailable = false;         // �и� Ÿ�̹� �÷���
    public GameObject breakEffectPrefab;          // �ı� ����Ʈ

    [Header("�и� ���� �� �ı��� �� ������Ʈ")]
    public GameObject stoneToBreak;               // �и� ���� �� �μ� �� (Inspector���� ����)

    [Header("�и� ���� �� Ȱ��ȭ�� ������Ʈ")]
    public GameObject objectToActivate;           // �� �μ� �� Ȱ��ȭ�� ������Ʈ

    void Update()
    {
        if (isParryAvailable && Input.GetMouseButtonDown(0))
        {
            Parry();
        }
    }

    void Parry()
    {
        Debug.Log("�и� �Էµ�! Slash �ִϸ��̼� ���");

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        animator.SetTrigger("Slash"); // Slash �ִϸ��̼� ���

        // stoneToBreak �ı�
        if (stoneToBreak != null)
        {
            Vector3 pos = stoneToBreak.transform.position;

            if (breakEffectPrefab != null)
                Instantiate(breakEffectPrefab, pos, Quaternion.identity);

            // ������ �ı� ���
            ParryTrigger trigger = stoneToBreak.GetComponent<ParryTrigger>();
            if (trigger != null)
            {
                trigger.Break();
            }

            Debug.Log(stoneToBreak.name + " �ı� �Ϸ�");
        }
        else
        {
            Debug.LogWarning("stoneToBreak�� �������� ����!");
        }

        // �и� Ÿ�� �ʱ�ȭ
        CurrentParryTarget = null;
        isParryAvailable = false;

        // �и� ���� �� ������Ʈ Ȱ��ȭ
        if (objectToActivate != null && !objectToActivate.activeSelf)
        {
            objectToActivate.SetActive(true);
            Debug.Log(objectToActivate.name + " ������Ʈ Ȱ��ȭ��!");
        }
    }
}
