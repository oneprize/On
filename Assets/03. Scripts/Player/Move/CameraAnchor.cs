using UnityEngine;

public class CameraRigAnchor : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0f, 1.6f, 0f);

    void LateUpdate()
    {
        if (!player) return;
        transform.position = player.position + offset; // 위치만 따라감
    }
}
