using UnityEngine;

public class SwordSlashEmitter : MonoBehaviour
{
    public VFXPool pool;          // 씬 어딘가의 VFXPool 참조
    public Transform slashOrigin; // 칼끝 또는 원하는 기준점
    public Vector3 localPosOffset;
    public Vector3 localEulerOffset;
    public bool alignToSwingDir = true;

    Vector3 _lastTipPos;

    void LateUpdate()
    {
        if (slashOrigin != null) _lastTipPos = slashOrigin.position;
    }

    // 애니메이션 이벤트에서 호출
    public void EmitSlash()
    {
        if (pool == null || slashOrigin == null) return;

        Vector3 pos = slashOrigin.TransformPoint(localPosOffset);
        Quaternion rot;
        if (alignToSwingDir)
        {
            Vector3 dir = (slashOrigin.position - _lastTipPos);
            if (dir.sqrMagnitude < 0.0001f) dir = slashOrigin.forward;
            rot = Quaternion.LookRotation(dir.normalized, Vector3.up) * Quaternion.Euler(localEulerOffset);
        }
        else
        {
            rot = slashOrigin.rotation * Quaternion.Euler(localEulerOffset);
        }

        pool.Spawn(pos, rot); // Instantiate 대신 재사용
    }
}
