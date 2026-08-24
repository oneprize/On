using UnityEngine;
using UnityEngine.InputSystem;

public class BuildSystem : MonoBehaviour
{
    [Header("Refs")]
    public Camera cam;
    public Transform ghostRoot;
    public LayerMask raycastMask = ~0;
    public float gridSize = 1f;
    public ResourceWallet wallet;

    [Header("Debug")]
    [SerializeField] private bool debugRay = true;
    [SerializeField] private float debugLogInterval = 0.5f; // �α� ���� ����
    private float _nextLogTime;

    [Header("State")]
    public PlaceableDef currentDef;
    public BuildGhost ghostPrefab; // currentDef�� ���� ���� �������� ���� �⺻(fallback) ����

    private BuildGhost ghost;
    private PlaceableDef ghostDef;
    private PlacementValidator validator = new PlacementValidator();
    private Quaternion rot = Quaternion.identity;
    private bool useGridSnap = true;

    void Start()
    {
        if (currentDef != null) EnsureGhostFor(currentDef);
        if (ghost != null) ghost.gameObject.SetActive(false);
    }

    private void EnsureGhostFor(PlaceableDef def)
    {
        if (ghost != null && ghostDef == def) return;

        var prefabToUse = (def != null && def.ghostPrefab != null) ? def.ghostPrefab : ghostPrefab;
        if (prefabToUse == null) return;

        var wasActive = ghost != null && ghost.gameObject.activeSelf;
        if (ghost != null) Destroy(ghost.gameObject);

        ghost = Instantiate(prefabToUse, ghostRoot);
        ghost.gameObject.SetActive(wasActive);
        ghostDef = def;
    }

    void Update()
    {
        if (currentDef == null) { if (ghost != null) ghost.gameObject.SetActive(false); return; }

        EnsureGhostFor(currentDef);
        if (ghost == null) return;

        if (RaycastGround(out var hit))
        {
            var targetPos = hit.point;
            if (useGridSnap)
            {
                var snapped = GridSnap.Snap(targetPos, gridSize);
                targetPos = new Vector3(snapped.x, hit.point.y, snapped.z);
            }

            var targetRot = currentDef.alignToSurfaceNormal
                ? Quaternion.FromToRotation(Vector3.up, hit.normal) * rot
                : rot;

            ghost.gameObject.SetActive(true);
            ghost.SetPose(targetPos, targetRot);          

            var isValid = validator.Check(currentDef, targetPos, targetRot, out _, hit.collider, hit.normal);
            ghost.SetValid(isValid);

            if (isValid && Mouse.current.leftButton.wasPressedThisFrame)
                TryPlace(targetPos, targetRot, hit.collider, hit.normal);

            // ����� �α�(������ ��� ����)
            if (debugRay && Time.time >= _nextLogTime)
            {
                LogHit("[GroundRay]", hit);
                _nextLogTime = Time.time + debugLogInterval;
            }
        }

        if (Keyboard.current.rKey.wasPressedThisFrame) Rotate(+45f);
        if (Keyboard.current.qKey.wasPressedThisFrame) Rotate(-45f);
        if (Keyboard.current.tabKey.wasPressedThisFrame) useGridSnap = !useGridSnap;
        if (Mouse.current.rightButton.wasPressedThisFrame) Cancel();
    }



    private void Rotate(float degrees)
    {
        rot = Quaternion.Euler(0f, degrees, 0f) * rot;
    }

    private void Cancel()
    {
        currentDef = null;
        if (ghost != null) ghost.gameObject.SetActive(false);
    }

    private bool RaycastGround(out RaycastHit hit)
    {
        var ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        return Physics.Raycast(ray, out hit, 1000f, raycastMask, QueryTriggerInteraction.Ignore);
    }

    private static void LogHit(string tag, in RaycastHit hit)
    {
        var go = hit.collider ? hit.collider.gameObject : null;
        var name = go ? go.name : "<none>";
        var t = go ? go.tag : "-";
        var layer = go ? LayerMask.LayerToName(go.layer) : "-";
        Debug.Log($"{tag} hit={name} tag={t} layer={layer} dist={hit.distance:F2} point={hit.point} normal={hit.normal}");
    }

    private void TryPlace(Vector3 pos, Quaternion rotQ, Collider surfaceCollider, Vector3 surfaceNormal)
    {
        if (wallet && wallet.Balance < currentDef.cost) return;

        var ok = validator.Check(currentDef, pos, rotQ, out var reason, surfaceCollider, surfaceNormal);
        if (!ok) return;

        if (wallet) wallet.Spend(currentDef.cost);

        Instantiate(currentDef.prefab, pos, rotQ);
        // TODO: ���� ť�� ���, �׺�޽� ������Ʈ ��
    }

    public void Select(PlaceableDef def)
    {
        currentDef = def;
        EnsureGhostFor(def);
        if (ghost != null) ghost.gameObject.SetActive(true);
        rot = Quaternion.identity;
    }
}
