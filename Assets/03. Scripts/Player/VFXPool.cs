using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.VFX;
using System.Collections;

public class VFXPool : MonoBehaviour
{
    [Header("Ǯ���� ������(������ ����Ʈ)")]
    public GameObject prefab;

    [Header("�ʱ�/�ִ� ����")]
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
        // �ڵ� ��ȯ ��ũ��Ʈ ����
        var auto = go.AddComponent<VFXAutoReturn>();
        auto.Init(this);
        go.SetActive(false);
        return go;
    }

    void OnGet(GameObject go)
    {
        go.SetActive(true);

        // ParticleSystem �ʱ�ȭ/���
        foreach (var ps in go.GetComponentsInChildren<ParticleSystem>(true))
        {
            ps.Clear(true);
            ps.Play(true);
        }
        // VFX Graph ���
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
        // �� ��� �ð� ��� �� �ڵ� ��ȯ
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

        // ParticleSystem ����: ���� duration + startLifetime �ִ밪
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

        // VFX Graph�� ���� ������ �뷫�� �ð� ���(������ �⺻ 1��)
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
