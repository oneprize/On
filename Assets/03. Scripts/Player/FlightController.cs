using UnityEngine;

public class FlightController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float verticalSpeed = 5f;
    public float mouseSensitivity = 2f;
    public float doubleJumpThreshold = 0.3f;

    private bool isFlying = false;
    private float lastJumpTime = -1f;

    private CharacterController controller;
    private Transform cam;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMouseLook();
        HandleDoubleJumpToggle();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleDoubleJumpToggle()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (Time.time - lastJumpTime < doubleJumpThreshold)
            {
                isFlying = !isFlying;
                Debug.Log(isFlying ? "���� ��� ON" : "���� ��� OFF");
            }
            lastJumpTime = Time.time;
        }
    }

    void HandleMovement()
    {
        if (isFlying)
        {
            // �⺻ ���� �� ����
            Vector3 flightMove = transform.forward * 3f + Vector3.down * 0.3f;

            // �÷��̾� ���� �Է� �߰�
            Vector3 inputMove = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");
            flightMove += inputMove * moveSpeed;

            // �� ���/�ϰ�
            if (Input.GetButton("Jump"))
                flightMove += Vector3.up * verticalSpeed;
            if (Input.GetKey(KeyCode.LeftShift))
                flightMove += Vector3.down * verticalSpeed;

            controller.Move(flightMove * Time.deltaTime);
        }
        else
        {
            // �Ϲ� �߷� ����
            Vector3 groundedMove = Vector3.zero;
            groundedMove += transform.right * Input.GetAxis("Horizontal");
            groundedMove += transform.forward * Input.GetAxis("Vertical");
            groundedMove.y += Physics.gravity.y;

            controller.Move(groundedMove * Time.deltaTime);
        }
    }
}
