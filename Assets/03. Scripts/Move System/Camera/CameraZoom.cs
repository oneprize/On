using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] CinemachineCamera cinemachineCamera;     // CinemachineCamera 참조
    [SerializeField] InputActionReference zoomAction;         // Player/Zoom (float, Scroll/Y)
    [SerializeField] float zoomSpeed = 2f;                    // 스크롤 민감도
    [SerializeField] float minRadius = 1.5f;                  // 최소 거리
    [SerializeField] float maxRadius = 8f;                    // 최대 거리

    // Follow 컴포넌트 참조 (자동으로 찾아서 캐시)
    private CinemachineOrbitalFollow orbitalFollow;
    private CinemachineThirdPersonFollow thirdPersonFollow;
    private CinemachineFollow follow;

    void Start()
    {
        // CinemachineCamera가 할당되지 않은 경우 자동으로 찾기
        if (cinemachineCamera == null)
        {
            cinemachineCamera = GetComponent<CinemachineCamera>();
        }

        if (cinemachineCamera != null)
        {
            // 사용 가능한 Follow 컴포넌트들 찾기 및 캐시
            orbitalFollow = cinemachineCamera.GetComponent<CinemachineOrbitalFollow>();
            thirdPersonFollow = cinemachineCamera.GetComponent<CinemachineThirdPersonFollow>();
            follow = cinemachineCamera.GetComponent<CinemachineFollow>();
        }
    }

    void Update()
    {
        if (zoomAction == null || cinemachineCamera == null) return;

        float scroll = zoomAction.action.ReadValue<float>();
        if (Mathf.Approximately(scroll, 0f)) return;

        // 각 Follow 컴포넌트 타입에 따라 줌 적용
        if (orbitalFollow != null)
        {
            ApplyZoomToOrbitalFollow(scroll);
        }
        else if (thirdPersonFollow != null)
        {
            ApplyZoomToThirdPersonFollow(scroll);
        }
        else if (follow != null)
        {
            ApplyZoomToFollow(scroll);
        }
    }

    void ApplyZoomToOrbitalFollow(float scroll)
    {
        // Radius 속성을 직접 사용 (가장 직관적인 방법)
        float currentRadius = orbitalFollow.Radius;
        float newRadius = Mathf.Clamp(currentRadius - scroll * zoomSpeed, minRadius, maxRadius);
        orbitalFollow.Radius = newRadius;
    }

    void ApplyZoomToThirdPersonFollow(float scroll)
    {
        float currentDistance = thirdPersonFollow.CameraDistance;
        thirdPersonFollow.CameraDistance = Mathf.Clamp(currentDistance - scroll * zoomSpeed, minRadius, maxRadius);
    }

    void ApplyZoomToFollow(float scroll)
    {
        Vector3 offset = follow.FollowOffset;
        float currentRadius = offset.magnitude;
        float newRadius = Mathf.Clamp(currentRadius - scroll * zoomSpeed, minRadius, maxRadius);

        if (currentRadius > 0)
        {
            follow.FollowOffset = offset.normalized * newRadius;
        }
        else
        {
            follow.FollowOffset = new Vector3(0, 0, -newRadius);
        }
    }
}