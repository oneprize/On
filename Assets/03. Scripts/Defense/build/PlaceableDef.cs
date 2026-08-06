using UnityEngine;

[CreateAssetMenu(menuName = "Building/PlaceableDef")]
public class PlaceableDef : ScriptableObject
{
    public string id;                  // GUID 또는 고유 문자열
    public GameObject prefab;          // 실제 배치 프리팹
    public Vector3Int gridSize = Vector3Int.one; // 그리드 셀 단위 크기
    public int cost = 0;               // 자원 비용
    public bool alignToSurfaceNormal;  // 표면 노멀 정렬 여부
    public bool requireGroundTag = true;
    public string[] requiredGroundTags = new string[] { "Ground", "Road1","Road2", "Road3" };
    public float maxSlopeDegrees = 30f;
    public LayerMask blockingLayers;   // 충돌 차단 레이어
}
