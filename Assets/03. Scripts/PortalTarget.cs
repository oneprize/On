using UnityEngine;

public class PortalTarget : MonoBehaviour
{
    [Header("�̵��� �� �̸� (Build Settings�� ���)")]
    public string targetSceneName;

    [Header("���� ������Ʈ(UI) ����")]
    public GameObject worldPrompt; // "E - ����" ���� World Space ĵ���� (������ ����ֵ� ��)

    void Awake()
    {
        if (worldPrompt) worldPrompt.SetActive(false);
    }

    // PortalRayInteractor�� ����/���� �� ȣ��
    public void SetPromptVisible(bool on)
    {
        if (worldPrompt) worldPrompt.SetActive(on);
    }
}
