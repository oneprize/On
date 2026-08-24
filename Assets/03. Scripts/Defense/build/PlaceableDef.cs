using UnityEngine;

[CreateAssetMenu(menuName = "Building/PlaceableDef")]
public class PlaceableDef : ScriptableObject
{
    public string id;                  // GUID �Ǵ� ���� ���ڿ�
    public GameObject prefab;          // ���� ��ġ ������
    public BuildGhost ghostPrefab;     // ��ġ �̸�����(����) ������ - ��������� BuildSystem�� �⺻ ���� ���
    public Vector3Int gridSize = Vector3Int.one; // �׸��� �� ���� ũ��
    public int cost = 0;               // �ڿ� ���
    public bool alignToSurfaceNormal;  // ǥ�� ��� ���� ����
    public bool requireGroundTag = true;
    public string[] requiredGroundTags = new string[] { "Ground", "Road1","Road2", "Road3" };
    public float maxSlopeDegrees = 30f;
    public LayerMask blockingLayers;   // �浹 ���� ���̾�
}
