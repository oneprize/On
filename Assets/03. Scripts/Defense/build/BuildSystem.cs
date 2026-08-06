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
    [SerializeField] private float debugLogInterval = 0.5f; // 로그 도배 방지
    private float _nextLogTime;

    [Header("State")]
    public PlaceableDef currentDef;
    public BuildGhost ghostPrefab;

    private BuildGhost ghost;
    private PlacementValidator validator = new PlacementValidator();
    private Quaternion rot = Quaternion.identity;
    private bool useGridSnap = true;

    void Start()
    {
        if (ghost == null) ghost = Instantiate(ghostPrefab, ghostRoot);
        ghost.gameObject.SetActive(false);
    }

    void Update()
    {
        if (currentDef == null) { ghost.gameObject.SetActive(false); return; }

        if (RaycastGround(out var hit))
        {
            var targetPos = hit.point;
            if (useGridSnap) targetPos = GridSnap.Snap(targetPos, gridSize);

            var targetRot = currentDef.alignToSurfaceNormal
                ? Quaternion.FromToRotation(Vector3.up, hit.normal) * rot
                : rot;

            ghost.gameObject.SetActive(true);
            ghost.SetPose(targetPos, targetRot);          

            var isValid = validator.Check(currentDef, targetPos, targetRot, out _);
            ghost.SetValid(isValid);

            if (isValid && Mouse.current.leftButton.wasPressedThisFrame)
                TryPlace(targetPos, targetRot);

            // 디버그 로그(과도한 출력 방지)
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
        ghost.gameObject.SetActive(false);
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

    private void TryPlace(Vector3 pos, Quaternion rotQ)
    {
        if (wallet && wallet.Balance < currentDef.cost) return;

        var ok = validator.Check(currentDef, pos, rotQ, out var reason);
        if (!ok) return;

        if (wallet) wallet.Spend(currentDef.cost);

        Instantiate(currentDef.prefab, pos, rotQ);
        // TODO: 저장 큐에 기록, 네비메시 업데이트 등
    }

    public void Select(PlaceableDef def)
    {
        currentDef = def;
        if (ghost == null) ghost = Instantiate(ghostPrefab, ghostRoot);
        ghost.gameObject.SetActive(true);
        rot = Quaternion.identity;
    }
}
