using UnityEngine;

public class ParryTimingUI : MonoBehaviour
{
    [Tooltip("패링 타이밍일 때 켜질 UI(아이콘/패널 등)")]
    public GameObject indicatorRoot;

    [Tooltip("비워두면 자동으로 ParryManager.Instance를 참조")]
    public ParryManager parryManager;

    [Tooltip("체크 주기(초). 0이면 매 프레임 체크")]
    public float pollInterval = 0f;

    private float _nextPollTime;

    void Awake()
    {
        if (parryManager == null) parryManager = ParryManager.Instance;
        if (indicatorRoot != null) indicatorRoot.SetActive(false);
    }

    void OnEnable()
    {
        // 시작 시 현재 상태 반영
        ApplyIndicator(parryManager != null && parryManager.isParryWindow);
        _nextPollTime = 0f;
    }

    void Update()
    {
        if (parryManager == null || indicatorRoot == null) return;

        if (pollInterval > 0f)
        {
            if (Time.unscaledTime < _nextPollTime) return;
            _nextPollTime = Time.unscaledTime + pollInterval;
        }

        bool shouldShow = parryManager.isParryWindow;
        if (indicatorRoot.activeSelf != shouldShow)
            indicatorRoot.SetActive(shouldShow);
    }

    private void ApplyIndicator(bool on)
    {
        if (indicatorRoot != null)
            indicatorRoot.SetActive(on);
    }
}
