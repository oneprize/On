using UnityEngine;
using Unity.Cinemachine;

public class PlayerCameraRecenteringUtility : MonoBehaviour
{
    [SerializeField] private CinemachineCamera virtualCamera;

    private CinemachineComponentBase component;

    private CinemachineRotationComposer composer;

    public void Initialize()
    {
        // component = virtualCamera.GetComponentOwner().GetComponent(CinemachineCore.Stage.Aim);

        composer = component as CinemachineRotationComposer;

        if (composer == null)
        {
            Debug.LogError("CinemachineRotationComposer를 찾을 수 없습니다.");
        }
    }

    public void EnableRecentering(float waitTime = -1f, float recenteringTime = -1f, float baseMovementSpeed = 1f, float movementSpeed = 1f)
    {
        if (composer == null) return;

        //composer.RecenterToTargetHeading.m_enabled = true;
        //composer.RecenterToTargetHeading.m_WaitTime = waitTime;
        //composer.RecenterToTargetHeading.m_RecenteringTime = recenterTime;
    }

    public void DisableRecentering()
    {
        if (composer == null) return;

        // composer.RecenterToTargetHeading.m_enabled = false;
    }
}
