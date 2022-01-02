using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public static class MathUtils
{
    public static quaternion ClampRotation(quaternion q, float3 clamp)
    {
        q.value.x /= q.value.w;
        q.value.y /= q.value.w;
        q.value.z /= q.value.w;
        q.value.w = 1.0f;

        float angleX = 2.0f * 57.29578f * math.atan(q.value.x);
        angleX = math.clamp(angleX, -clamp.x, clamp.x);
        q.value.x = math.tan(0.5f * 0.0174532924f * angleX);

        float angleY = 2.0f * 57.29578f * math.atan(q.value.y);
        angleY = math.clamp(angleY, -clamp.y, clamp.y);
        q.value.y = math.tan(0.5f * 0.0174532924f * angleY);

        float angleZ = 2.0f * 57.29578f * math.atan(q.value.z);
        angleZ = math.clamp(angleZ, -clamp.z, clamp.z);
        q.value.z = math.tan(0.5f * 0.0174532924f * angleZ);

        return q;
    }
    public static quaternion RotateToTarget(float3 position, quaternion rotation, float speed, float3 targetPosition, float3 negateComponent, float3 clamp)
    {
        float3 dir = targetPosition - position;

        if (negateComponent.x == 1)
            dir.x = 0;

        if (negateComponent.y == 1)
            dir.y = 0;

        if (negateComponent.z == 1)
            dir.z = 0;

        quaternion targetRotation = quaternion.LookRotationSafe(dir, new float3(0f, 1f, 0f));

        float angleToTarget = GetQuaternionAngle(rotation, targetRotation);
        float timeToComplete = angleToTarget / speed;

        float rotationPercentage = math.min(1F, 30f / timeToComplete);

        quaternion rotationNew = math.slerp(rotation, targetRotation, rotationPercentage);

        if ((clamp.x + clamp.y + clamp.z) == 0)
        {
            return rotationNew;
        }
        else
        {
            return ClampRotation(rotationNew, new float3(clamp.x, clamp.y, clamp.z));
        }
    }
    public static quaternion RotateToTargetA(float3 position, quaternion rotation, float speed, float3 targetPosition, float3 negateComponent, float3 clamp)
    {
        //Uses MathUtils.Float3Angle for time to complete instead of MathUtils.GetQuaternionAngle

        float3 dir = targetPosition - position;

        if (negateComponent.x == 1)
            dir.x = 0;

        if (negateComponent.y == 1)
            dir.y = 0;

        if (negateComponent.z == 1)
            dir.z = 0;

        quaternion targetRotation = quaternion.LookRotationSafe(dir, new float3(0, 1, 0));

        float angleToTarget = Float3Angle(math.forward(rotation), dir);
        float timeToComplete = angleToTarget / speed;

        float rotationPercentage = math.min(1F, 30f / timeToComplete);

        quaternion rotationNew = math.slerp(rotation, targetRotation, rotationPercentage);

        if ((clamp.x + clamp.y + clamp.z) == 0)
        {
            return rotationNew;
        }
        else
        {
            return ClampRotation(rotationNew, new float3(clamp.x, clamp.y, clamp.z));
        }
    }
    public static quaternion RotateToTargetHasParent(float4x4 parentLocalToWorld, float3 position, quaternion rotation, float speed, float3 targetPosition, float3 negateComponent, float3 clamp)
    {
        //Get direction vector in world space by transforming the inverse parent entities' localToWorld.
        float4x4 inverseParentLocalToWorld = math.inverse(parentLocalToWorld);
        float3 dir = math.transform(inverseParentLocalToWorld, targetPosition) - math.transform(inverseParentLocalToWorld, position);

        if (negateComponent.x == 1)
            dir.x = 0;

        if (negateComponent.y == 1)
            dir.y = 0;

        if (negateComponent.z == 1)
            dir.z = 0;

        quaternion targetRotation = quaternion.LookRotationSafe(dir, new float3(0f, 1f, 0f));

        float angleToTarget = GetQuaternionAngle(rotation, targetRotation);
        float timeToComplete = angleToTarget / speed;

        float rotationPercentage = math.min(1F, 30f / timeToComplete);

        quaternion rotationNew = math.slerp(rotation, targetRotation, rotationPercentage);

        if ((clamp.x + clamp.y + clamp.z) == 0)
        {
            return rotationNew;
        }
        else
        {
            return ClampRotation(rotationNew, new float3(clamp.x, clamp.y, clamp.z));
        }
    }
    public static quaternion RotateToTargetAHasParent(float4x4 parentLocalToWorld, float3 position, quaternion rotation, float speed, float3 targetPosition, float3 negateComponent, float3 clamp)
    {
        //Uses MathUtils.Float3Angle for time to complete instead of MathUtils.GetQuaternionAngle

        //Get direction vector in world space by transforming the inverse parent entities' localToWorld.
        float4x4 inverseParentLocalToWorld = math.inverse(parentLocalToWorld);
        float3 dir = math.transform(inverseParentLocalToWorld, targetPosition) - math.transform(inverseParentLocalToWorld, position);

        if (negateComponent.x == 1)
            dir.x = 0;

        if (negateComponent.y == 1)
            dir.y = 0;

        if (negateComponent.z == 1)
            dir.z = 0;

        quaternion targetRotation = quaternion.LookRotationSafe(dir, new float3(0, 1, 0));

        float angleToTarget = Float3Angle(math.forward(rotation), dir);
        float timeToComplete = angleToTarget / speed;

        float rotationPercentage = math.min(1F, 30f / timeToComplete);

        quaternion rotationNew = math.slerp(rotation, targetRotation, rotationPercentage);

        if ((clamp.x + clamp.y + clamp.z) == 0)
        {
            return rotationNew;
        }
        else
        {
            return ClampRotation(rotationNew, new float3(clamp.x, clamp.y, clamp.z));
        }
    }
    public static float GetQuaternionAngle(quaternion a, quaternion b)
    {
        float f = math.dot(a, b);
        return math.acos(math.min(math.abs(f), 1f)) * 2f * 57.29578f;
    }
    public static float Float3Angle(float3 from, float3 to)
    {
        return math.acos(math.clamp(math.dot(math.normalize(from), math.normalize(to)), -1f, 1f)) * 57.29578f;
    }
    public static quaternion AngleAxis(float angle, float3 axis)
    {
        math.normalize(axis);
        float rad = angle * 0.0174532924f * 0.5f;
        axis *= math.sin(rad);
        return new quaternion(axis.x, axis.y, axis.z, math.cos(rad));
    }
    public static quaternion FromToRotation(float3 fromDirection, float3 toDirection)
    {
        float3 axis = math.cross(fromDirection, toDirection);
        float angle = Float3Angle(fromDirection, toDirection);
        return AngleAxis(angle, math.normalize(axis));
    }
}
