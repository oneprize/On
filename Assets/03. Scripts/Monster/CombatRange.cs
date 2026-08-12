using UnityEngine;

// 공격자(attacker) 기준으로 target이 정면 부채꼴(거리 + 각도) 범위 안에 있는지 판정하는 공용 유틸리티.
// 패링 성공 판정과 피격 판정이 서로 다른 기준을 쓰지 않도록 두 곳에서 공유해서 사용한다.
public static class CombatRange
{
    public static bool IsInFrontArc(Transform attacker, Transform target, float range, float halfAngleDegrees)
    {
        if (attacker == null || target == null) return false;

        Vector3 toTarget = target.position - attacker.position;
        toTarget.y = 0f;

        if (toTarget.magnitude > range) return false;
        if (toTarget.sqrMagnitude < 0.0001f) return true;

        Vector3 forward = attacker.forward;
        forward.y = 0f;

        float angle = Vector3.Angle(forward, toTarget);
        return angle <= halfAngleDegrees;
    }
}
