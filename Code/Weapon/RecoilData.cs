using System;
using UnityEngine;

[Serializable]
public class RecoilData
{
    [SerializeField] protected AnimationCurve recoilOverTime = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] protected float minVerticalRecoil = 0.5f;
    [SerializeField] protected float maxVerticalRecoil = 1f;
    [SerializeField] protected float minHorizontalRecoil = -0.5f;
    [SerializeField] protected float maxHorizontalRecoil = 0.5f;
    public Vector2 Process(float time = 1)
    {
        Vector2 recoil = Vector2.Lerp(Vector2.zero, UnityEngine.Random.insideUnitCircle, recoilOverTime.Evaluate(time));
        recoil.x = Remap(recoil.x, -1f, 1f, minHorizontalRecoil, maxHorizontalRecoil);
        recoil.y = Remap(recoil.y, -1f, 1f, minVerticalRecoil, maxVerticalRecoil);

        return recoil;
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
