using UnityEngine;
using System.Collections;

public class ParryManager : MonoBehaviour
{
    public static ParryManager Instance;

    [Header("�и� â �⺻��(��)")]
    public float defaultParryWindow = 0.4f;

    [Header("���ο� ���")]
    public bool useSlowMotion = true;
    public float slowMotionScale = 0.2f;
    public float slowMotionDuration = 0.25f;

    [Header("�÷��̾� �ٿ� ����")]
    public PlayerHitReceiver playerHitReceiver;

    [Header("�и� ���� i-frame(��)")]
    public float parrySuccessIFrame = 0.12f;

    // ���� ������ ���� �ٿ� ȿ��
    [SerializeField] private bool isStrongAttack = false; // ���� ���� ���θ� ������ ����

    // �и� Ÿ�̹� ����
    public bool isParryWindow { get; private set; }
    private float parryStartUnscaled;
    private float activeWindowDuration;

    // �и� ��� ����
    private GameObject targetEnemy;
    private EnemyWeapon targetWeapon;

    // �и� ���� �� ���� ���� ����
    private float invulnUntilUnscaled = 0f;
    private bool parrySucceeded = false;

    // ����Ʈ/���� ��
    public GameObject parryVFXPrefab;
    public Transform vfxSpawnPoint;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // �ʿ��� ��� �ּ� ����
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // �и� Ÿ�̹� �� �÷��̾� �Է� ����
        if (isParryWindow)
        {
            // Ŭ���� ���� ����
            if (Input.GetButtonDown("Fire1"))
            {
                float elapsed = Time.unscaledTime - parryStartUnscaled;
                if (elapsed <= activeWindowDuration)
                {
                    OnParrySuccess();
                }
            }
            // �и� Ÿ�̹� â�� ������ â �ݱ�
            if (Time.unscaledTime - parryStartUnscaled > activeWindowDuration)
            {
                CloseParryWindow();
            }
        }
    }

    //---------------------------------------------------------
    // �и� ���� ����
    //---------------------------------------------------------
    public void StartParryWindow(GameObject enemy, EnemyWeapon weapon, float duration, bool isStrong)
    {
        targetEnemy = enemy;
        targetWeapon = weapon;
        isStrongAttack = isStrong; // ���� ���� ���θ� �� ������ ����

        activeWindowDuration = duration > 0f ? duration : defaultParryWindow;
        parryStartUnscaled = Time.unscaledTime;
        isParryWindow = true;
        parrySucceeded = false;

        Debug.Log("�и� Ÿ�̹� ����");
    }

    // �и� ���� �� ȣ��Ǵ� �Լ�
    private void OnParrySuccess()
    {
        if (parrySucceeded) return; // �ߺ� ����
        parrySucceeded = true;

        Debug.Log("�и� ����");

        // ���ο� ��� �ߵ�
        if (useSlowMotion)
        {
            Time.timeScale = Mathf.Clamp(slowMotionScale, 0.01f, 1f);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            CancelInvoke(nameof(ResetTimeScale));
            Invoke(nameof(ResetTimeScale), slowMotionDuration);
        }

        // ����Ʈ ����
        if (parryVFXPrefab != null && vfxSpawnPoint != null)
        {
            Instantiate(parryVFXPrefab, vfxSpawnPoint.position, Quaternion.identity);
        }

        // 1) ���� ��� ����ȭ
        if (targetWeapon != null)
        {
            targetWeapon.OnParried();
        }

        // 2) �� �׷α�
        if (targetEnemy)
        {
            var recv = targetEnemy.GetComponent<EnemyParryReceiver>();
            if (recv != null)
            {
                recv.EnterGroggyState();
            }
        }

        // 3) �÷��̾� i-frame �ο�
        invulnUntilUnscaled = Time.unscaledTime + parrySuccessIFrame;

        CloseParryWindow();
    }

    // �и� Ÿ�̹� �� �¾��� �� ȣ��
    public void FailParryDueToHit()
    {
        if (!isParryWindow || parrySucceeded) return;
        OnParryFail();
    }

    private void OnParryFail()
    {
        CloseParryWindow();
        ResetTimeScale();

        // ������ ���� ������ ���� �ٸ� �ٿ� ȿ���� �ߵ�
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
            // playerHitReceiver ������ ���� ��� ���� ����
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

    // �ܺ� ����� (�ٸ� ��ũ��Ʈ���� ȣ��)
    public bool IsParryInvulnerable()
    {
        return Time.unscaledTime <= invulnUntilUnscaled;
    }
}