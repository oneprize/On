using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class CameraFX : MonoBehaviour
{
    [Header("Trauma Shake")]
    [Range(0f, 1f)] public float trauma = 0f;     // 0~1
    public float traumaDecay = 1.8f;              // 초당 감소
    public float maxPosShake = 0.20f;             // m
    public float maxRotShake = 7f;                // deg
    public float noiseFreq = 25f;

    [Header("Impulse (Kick)")]
    public float impulsePosDamping = 14f;         // 클수록 빨리 감쇠
    public float impulseRotDamping = 14f;         // 클수록 빨리 감쇠
    // 임펄스는 로컬 기준: 좌(+x), 상(+y), 앞(+z)

    [Header("FOV Kick")]
    public Camera targetCam;                      // 지정 없으면 자동
    public float fovReturnSpeed = 6f;             // 기본 복귀 속도

    // 내부 상태
    Vector3 noiseSeed;
    Vector3 posImpulse;       // 현재 위치 임펄스 오프셋(로컬)
    Vector3 rotImpulseEuler;  // 현재 회전 임펄스 오프셋(로컬 Euler)

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
    /// 로컬 기준 임펄스(반동) 추가. 
    /// localKick: 로컬 방향(예: 뒤로 차면 new Vector3(0,0,-1)).
    /// posAmp: 위치 반동 크기(미터 스케일), rotAmp: 회전 반동 크기(도), duration은 감쇠 속도 보정.
    /// </summary>
    public void AddImpulse(Vector3 localKick, float posAmp = 0.1f, float rotAmp = 2f, float duration = 0.12f)
    {
        // 짧은 시간 동안 더 강한 초기값을 줌
        posImpulse += localKick.normalized * posAmp;
        // 로컬 z축 반동은 보통 pitch에 걸어줌(총/검기 등 뒤로 젖혀짐)
        rotImpulseEuler += new Vector3(localKick.y, -localKick.x, -localKick.z) * rotAmp;

        // duration은 감쇠 속도에 간접 반영(짧을수록 더 빨리 감쇠)
        float durScale = Mathf.Max(0.05f, duration);
        impulsePosDamping = Mathf.Max(impulsePosDamping, 10f / durScale);
        impulseRotDamping = Mathf.Max(impulseRotDamping, 10f / durScale);
    }

    /// <summary>
    /// FOV를 delta만큼 즉시 확대 후 천천히 복귀.
    /// 예) StartCoroutine(KickFOV(8f, 0.18f));
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

        // 1) Trauma 기반 노이즈
        Vector3 shakePos = Vector3.zero;
        Vector3 shakeRot = Vector3.zero;
        if (trauma > 0f)
        {
            float t2 = trauma * trauma; // 제곱으로 강도 제어
            float nx = (Mathf.PerlinNoise(noiseSeed.x, Time.time * noiseFreq) * 2f - 1f);
            float ny = (Mathf.PerlinNoise(noiseSeed.y, Time.time * noiseFreq) * 2f - 1f);
            float nz = (Mathf.PerlinNoise(noiseSeed.z, Time.time * noiseFreq) * 2f - 1f);

            shakePos = new Vector3(nx, ny, 0f) * (maxPosShake * t2);
            shakeRot = new Vector3(nz, nx, ny) * (maxRotShake * t2);

            trauma = Mathf.Max(0f, trauma - traumaDecay * dt);
        }

        // 2) 임펄스(반동) 감쇠(지수 감쇠)
        float pLerp = 1f - Mathf.Exp(-impulsePosDamping * dt);
        float rLerp = 1f - Mathf.Exp(-impulseRotDamping * dt);
        posImpulse = Vector3.Lerp(posImpulse, Vector3.zero, pLerp);
        rotImpulseEuler = Vector3.Lerp(rotImpulseEuler, Vector3.zero, rLerp);

        // 3) 결과 적용: 로컬 포즈에 더한다
        transform.localPosition = posImpulse + shakePos;
        transform.localRotation = Quaternion.Euler(rotImpulseEuler + shakeRot);

        // 4) FOV 복귀(킥 코루틴 외 일반 프레임 복귀)
        if (targetCam != null)
        {
            // 코루틴 진행 중이면 fovCurrent가 이미 갱신됨
            // 코루틴이 없으면 천천히 baseFOV로 복귀
            fovCurrent = Mathf.Lerp(fovCurrent, baseFOV, 1f - Mathf.Exp(-fovReturnSpeed * Time.unscaledDeltaTime));
            targetCam.fieldOfView = fovCurrent;
        }
    }

    // 상황에 따라 기본 FOV를 바꾸고 싶을 때(달리기 토글 등)
    public void SetBaseFOV(float newBaseFOV)
    {
        baseFOV = Mathf.Clamp(newBaseFOV, 1f, 179f);
    }
}
