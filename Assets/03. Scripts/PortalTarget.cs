using UnityEngine;

public class PortalTarget : MonoBehaviour
{
    [Header("이동할 씬 이름 (Build Settings에 등록)")]
    public string targetSceneName;

    [Header("월드 프롬프트(UI) 선택")]
    public GameObject worldPrompt; // "E - 입장" 같은 World Space 캔버스 (없으면 비워둬도 됨)

    void Awake()
    {
        if (worldPrompt) worldPrompt.SetActive(false);
    }

    // PortalRayInteractor가 감지/해제 시 호출
    public void SetPromptVisible(bool on)
    {
        if (worldPrompt) worldPrompt.SetActive(on);
    }
}
