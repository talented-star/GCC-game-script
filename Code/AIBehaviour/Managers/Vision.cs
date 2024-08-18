using UnityEngine;
using System.Collections.Generic;

namespace GrabCoin.AIBehaviour
{
    public class Vision
    {
        //static bool b = true;

        //public static bool IsVisibleUnit<T>(T unit, Transform from, List<Transform> visiblePoints, float angle, float distance, LayerMask mask) where T : BaseEnemy
        //{
        //    bool result = false;
        //    if (unit != null)
        //    {
        //        foreach (Transform visiblePoint in visiblePoints)
        //        {
        //            if (IsVisibleObject(from, visiblePoint.position, unit.gameObject, angle, distance, mask))
        //            {
        //                result = true;
        //                break;
        //            }
        //        }
        //    }
        //    return result;
        //}

        //public static bool IsVisibleUnit<T>(T unit, Transform from, Transform visiblePoint, float angle, float distance, LayerMask mask) where T : BaseEnemy
        //{
        //    bool result = false;
        //    if (unit != null)
        //    {
        //        if (IsVisibleObject(from, visiblePoint.position, unit.gameObject, angle, distance, mask))
        //        {
        //            result = true;
        //        }
        //    }
        //    return result;
        //}

        //public static bool IsVisibleUnit<T>(T unit, Transform from, Transform visiblePoint, VisionParam enemyVision) where T : BaseEnemy
        //{
        //    bool result = false;
        //    if (unit != null)
        //    {
        //        if (IsVisibleObject(from, visiblePoint.position, unit.gameObject, enemyVision.angle, enemyVision.distance, enemyVision.mask))
        //        {
        //            result = true;
        //        }
        //    }
        //    return result;
        //}

        //public static bool IsVisibleUnit<T>(T unit, Transform from, Transform visiblePoint, EnemyVision enemyVision) where T : BaseEnemy
        //{
        //    bool result = false;
        //    if (unit != null)
        //    {
        //        foreach (VisionParam visible in enemyVision.Visions)
        //        {
        //            if (IsVisibleObject(from, visiblePoint.position, unit.gameObject, visible.angle, visible.distance, visible.mask))
        //            {
        //                result = true;
        //                break;
        //            }
        //        }
        //    }
        //    return result;
        //}

        public static bool IsVisibleUnit(IBattleEnemy unit, Transform from, Transform visiblePoint, EnemyVision enemyVision)
        {
            bool result = false;
            if (unit != null)
            {
                foreach (VisionParam visible in enemyVision.Visions)
                {
                    if (IsVisibleObject(from, visiblePoint.position, unit.GetGameObject, visible.angle, visible.distance + unit.GetSize, visible.mask))
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        public static bool IsVisibleObject(Transform from, Vector3 point, GameObject target, float angle, float distance, LayerMask mask)
        {
            bool result = false;

            Vector3 fromPos = from.position + Vector3.up;
            Vector3 direction = ((point + Vector3.up * 1.5f) - fromPos);

            //Debug.Log(Mathf.Abs(lookAngle));

            if (IsAvailablePoint(from, point, direction, angle, distance))
            {
                distance += distance * 0.2f;
                Ray ray = new Ray(fromPos, direction);

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, distance, mask))
                {
                    if (hit.collider?.GetComponentInParent<IBattleEnemy>()?.GetGameObject == target)
                    {
                        // цель видна
                        result = true;
                        Debug.DrawRay(fromPos, direction, Color.green, 1);
                    }
                    else
                    {
                        // что-то загораживает цель
                        Debug.DrawRay(fromPos, direction, Color.red, 1);
                    }
                }
                else
                {
                    // Райкаст не попадает ни куда
                    Debug.DrawRay(fromPos, direction, Color.blue, 1);
                }
            }
            else
            {
                // вне зоны видимости
                Debug.DrawRay(fromPos, direction, Color.yellow, 1);
            }
            return result;
        }

        //public static bool IsAvailablePoint(Transform from, Vector3 point, float angle, float distance)
        //{

        //    bool result = false;

        //    if (from != null && Vector3.Distance(from.position, point) <= distance)
        //    {
        //        Vector3 direction = (point - from.position);
        //        float dot = Vector3.Dot(from.forward, direction.normalized);
        //        if (dot < 1)
        //        {
        //            float angleRadians = Mathf.Acos(dot);
        //            float angleDeg = angleRadians * Mathf.Rad2Deg;
        //            result = (angleDeg <= angle);
        //        }
        //        else
        //        {
        //            result = true;
        //        }
        //    }
        //    return result;
        //}

        public static bool IsAvailablePoint(Transform from, Vector3 point, Vector3 direction,float angle, float distance)
        {

            bool result = false;

            if (from != null && Vector3.Distance(from.position, point) <= distance)
            {
                //Vector3 direction = (point - from.position);
                Quaternion lookRot = Quaternion.LookRotation(direction);
                float lookAngle = lookRot.eulerAngles.y - from.rotation.eulerAngles.y;
                if (lookAngle > 180.0f)
                    lookAngle = 360.0f - lookAngle;

                result = (Mathf.Abs(lookAngle) <= angle);
            }
            return result;
        }
    }
}
