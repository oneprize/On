using UnityEngine;
using System.Collections;

public class ParryManager : MonoBehaviour
{
    public static ParryManager Instance;
    public PlayerMovement playerMovement;

    [Header("패링 창 기본값(초)")]
    public float defaultParryWindow = 0.4f;

    [Header("슬로우모션 연출")]
    public bool useSlowMotion = true;
    public float slowMotionScale = 0.2f;
    public float slowMotionDuration = 0.25f;

    [Header("플레이어 다운 리시버")]
    public PlayerHitReceiver playerHitReceiver;

    [Header("패링 성공 i-frame(초)")]
    public float parrySuccessIFrame = 0.12f;

    [Header("패링 유효 범위 (몬스터 정면 기준)")]
    public float parryRange = 3f;
    public float parryHalfAngle = 70f;

    // 강공격 여부에 따른 다운 효과
    [SerializeField] private bool isStrongAttack = false; // 강한 공격 여부를 저장할 변수

    // 패링 타이밍 관리
    public bool isParryWindow { get; private set; }
    private float parryStartUnscaled;
    private float activeWindowDuration;

    // 패링 대상 관리
    private GameObject targetEnemy;
    private EnemyWeapon targetWeapon;

    // 패링 성공 후 무적 관련 변수
    private float invulnUntilUnscaled = 0f;
    private bool parrySucceeded = false;

    // 이펙트/연출 등
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
            // (참고) 방어 중이라면 우클릭 패링 판정
            //if (playerMovement != null && playerMovement.IsDefending())
            if (Input.GetMouseButton(1))
            {
                float elapsed = Time.unscaledTime - parryStartUnscaled;
                if (elapsed <= activeWindowDuration && IsPlayerInParryRange())
                {
                    Debug.Log("우클릭 패링");
                    OnParrySuccess();
                    return; // 성공했으므로 추가 입력 처리하지 않고 종료
                }

            }
            // 클릭으로 패링 판정
            if (Input.GetButtonDown("Fire1"))
            {
                float elapsed = Time.unscaledTime - parryStartUnscaled;
                if (elapsed <= activeWindowDuration && IsPlayerInParryRange())
                {
                    Debug.Log("좌클릭 패링");
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

    // 몬스터 정면 유효 범위 안에 플레이어가 있는지 확인 (거리 + 각도)
    private bool IsPlayerInParryRange()
    {
        if (targetEnemy == null || playerMovement == null) return false;
        return CombatRange.IsInFrontArc(targetEnemy.transform, playerMovement.transform, parryRange, parryHalfAngle);
    }

    //---------------------------------------------------------
    // 패링 타이밍 창 시작
    //---------------------------------------------------------
    public void StartParryWindow(GameObject enemy, EnemyWeapon weapon, float duration, bool isStrong)
    {
        targetEnemy = enemy;
        targetWeapon = weapon;
        isStrongAttack = isStrong; // 강한 공격 여부를 미리 저장해둠

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

        // 슬로우모션 연출 발동
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

        // 1) 무기 판정 무효화
        if (targetWeapon != null)
        {
            targetWeapon.OnParried();
        }

        // 2) 몬스터 그로기
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

        // 강공격 여부에 따라 서로 다른 다운 효과를 발동
        if (playerHitReceiver != null)
        {
            if (isStrongAttack)
            {
                Debug.Log("Knockdown2");
                playerHitReceiver.TriggerKnockdown2();
            }
            else
            {
                Debug.Log("Knockdown1");
                playerHitReceiver.TriggerKnockdown();
            }
        }
        else
        {
            // playerHitReceiver 참조가 없는 경우 씬에서 찾아서 사용
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

    // 외부 노출용 (다른 스크립트에서 호출)
    public bool IsParryInvulnerable()
    {
        return Time.unscaledTime <= invulnUntilUnscaled;
    }
}
