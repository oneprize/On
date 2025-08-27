using UnityEngine;

public class CameraRig : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float sensitivityX = 250f;
    public float sensitivityY = 200f;
    public float minPitch = -45f;
    public float maxPitch = 70f;

    [Header("Optional")]
    public bool lockCursor = true;

    float yaw;
    float pitch;

    void Start()
    {
        Vector3 e = transform.localEulerAngles;
        yaw = e.y; pitch = e.x;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void LateUpdate()
    {
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        yaw += mx * sensitivityX * Time.deltaTime;
        pitch -= my * sensitivityY * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
