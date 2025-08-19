using UnityEngine;

public class BahamutAttack : MonoBehaviour
{
    public CameraFX camFX; // 인스펙터에서 Main Camera 드래그

    void Start()
    {
        if (camFX == null)
            camFX = Camera.main.GetComponent<CameraFX>();
    }

    // 공격 애니메이션 타이밍에서 호출 (예: Animation Event)
    public void OnAttackHit()
    {
        if (camFX != null)
        {
            // FOV를 +8만큼 확대 -> 0.15초간 확대 -> 0.05초 유지 -> 0.25초 동안 원래대로
            StartCoroutine(camFX.KickFOV(8f, 7f, 0.1f, 0.25f));
        }
    }
}
