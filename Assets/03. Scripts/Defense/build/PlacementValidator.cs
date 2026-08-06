using System.Diagnostics;
using UnityEngine;

public class PlacementValidator
{
    public bool Check(PlaceableDef def, Vector3 pos, Quaternion rot, out string reason, Bounds? worldBoundsOverride = null, Collider supportToIgnore = null,
    bool debug = false)
    {
        reason = "";

        // 1) 경사 제한
        if (def.maxSlopeDegrees < 89f)
        {
            if (Physics.Raycast(new Ray(pos + Vector3.up * 0.5f, Vector3.down), out var hit, 5f))
            {
                if (def.requireGroundTag)
                {
                    bool hasValidTag = false;

                    // 여러 태그 중 하나라도 맞으면 true
                    foreach (var tag in def.requiredGroundTags)
                    {
                        if (hit.collider.CompareTag(tag))
                        {
                            hasValidTag = true;
                            break;
                        }
                    }

                    if (!hasValidTag)
                    {
                        reason = "잘못된 지면 태그";
                        return false;
                    }
                }
                var slope = Vector3.Angle(hit.normal, Vector3.up);
                if (slope > def.maxSlopeDegrees)
                {
                    reason = "경사도가 너무 큼";
                    return false;
                }
            }
        }

        // 2) 충돌 체크(AABB 근사)
        var bounds = GetWorldBounds(def, pos, rot);
        var hits = Physics.OverlapBox(bounds.center, bounds.extents, rot, def.blockingLayers, QueryTriggerInteraction.Ignore);
        if (hits.Length > 0)
        {
            reason = "충돌 발생";
            return false;
        }

        return true;
    }

    private Bounds GetWorldBounds(PlaceableDef def, Vector3 pos, Quaternion rot)
    {
        // gridSize를 미터 단위로 해석(셀=1m 가정)
        var size = Vector3.Scale((Vector3)def.gridSize, Vector3.one);
        var bounds = new Bounds(pos, size);
        return bounds;
    }
}
