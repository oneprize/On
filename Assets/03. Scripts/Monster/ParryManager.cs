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

    // 공격 유형에 따른 다운 효과
    [SerializeField] private bool isStrongAttack = false; // 강한 공격 여부를 저장할 변수

    // 패링 타이밍 관리
    public bool isParryWindow { get; private set; }
    private float parryStartUnscaled;
    private float activeWindowDuration;

    // 패링 대상 관리
    private GameObject targetEnemy;
    private EnemyWeapon targetWeapon;

    // 패링 성공 및 무적 상태 관리
    private float invulnUntilUnscaled = 0f;
    private bool parrySucceeded = false;

    // 이펙트/사운드 등
    public GameObject parryVFXPrefab;
    public Transform vfxSpawnPoint;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 필요한 경우 주석 해제
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // 패링 타이밍 중 플레이어 입력 감지
        if (isParryWindow)
        {
            // 클릭형 성공 판정
            if (Input.GetButtonDown("Fire1"))
            {
                float elapsed = Time.unscaledTime - parryStartUnscaled;
                if (elapsed <= activeWindowDuration)
                {
                    OnParrySuccess();
                }
            }
            // 패링 타이밍 창이 끝나면 창 닫기
            if (Time.unscaledTime - parryStartUnscaled > activeWindowDuration)
            {
                CloseParryWindow();
            }
        }
    }

    //---------------------------------------------------------
    // 패링 관리 로직
    //---------------------------------------------------------
    public void StartParryWindow(GameObject enemy, EnemyWeapon weapon, float duration, bool isStrong)
    {
        targetEnemy = enemy;
        targetWeapon = weapon;
        isStrongAttack = isStrong; // 강한 공격 여부를 이 변수에 저장

        activeWindowDuration = duration > 0f ? duration : defaultParryWindow;
        parryStartUnscaled = Time.unscaledTime;
        isParryWindow = true;
        parrySucceeded = false;

        Debug.Log("패링 타이밍 시작");
    }

    // 패링 성공 시 호출되는 함수
    private void OnParrySuccess()
    {
        if (parrySucceeded) return; // 중복 방지
        parrySucceeded = true;

        Debug.Log("패링 성공");

        // 슬로우 모션 발동
        if (useSlowMotion)
        {
            Time.timeScale = Mathf.Clamp(slowMotionScale, 0.01f, 1f);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            CancelInvoke(nameof(ResetTimeScale));
            Invoke(nameof(ResetTimeScale), slowMotionDuration);
        }

        // 이펙트 생성
        if (parryVFXPrefab != null && vfxSpawnPoint != null)
        {
            Instantiate(parryVFXPrefab, vfxSpawnPoint.position, Quaternion.identity);
        }

        // 1) 무기 즉시 무력화
        if (targetWeapon != null)
        {
            targetWeapon.OnParried();
        }

        // 2) 적 그로기
        if (targetEnemy)
        {
            var recv = targetEnemy.GetComponent<EnemyParryReceiver>();
            if (recv != null)
            {
                recv.EnterGroggyState();
            }
        }

        // 3) 플레이어 i-frame 부여
        invulnUntilUnscaled = Time.unscaledTime + parrySuccessIFrame;

        CloseParryWindow();
    }

    // 패링 타이밍 중 맞았을 때 호출
    public void FailParryDueToHit()
    {
        if (!isParryWindow || parrySucceeded) return;
        OnParryFail();
    }

    private void OnParryFail()
    {
        CloseParryWindow();
        ResetTimeScale();

        // 몬스터의 공격 유형에 따라 다른 다운 효과를 발동
        if (playerHitReceiver != null)
        {
            if (isStrongAttack)
            {
                playerHitReceiver.TriggerKnockdown2();
            }
            else
            {
                playerHitReceiver.TriggerKnockdown();
            }
        }
        else
        {
            // playerHitReceiver 참조가 없는 경우 폴백 로직
            var fallback = FindAnyObjectByType<PlayerHitReceiver>();
            if (fallback != null)
            {
                if (isStrongAttack)
                {
                    fallback.TriggerKnockdown2();
                }
                else
                {
                    fallback.TriggerKnockdown();
                }
            }
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

    // 외부 가드용 (다른 스크립트에서 호출)
    public bool IsParryInvulnerable()
    {
        return Time.unscaledTime <= invulnUntilUnscaled;
    }
}