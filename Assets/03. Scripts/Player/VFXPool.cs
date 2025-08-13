using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.VFX;
using System.Collections;

public class VFXPool : MonoBehaviour
{
    [Header("풀링할 프리팹(슬래시 이펙트)")]
    public GameObject prefab;

    [Header("초기/최대 개수")]
    public int defaultCapacity = 8;
    public int maxSize = 32;

    ObjectPool<GameObject> _pool;

    void Awake()
    {
        _pool = new ObjectPool<GameObject>(
            CreateFunc, OnGet, OnRelease, DestroyPooledObject, true, defaultCapacity, maxSize
        );
    }

    GameObject CreateFunc()
    {
        var go = Instantiate(prefab);
        // 자동 반환 스크립트 부착
        var auto = go.AddComponent<VFXAutoReturn>();
        auto.Init(this);
        go.SetActive(false);
        return go;
    }

    void OnGet(GameObject go)
    {
        go.SetActive(true);

        // ParticleSystem 초기화/재생
        foreach (var ps in go.GetComponentsInChildren<ParticleSystem>(true))
        {
            ps.Clear(true);
            ps.Play(true);
        }
        // VFX Graph 재생
        foreach (var vfx in go.GetComponentsInChildren<VisualEffect>(true))
        {
            vfx.Reinit();
            vfx.Play();
        }
    }

    void OnRelease(GameObject go) => go.SetActive(false);

    void DestroyPooledObject(GameObject go) => Destroy(go);

    public GameObject Spawn(Vector3 pos, Quaternion rot, Transform parent = null)
    {
        var go = _pool.Get();
        go.transform.SetPositionAndRotation(pos, rot);
        if (parent != null) go.transform.SetParent(parent, worldPositionStays: true);
        return go;
    }

    public void Despawn(GameObject go) => _pool.Release(go);
}

public class VFXAutoReturn : MonoBehaviour
{
    VFXPool _pool;
    Coroutine _co;

    public void Init(VFXPool pool) => _pool = pool;

    void OnEnable()
    {
        // 총 재생 시간 계산 후 자동 반환
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(ReturnAfter(CalcDuration()));
    }

    IEnumerator ReturnAfter(float secs)
    {
        yield return new WaitForSeconds(secs);
        _pool.Despawn(gameObject);
    }

    float CalcDuration()
    {
        float maxT = 0f;

        // ParticleSystem 기준: 메인 duration + startLifetime 최대값
        foreach (var ps in GetComponentsInChildren<ParticleSystem>(true))
        {
            var m = ps.main;
            float life = 0f;
            if (m.startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
                life = m.startLifetime.constantMax;
            else if (m.startLifetime.mode == ParticleSystemCurveMode.Constant)
                life = m.startLifetime.constant;
            else life = 1f;

            maxT = Mathf.Max(maxT, m.duration + life);
        }

        // VFX Graph는 노출 변수나 대략적 시간 사용(없으면 기본 1초)
        if (maxT <= 0f)
        {
            var vfx = GetComponentInChildren<VisualEffect>(true);
            if (vfx != null && vfx.HasFloat("Lifetime"))
                maxT = vfx.GetFloat("Lifetime");
            if (maxT <= 0f) maxT = 1f;
        }
        return maxT;
    }
}
