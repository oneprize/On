using UnityEngine;

public static class GridSnap
{
    public static Vector3 Snap(Vector3 worldPos, float cellSize)
    {
        var x = Mathf.Round(worldPos.x / cellSize) * cellSize;
        var y = Mathf.Round(worldPos.y / cellSize) * cellSize;
        var z = Mathf.Round(worldPos.z / cellSize) * cellSize;
        return new Vector3(x, y, z);
    }
}
