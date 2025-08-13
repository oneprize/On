using UnityEngine;
using System.Collections;

public class ParryManager : MonoBehaviour
{
    public static ParryManager Instance;

    [Header("패링 창 기본값(초)")]
    public float defaultParryWindow = 0.4f;

    [Header("슬로우 모션")]
    public bool useSlowMotion = true;
    public float slowMotionScale = 0.2f;
    public float slowMotionDuration = 0.25f;

    [Header("플레이어 다운 연동")]
    public PlayerHitReceiver playerHitReceiver;

    [Header("패링 성공 i-frame(초)")]
    public float parrySuccessIFrame = 0.12f;

    public bool isParryWindow { get; private set; }
    private float parryStartUnscaled;
    private float activeWindowDuration;

    private GameObject targetEnemy;
    private EnemyWeapon targetWeapon;

    // 성공 후 무적 끝나는 시각(unscaled)
    private float invulnUntilUnscaled = 0f;
    // 이미 성공 처리했는지(중복 가드)
    private bool parrySucceeded = false;

    public GameObject parryVFXPrefab;
    public Transform vfxSpawnPoint;

    void Awake() { Instance = this; }

    void Update()
    {
        if (!isParryWindow) return;

        // 클릭형 성공 판정
        if (Input.GetButtonDown("Fire1"))
        {
            float elapsed = Time.unscaledTime - parryStartUnscaled;
            if (elapsed <= activeWindowDuration)
            {
                OnParrySuccess();
            }
        }

        if (Time.unscaledTime - parryStartUnscaled > activeWindowDuration)
            CloseParryWindow();
    }

    public void StartParryWindow(GameObject enemy, EnemyWeapon weapon, float duration)
    {
        targetEnemy = enemy;
        targetWeapon = weapon;

        activeWindowDuration = duration > 0f ? duration : defaultParryWindow;
        parryStartUnscaled = Time.unscaledTime;
        isParryWindow = true;
        parrySucceeded = false; // 창 열 때 초기화

        Debug.Log("패링 타이밍");

        if (useSlowMotion)
        {
            Time.timeScale = Mathf.Clamp(slowMotionScale, 0.01f, 1f);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            CancelInvoke(nameof(ResetTimeScale));
            Invoke(nameof(ResetTimeScale), slowMotionDuration);
        }
    }

    public void FailParryDueToHit()
    {
        if (!isParryWindow || parrySucceeded) return; // 성공 이후엔 무시
        OnParryFail();
    }

    private void OnParrySuccess()
    {
        if (parrySucceeded) return; // 중복 방지
        parrySucceeded = true;

        Debug.Log("패링 성공");

        Instantiate(parryVFXPrefab, vfxSpawnPoint.position, Quaternion.identity);

        // 1) 무기 즉시 무력화
        if (targetWeapon != null)
        {
            targetWeapon.OnParried();

            // 추가 안전장치: 아주 짧게 무기 콜라이더 OFF
            var wcol = targetWeapon.GetComponent<Collider>();
            if (wcol != null && wcol.enabled)
                StartCoroutine(DisableColliderBriefly(wcol, 0.1f));
        }

        // 2) 적 그로기
        if (targetEnemy)
        {
            var recv = targetEnemy.GetComponent<EnemyParryReceiver>();
            if (recv != null) recv.EnterGroggyState();
        }

        // 3) 플레이어 i-frame 부여(히트 무시)
        invulnUntilUnscaled = Time.unscaledTime + parrySuccessIFrame;

        CloseParryWindow();   // 창 먼저 닫아 실패 경로 차단
        ResetTimeScale();
    }

    private IEnumerator DisableColliderBriefly(Collider col, float secs)
    {
        col.enabled = false;
        yield return new WaitForSecondsRealtime(secs);
        col.enabled = true;
    }

    private void OnParryFail()
    {
        CloseParryWindow();
        ResetTimeScale();

        if (playerHitReceiver != null) playerHitReceiver.TriggerKnockdown();
        else
        {
            var fallback = FindAnyObjectByType<PlayerHitReceiver>();
            if (fallback != null) fallback.TriggerKnockdown();
        }
    }

    private void CloseParryWindow()
    {
        isParryWindow = false;
        targetEnemy = null;
        targetWeapon = null;
    }

    private void ResetTimeScale()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    // 외부 가드용
    public bool IsParryInvulnerable()
    {
        return Time.unscaledTime <= invulnUntilUnscaled;
    }
}
