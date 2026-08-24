using System.Diagnostics;
using UnityEngine;

public class PlacementValidator
{
    public bool Check(PlaceableDef def, Vector3 pos, Quaternion rot, out string reason, Collider surfaceCollider = null, Vector3? surfaceNormal = null, Bounds? worldBoundsOverride = null, Collider supportToIgnore = null,
    bool debug = false)
    {
        reason = "";

        // 1) 설치 표면 태그/경사 검사
        // BuildSystem이 커서 레이로 실제로 맞춘 표면(바닥/벽/천장)을 그대로 전달받아 검사한다.
        // (아래로 다시 레이캐스트하면 벽/천장 설치 시 엉뚱하게 바닥이 걸려서 항상 실패하게 됨)
        if (surfaceCollider != null)
        {
            if (def.requireGroundTag)
            {
                bool hasValidTag = false;

                // 지정 태그 중 하나라도 맞으면 true
                foreach (var tag in def.requiredGroundTags)
                {
                    if (surfaceCollider.CompareTag(tag))
                    {
                        hasValidTag = true;
                        break;
                    }
                }

                if (!hasValidTag)
                {
                    reason = "잘못된 설치 표면 태그";
                    return false;
                }
            }

            if (def.maxSlopeDegrees < 89f && surfaceNormal.HasValue)
            {
                var slope = Vector3.Angle(surfaceNormal.Value, Vector3.up);
                if (slope > def.maxSlopeDegrees)
                {
                    reason = "경사도가 너무 큼";
                    return false;
                }
            }
        }

        // 2) �浹 üũ(AABB �ٻ�)
        var bounds = GetWorldBounds(def, pos, rot);
        var hits = Physics.OverlapBox(bounds.center, bounds.extents, rot, def.blockingLayers, QueryTriggerInteraction.Collide);
        if (hits.Length > 0)
        {
            reason = "�浹 �߻�";
            return false;
        }

        return true;
    }

    private Bounds GetWorldBounds(PlaceableDef def, Vector3 pos, Quaternion rot)
    {
        // gridSize�� ���� ������ �ؼ�(��=1m ����)
        var size = Vector3.Scale((Vector3)def.gridSize, Vector3.one);
        var bounds = new Bounds(pos, size);
        return bounds;
    }
}
