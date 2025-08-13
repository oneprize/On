using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;          // 따라갈 대상 (플레이어)
    public Vector3 offset = new Vector3(0, 3, -5); // 기본 거리 및 높이
    public float rotationSpeed = 5f;  // 마우스 회전 속도
    public float minY = -30f;         // 위로 보는 각도 제한
    public float maxY = 60f;          // 아래로 보는 각도 제한

    private float rotX; // 마우스 상하 회전
    private float rotY; // 마우스 좌우 회전

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
        // 마우스 입력
        rotY += Input.GetAxis("Mouse X") * rotationSpeed;
        rotX -= Input.GetAxis("Mouse Y") * rotationSpeed;
        rotX = Mathf.Clamp(rotX, minY, maxY);

        // 카메라 회전 적용
        Quaternion rotation = Quaternion.Euler(rotX, rotY, 0);
        Vector3 position = target.position + rotation * offset;

        transform.position = position;
        transform.rotation = rotation;
    }
}
