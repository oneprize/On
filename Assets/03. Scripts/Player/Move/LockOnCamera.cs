using UnityEngine;
using Unity.Cinemachine; // CM3 네임스페이스

public class LockOnCameraController : MonoBehaviour
{
    [Header("References")]
    public Transform player;                 // 플레이어 Transform
    public Transform cameraRig;              // Player/CameraRig
    public CinemachineCamera followCam;  // VCam_Follow
    public CinemachineCamera lockOnCam;  // VCam_LockOn
    public Camera mainCamera;                // 메인카메라 (Brain 달린 실제 카메라)

    [Header("Target Search")]
    public LayerMask enemyLayer;             // Enemy 레이어
    public float searchRadius = 20f;         // 탐색 반경
    public float maxLockAngle = 70f;         // 카메라 정면 기준 허용 각도
    public float loseLockDistance = 30f;     // 너무 멀어지면 해제
    public float loseLockAngle = 85f;        // 시야에서 크게 벗어나면 해제

    [Header("Key")]
    public KeyCode toggleKey = KeyCode.Q;

    Transform currentTarget;
    bool isLocked;

    void Reset()
    {
        if (!mainCamera) mainCamera = Camera.main;
    }

    void Start()
    {
        if (lockOnCam)
        {
            var aim = lockOnCam.GetComponent<CinemachineHardLookAt>();
            if (aim == null)
                aim = lockOnCam.gameObject.AddComponent<CinemachineHardLookAt>(); // OK in CM3
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (isLocked) Unlock();
            else TryLock();
        }

        if (isLocked)
        {
            // 타겟 유효성 체크: 멀어지거나 시야 크게 벗어나면 자동 해제
            if (!currentTarget)
            {
                Unlock();
                return;
            }

            float dist = Vector3.Distance(player.position, currentTarget.position);
            if (dist > loseLockDistance)
            {
                Unlock();
                return;
            }

            Vector3 dir = (currentTarget.position - mainCamera.transform.position).normalized;
            float angle = Vector3.Angle(mainCamera.transform.forward, dir);
            if (angle > loseLockAngle)
            {
                Unlock();
                return;
            }

            // 계속 타겟을 바라보도록 유지 (혹시 LookAt이 지워졌다면 복구)
            if (lockOnCam.LookAt != currentTarget) lockOnCam.LookAt = currentTarget;
        }
    }

    void TryLock()
    {
        Transform best = FindBestTarget();
        if (!best) return;

        // 우선순위로 전환
        if (followCam) followCam.Priority = 10;
        if (lockOnCam)
        {
            lockOnCam.Priority = 20;
            lockOnCam.LookAt = best;   // CM3에서도 LookAt/Follow 속성 사용 가능
        }
    }

    void Unlock()
    {
        if (lockOnCam)
        {
            lockOnCam.Priority = 0;
            lockOnCam.LookAt = null;
        }
        if (followCam) followCam.Priority = 10;
    }

    Transform FindBestTarget()
    {
        Collider[] hits = Physics.OverlapSphere(player.position, searchRadius, enemyLayer);
        if (hits.Length == 0) return null;

        Transform best = null;
        float bestScore = float.MaxValue;

        Vector2 screenCenter = new Vector2(0.5f, 0.5f);

        foreach (var h in hits)
        {
            Transform t = h.transform;

            // 화면 중앙 기준 각도 제한
            Vector3 to = (t.position - mainCamera.transform.position).normalized;
            float ang = Vector3.Angle(mainCamera.transform.forward, to);
            if (ang > maxLockAngle) continue;

            // 화면 중심에 가까울수록 가산점 (뷰포트 거리)
            Vector3 vp = mainCamera.WorldToViewportPoint(t.position);
            if (vp.z < 0f) continue; // 카메라 뒤

            float centerDist = Vector2.Distance(new Vector2(vp.x, vp.y), screenCenter);

            // 거리도 고려해 가중치 적용
            float worldDist = Vector3.Distance(player.position, t.position);
            float score = centerDist * 2f + worldDist * 0.02f; // 가중치 비율은 취향대로

            if (score < bestScore)
            {
                bestScore = score;
                best = t;
            }
        }

        return best;
    }

    void OnDrawGizmosSelected()
    {
        if (!player) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, searchRadius);
    }
}
