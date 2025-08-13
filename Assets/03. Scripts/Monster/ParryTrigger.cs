using UnityEngine;

public class ParryTrigger : MonoBehaviour
{
    public float parryDistance = 3f;                  // 플레이어와의 거리 기준
    public Material normalMaterial;                   // 원래 머티리얼
    public Material highlightMaterial;                // 노란색 머티리얼
    public bool isParryWindow = false;

    private Renderer rend;
    private Transform player;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        if (isParryWindow) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist < parryDistance)
        {
            StartParryWindow();
        }
    }

    void StartParryWindow()
    {
        isParryWindow = true;

        // 머티리얼 변경
        if (highlightMaterial != null && rend != null)
        {
            rend.material = highlightMaterial;
        }

        // 플레이어에게 알림
        PlayerParryControl playerParry = player.GetComponent<PlayerParryControl>();
        if (playerParry != null)
        {
            playerParry.CurrentParryTarget = this.gameObject;
            playerParry.isParryAvailable = true;
        }
    }

    // 돌 제거 함수 (외부에서 호출됨)
    public void Break()
    {
        Destroy(gameObject);
    }
}
