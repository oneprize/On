using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalRayInteractor : MonoBehaviour
{
    [Header("���� �߻� ����")]
    public Camera cam;           // ���� Main Camera
    public Transform origin;     // ���� ����(������ cam.transform)

    [Header("Ž�� ����")]
    public float maxDistance = 4f;
    public LayerMask portalMask; // Portal ���̾ üũ ����
    public bool useSphereCast = true;
    public float sphereRadius = 0.25f;
    [Range(-1f, 1f)] public float fovDot = 0.6f; // �þ� ����(�������� ����)

    [Header("�Է�/UI")]
    public KeyCode interactKey = KeyCode.E;
    public GameObject overlayCanvas; // ȭ�� �ϴ� ���� Overlay Canvas(����) ? ������ �����
    public string overlayText = "E - open"; // Overlay�� �ؽ�Ʈ ���� �Ÿ� ���� �����ص� ��

    [Header("�ε� ����(����)")]
    public bool asyncLoad = true;

    // ����
    PortalTarget _current;
    bool _loading;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!origin) origin = cam ? cam.transform : transform;
        if (overlayCanvas) overlayCanvas.SetActive(false);
    }

    void Update()
    {
        if (_loading) return;

        // 1) ��Ż ����
        var hitPortal = DetectPortal();

        // 2) UI ǥ��/����
        if (hitPortal != _current)
        {
            if (_current)
            {
                _current.SetPromptVisible(false);
                if (overlayCanvas) overlayCanvas.SetActive(false);
            }

            _current = hitPortal;

            if (_current)
            {
                _current.SetPromptVisible(true);
                if (overlayCanvas) overlayCanvas.SetActive(true);
            }
        }

        // 3) ��ȣ�ۿ�
        if (_current && Input.GetKeyDown(interactKey))
        {
            if (string.IsNullOrEmpty(_current.targetSceneName))
            {
                Debug.LogWarning("[PortalRayInteractor] targetSceneName�� ����ֽ��ϴ�.");
                return;
            }
            StartCoroutine(LoadSceneCo(_current.targetSceneName));
        }
    }

    PortalTarget DetectPortal()
    {
        Vector3 o = origin.position;
        Vector3 d = origin.forward;

        if (useSphereCast)
        {
            var hits = Physics.SphereCastAll(o, sphereRadius, d, maxDistance, portalMask, QueryTriggerInteraction.Ignore);
            float best = float.MaxValue;
            PortalTarget bestPt = null;

            foreach (var h in hits)
            {
                var pt = h.collider.GetComponentInParent<PortalTarget>();
                if (!pt) continue;

                // �þ߰� ����
                Vector3 to = (pt.transform.position - o).normalized;
                if (Vector3.Dot(d, to) < fovDot) continue;

                if (h.distance < best)
                {
                    best = h.distance;
                    bestPt = pt;
                }

                // �����: � ���̾� �¾Ҵ��� Ȯ��
                // Debug.Log($"[PortalRayInteractor] Sphere hit {h.collider.name} on {LayerMask.LayerToName(h.collider.gameObject.layer)}");
            }
            return bestPt;
        }
        else
        {
            if (Physics.Raycast(o, d, out RaycastHit hit, maxDistance, portalMask, QueryTriggerInteraction.Ignore))
            {
                var pt = hit.collider.GetComponentInParent<PortalTarget>();
                if (!pt) return null;

                // �þ߰� ����
                Vector3 to = (pt.transform.position - o).normalized;
                if (Vector3.Dot(d, to) < fovDot) return null;

                // �����:
                // Debug.Log($"[PortalRayInteractor] Ray hit {hit.collider.name} on {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
                return pt;
            }
            return null;
        }
    }

    IEnumerator LoadSceneCo(string sceneName)
    {
        _loading = true;

        // UI ����
        if (_current) _current.SetPromptVisible(false);
        if (overlayCanvas) overlayCanvas.SetActive(false);

        if (asyncLoad)
        {
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (!op.isDone) yield return null;
        }
        else
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}
