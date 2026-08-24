using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ObstacleSelectButton : MonoBehaviour
{
    [SerializeField] private BuildSystem buildSystem;
    [SerializeField] private PlaceableDef def;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (buildSystem != null) buildSystem.Select(def);
    }
}
