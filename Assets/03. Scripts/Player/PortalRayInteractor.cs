using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalRayInteractor : MonoBehaviour
{
    [Header("레이 발사 기준")]
    public Camera cam;           // 보통 Main Camera
    public Transform origin;     // 레이 시작(없으면 cam.transform)

    [Header("탐지 설정")]
    public float maxDistance = 4f;
    public LayerMask portalMask; // Portal 레이어만 체크 권장
    public bool useSphereCast = true;
    public float sphereRadius = 0.25f;
    [Range(-1f, 1f)] public float fovDot = 0.6f; // 시야 제한(낮을수록 넓음)

    [Header("입력/UI")]
    public KeyCode interactKey = KeyCode.E;
    public GameObject overlayCanvas; // 화면 하단 등의 Overlay Canvas(선택) ? 없으면 비워둠
    public string overlayText = "E - open"; // Overlay에 텍스트 넣을 거면 직접 갱신해도 됨

    [Header("로딩 연출(선택)")]
    public bool asyncLoad = true;

    // 내부
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

        // 1) 포탈 감지
        var hitPortal = DetectPortal();

        // 2) UI 표시/해제
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

        // 3) 상호작용
        if (_current && Input.GetKeyDown(interactKey))
        {
            if (string.IsNullOrEmpty(_current.targetSceneName))
            {
                Debug.LogWarning("[PortalRayInteractor] targetSceneName이 비어있습니다.");
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

                // 시야각 제한
                Vector3 to = (pt.transform.position - o).normalized;
                if (Vector3.Dot(d, to) < fovDot) continue;

                if (h.distance < best)
                {
                    best = h.distance;
                    bestPt = pt;
                }

                // 디버그: 어떤 레이어 맞았는지 확인
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

                // 시야각 제한
                Vector3 to = (pt.transform.position - o).normalized;
                if (Vector3.Dot(d, to) < fovDot) return null;

                // 디버그:
                // Debug.Log($"[PortalRayInteractor] Ray hit {hit.collider.name} on {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
                return pt;
            }
            return null;
        }
    }

    IEnumerator LoadSceneCo(string sceneName)
    {
        _loading = true;

        // UI 끄기
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
