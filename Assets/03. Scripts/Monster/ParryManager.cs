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

    public bool isParryWindow { get; private set; }
    private float parryStartUnscaled;
    private float activeWindowDuration;

    private GameObject targetEnemy;
    private EnemyWeapon targetWeapon;

    // ���� �� ���� ������ �ð�(unscaled)
    private float invulnUntilUnscaled = 0f;
    // �̹� ���� ó���ߴ���(�ߺ� ����)
    private bool parrySucceeded = false;

    public GameObject parryVFXPrefab;
    public Transform vfxSpawnPoint;

    void Awake() { Instance = this; }

    void Update()
    {
        if (!isParryWindow) return;

        // Ŭ���� ���� ����
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
        parrySucceeded = false; // â �� �� �ʱ�ȭ

        Debug.Log("�и� Ÿ�̹�");

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
        if (!isParryWindow || parrySucceeded) return; // ���� ���Ŀ� ����
        OnParryFail();
    }

    private void OnParrySuccess()
    {
        if (parrySucceeded) return; // �ߺ� ����
        parrySucceeded = true;

        Debug.Log("�и� ����");

        Instantiate(parryVFXPrefab, vfxSpawnPoint.position, Quaternion.identity);

        // 1) ���� ��� ����ȭ
        if (targetWeapon != null)
        {
            targetWeapon.OnParried();

            // �߰� ������ġ: ���� ª�� ���� �ݶ��̴� OFF
            var wcol = targetWeapon.GetComponent<Collider>();
            if (wcol != null && wcol.enabled)
                StartCoroutine(DisableColliderBriefly(wcol, 0.1f));
        }

        // 2) �� �׷α�
        if (targetEnemy)
        {
            var recv = targetEnemy.GetComponent<EnemyParryReceiver>();
            if (recv != null) recv.EnterGroggyState();
        }

        // 3) �÷��̾� i-frame �ο�(��Ʈ ����)
        invulnUntilUnscaled = Time.unscaledTime + parrySuccessIFrame;

        CloseParryWindow();   // â ���� �ݾ� ���� ��� ����
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

    // �ܺ� �����
    public bool IsParryInvulnerable()
    {
        return Time.unscaledTime <= invulnUntilUnscaled;
    }
}
