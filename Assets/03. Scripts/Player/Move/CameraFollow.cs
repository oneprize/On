using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;          // ���� ��� (�÷��̾�)
    public Vector3 offset = new Vector3(0, 3, -5); // �⺻ �Ÿ� �� ����
    public float rotationSpeed = 5f;  // ���콺 ȸ�� �ӵ�
    public float minY = -30f;         // ���� ���� ���� ����
    public float maxY = 60f;          // �Ʒ��� ���� ���� ����

    private float rotX; // ���콺 ���� ȸ��
    private float rotY; // ���콺 �¿� ȸ��

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        rotX = angles.x;
        rotY = angles.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        // ���콺 �Է�
        rotY += Input.GetAxis("Mouse X") * rotationSpeed;
        rotX -= Input.GetAxis("Mouse Y") * rotationSpeed;
        rotX = Mathf.Clamp(rotX, minY, maxY);

        // ī�޶� ȸ�� ����
        Quaternion rotation = Quaternion.Euler(rotX, rotY, 0);
        Vector3 position = target.position + rotation * offset;

        transform.position = position;
        transform.rotation = rotation;
    }
}
