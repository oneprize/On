using UnityEngine;

public class BuildGhost : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Color okColor = new Color(0f, 1f, 0f, 0.35f);
    [SerializeField] private Color badColor = new Color(1f, 0f, 0f, 0.35f);

    public void SetPose(Vector3 pos, Quaternion rot)
    {
        transform.SetPositionAndRotation(pos, rot);
        transform.localScale = Vector3.one;
        var newPos = new Vector3(pos.x, pos.y + 0.95f, pos.z);
    }

    public void SetValid(bool isValid)
    {
        var c = isValid ? okColor : badColor;
        foreach (var r in renderers)
        {
            foreach (var m in r.materials) m.color = c;
        }
    }
}
