using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class CameraFX : MonoBehaviour
{
    [Header("Trauma Shake")]
    [Range(0f, 1f)] public float trauma = 0f;     // 0~1
    public float traumaDecay = 1.8f;              // �ʴ� ����
    public float maxPosShake = 0.20f;             // m
    public float maxRotShake = 7f;                // deg
    public float noiseFreq = 25f;

    [Header("Impulse (Kick)")]
    public float impulsePosDamping = 14f;         // Ŭ���� ���� ����
    public float impulseRotDamping = 14f;         // Ŭ���� ���� ����
    // ���޽��� ���� ����: ��(+x), ��(+y), ��(+z)

    [Header("FOV Kick")]
    public Camera targetCam;                      // ���� ������ �ڵ�
    public float fovReturnSpeed = 6f;             // �⺻ ���� �ӵ�

    // ���� ����
    Vector3 noiseSeed;
    Vector3 posImpulse;       // ���� ��ġ ���޽� ������(����)
    Vector3 rotImpulseEuler;  // ���� ȸ�� ���޽� ������(���� Euler)

    float baseFOV;
    float fovCurrent;

    void Awake()
    {
        noiseSeed = new Vector3(Random.value * 100f, Random.value * 100f, Random.value * 100f);
        if (targetCam == null) targetCam = GetComponent<Camera>();
        if (targetCam != null)
        {
            baseFOV = targetCam.fieldOfView;
            fovCurrent = baseFOV;
        }
    }

    public void AddTrauma(float amount)
    {
        trauma = Mathf.Clamp01(trauma + amount);
    }

    /// <summary>
    /// ���� ���� ���޽�(�ݵ�) �߰�. 
    /// localKick: ���� ����(��: �ڷ� ���� new Vector3(0,0,-1)).
    /// posAmp: ��ġ �ݵ� ũ��(���� ������), rotAmp: ȸ�� �ݵ� ũ��(��), duration�� ���� �ӵ� ����.
    /// </summary>
    public void AddImpulse(Vector3 localKick, float posAmp = 0.1f, float rotAmp = 2f, float duration = 0.12f)
    {
        // ª�� �ð� ���� �� ���� �ʱⰪ�� ��
        posImpulse += localKick.normalized * posAmp;
        // ���� z�� �ݵ��� ���� pitch�� �ɾ���(��/�˱� �� �ڷ� ������)
        rotImpulseEuler += new Vector3(localKick.y, -localKick.x, -localKick.z) * rotAmp;

        // duration�� ���� �ӵ��� ���� �ݿ�(ª������ �� ���� ����)
        float durScale = Mathf.Max(0.05f, duration);
        impulsePosDamping = Mathf.Max(impulsePosDamping, 10f / durScale);
        impulseRotDamping = Mathf.Max(impulseRotDamping, 10f / durScale);
    }

    /// <summary>
    /// FOV�� delta��ŭ ��� Ȯ�� �� õõ�� ����.
    /// ��) StartCoroutine(KickFOV(8f, 0.18f));
    /// </summary>
    public IEnumerator KickFOV(float delta, float inTime = 0.15f, float holdTime = 0f, float outTime = 0.25f)
    {
        if (targetCam == null) yield break;
        float start = fovCurrent;
        float peak = Mathf.Clamp(baseFOV + delta, 1f, 179f);

        // In
        float t = 0f;
        while (t < inTime)
        {
            t += Time.unscaledDeltaTime;
            float a = (inTime <= 0f) ? 1f : Mathf.Clamp01(t / inTime);
            fovCurrent = Mathf.Lerp(start, peak, a);
            yield return null;
        }

        // Hold
        t = 0f;
        while (t < holdTime)
        {
            t += Time.unscaledDeltaTime;
            fovCurrent = peak;
            yield return null;
        }

        // Out
        t = 0f;
        float from = fovCurrent;
        while (t < outTime)
        {
            t += Time.unscaledDeltaTime;
            float a = (outTime <= 0f) ? 1f : Mathf.Clamp01(t / outTime);
            fovCurrent = Mathf.Lerp(from, baseFOV, a);
            yield return null;
        }
    }

    void LateUpdate()
    {
        float dt = Time.deltaTime;

        // 1) Trauma ��� ������
        Vector3 shakePos = Vector3.zero;
        Vector3 shakeRot = Vector3.zero;
        if (trauma > 0f)
        {
            float t2 = trauma * trauma; // �������� ���� ����
            float nx = (Mathf.PerlinNoise(noiseSeed.x, Time.time * noiseFreq) * 2f - 1f);
            float ny = (Mathf.PerlinNoise(noiseSeed.y, Time.time * noiseFreq) * 2f - 1f);
            float nz = (Mathf.PerlinNoise(noiseSeed.z, Time.time * noiseFreq) * 2f - 1f);

            shakePos = new Vector3(nx, ny, 0f) * (maxPosShake * t2);
            shakeRot = new Vector3(nz, nx, ny) * (maxRotShake * t2);

            trauma = Mathf.Max(0f, trauma - traumaDecay * dt);
        }

        // 2) ���޽�(�ݵ�) ����(���� ����)
        float pLerp = 1f - Mathf.Exp(-impulsePosDamping * dt);
        float rLerp = 1f - Mathf.Exp(-impulseRotDamping * dt);
        posImpulse = Vector3.Lerp(posImpulse, Vector3.zero, pLerp);
        rotImpulseEuler = Vector3.Lerp(rotImpulseEuler, Vector3.zero, rLerp);

        // 3) ��� ����: ���� ��� ���Ѵ�
        transform.localPosition = posImpulse + shakePos;
        transform.localRotation = Quaternion.Euler(rotImpulseEuler + shakeRot);

        // 4) FOV ����(ű �ڷ�ƾ �� �Ϲ� ������ ����)
        if (targetCam != null)
        {
            // �ڷ�ƾ ���� ���̸� fovCurrent�� �̹� ���ŵ�
            // �ڷ�ƾ�� ������ õõ�� baseFOV�� ����
            fovCurrent = Mathf.Lerp(fovCurrent, baseFOV, 1f - Mathf.Exp(-fovReturnSpeed * Time.unscaledDeltaTime));
            targetCam.fieldOfView = fovCurrent;
        }
    }

    // ��Ȳ�� ���� �⺻ FOV�� �ٲٰ� ���� ��(�޸��� ��� ��)
    public void SetBaseFOV(float newBaseFOV)
    {
        baseFOV = Mathf.Clamp(newBaseFOV, 1f, 179f);
    }
}
