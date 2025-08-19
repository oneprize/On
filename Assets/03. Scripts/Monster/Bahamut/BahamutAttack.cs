using UnityEngine;

public class BahamutAttack : MonoBehaviour
{
    public CameraFX camFX; // �ν����Ϳ��� Main Camera �巡��

    void Start()
    {
        if (camFX == null)
            camFX = Camera.main.GetComponent<CameraFX>();
    }

    // ���� �ִϸ��̼� Ÿ�ֿ̹��� ȣ�� (��: Animation Event)
    public void OnAttackHit()
    {
        if (camFX != null)
        {
            // FOV�� +8��ŭ Ȯ�� -> 0.15�ʰ� Ȯ�� -> 0.05�� ���� -> 0.25�� ���� �������
            StartCoroutine(camFX.KickFOV(8f, 7f, 0.1f, 0.25f));
        }
    }
}
