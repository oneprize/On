using UnityEngine;

public class MonsterThrower : MonoBehaviour
{
    public Transform handTransform; // 손 위치
    public Rigidbody targetObject;  // 던질 물체
    public Transform throwDirection; // 던질 방향 기준

    private bool hasObject = false;

    public Transform player; // 플레이어 위치
    public float parryDistance = 3f;
    public float parryWindowTime = 0.7f;
    private float parryStartTime;
    private bool isParryWindow = false;
    private bool parryTriggered = false;

    public Material normalMaterial;
    public Material highlightMaterial;

    private Renderer targetRenderer;

    public void PickupObject()
    {
        if (targetObject == null) return;

        // 손에 붙이기
        targetObject.transform.SetParent(handTransform);
        targetObject.transform.localPosition = Vector3.zero;
        targetObject.transform.localRotation = Quaternion.identity;

        // 물리 멈춤
        targetObject.isKinematic = true;
        hasObject = true;
    }

    public void ThrowObject()
    {
        if (!hasObject) return;

        // 손에서 분리
        targetObject.transform.SetParent(null);
        targetObject.isKinematic = false;

        // linearVelocity 사용해 던짐
        Vector3 throwDir = throwDirection.forward;
        targetObject.linearVelocity = throwDir * 10f;

        // 렌더러 저장
        targetRenderer = targetObject.GetComponent<Renderer>();
        parryTriggered = false;
        isParryWindow = false;

        hasObject = false;
    }

    void Update()
    {
        if (targetObject != null && !targetObject.isKinematic && !parryTriggered)
        {
            float distanceToPlayer = Vector3.Distance(targetObject.position, player.position);

            if (!isParryWindow && distanceToPlayer < parryDistance)
            {
                StartParryWindow();
            }

            if (isParryWindow)
            {
                float elapsed = Time.unscaledTime - parryStartTime;
                if (elapsed >= parryWindowTime)
                {
                    EndParryWindow(false);
                }
            }
        }
    }

    void StartParryWindow()
    {
        isParryWindow = true;
        parryStartTime = Time.unscaledTime;

        // 시간 느려짐
        Time.timeScale = 0.2f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // 노란빛 하이라이트 머티리얼 적용
        if (targetRenderer != null && highlightMaterial != null)
        {
            targetRenderer.material = highlightMaterial;
        }

        Debug.Log("패링 타이밍!");
    }

    void EndParryWindow(bool success)
    {
        isParryWindow = false;
        parryTriggered = true;

        // 시간 원래대로
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // 머티리얼 원상복구
        if (targetRenderer != null && normalMaterial != null)
        {
            targetRenderer.material = normalMaterial;
        }

        if (!success)
        {
            Debug.Log("패링 실패!");
        }
    }

    void FindNearestThrowable()
    {
        float radius = 3f;
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Throwable"))
            {
                targetObject = hit.GetComponent<Rigidbody>();
                break;
            }
        }
    }
}
