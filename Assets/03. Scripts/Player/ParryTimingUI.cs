using UnityEngine;

public class ParryTimingUI : MonoBehaviour
{
    [Tooltip("�и� Ÿ�̹��� �� ���� UI(������/�г� ��)")]
    public GameObject indicatorRoot;

    [Tooltip("����θ� �ڵ����� ParryManager.Instance�� ����")]
    public ParryManager parryManager;

    [Tooltip("üũ �ֱ�(��). 0�̸� �� ������ üũ")]
    public float pollInterval = 0f;

    private float _nextPollTime;

    void Awake()
    {
        if (parryManager == null) parryManager = ParryManager.Instance;
        if (indicatorRoot != null) indicatorRoot.SetActive(false);
    }

    void OnEnable()
    {
        // ���� �� ���� ���� �ݿ�
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
