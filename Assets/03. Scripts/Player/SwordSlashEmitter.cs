using UnityEngine;

public class SwordSlashEmitter : MonoBehaviour
{
    public VFXPool pool;          // �� ����� VFXPool ����
    public Transform slashOrigin; // Į�� �Ǵ� ���ϴ� ������
    public Vector3 localPosOffset;
    public Vector3 localEulerOffset;
    public bool alignToSwingDir = true;

    Vector3 _lastTipPos;

    void LateUpdate()
    {
        if (slashOrigin != null) _lastTipPos = slashOrigin.position;
    }

    // �ִϸ��̼� �̺�Ʈ���� ȣ��
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

        pool.Spawn(pos, rot); // Instantiate ��� ����
    }
}
