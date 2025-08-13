using UnityEngine;

public class ParryEffectHandler : MonoBehaviour
{
    [Header("머터리얼 설정")]
    public Material normalMaterial;          // 원래 머터리얼
    public Material parryMaterial;           // 패링 타이밍 중일 때 머터리얼

    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();

        // 시작 시 정상 머터리얼로 설정
        if (rend != null && normalMaterial != null)
        {
            rend.material = normalMaterial;
        }
    }

    public void ActivateParryEffect()
    {
        if (rend != null && parryMaterial != null)
        {
            rend.material = parryMaterial;
        }

        Debug.Log("패링 타이밍 진입!");
    }

    public void ResetMaterial()
    {
        if (rend != null && normalMaterial != null)
        {
            rend.material = normalMaterial;
        }
    }
}
