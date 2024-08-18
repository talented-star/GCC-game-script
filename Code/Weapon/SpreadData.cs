using System;
using UnityEngine;

[Serializable]
public class SpreadData
{
    [SerializeField] protected AnimationCurve spreadOverTime = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] protected float minVerticalSpread = -1f;
    [SerializeField] protected float maxVerticalSpread = 1f;
    [SerializeField] protected float minHorizontalSpread = -1f;
    [SerializeField] protected float maxHorizontalSpread = 1f;

    public Vector2 Process(float time = 1)
    {
        Vector2 recoil = Vector2.Lerp(Vector2.zero, UnityEngine.Random.insideUnitCircle, spreadOverTime.Evaluate(time));
        recoil.x = Remap(recoil.x, -1f, 1f, minHorizontalSpread, maxHorizontalSpread);
        recoil.y = Remap(recoil.y, -1f, 1f, minVerticalSpread, maxVerticalSpread);

        return recoil;
    }

    public Quaternion GetSpreadAngle(Vector2 recoil)
    {
        return Quaternion.Euler(-recoil.y, recoil.x, 0);
    }

    public float Remap(float from, float fromMin, float fromMax, float toMin, float toMax)
    {
        var fromAbs = from - fromMin;
        var fromMaxAbs = fromMax - fromMin;

        var normal = fromAbs / fromMaxAbs;

        var toMaxAbs = toMax - toMin;
        var toAbs = toMaxAbs * normal;

        var to = toAbs + toMin;

        return to;
    }
}
