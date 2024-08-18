using UnityEngine;

namespace GrabCoin.GameWorld.Weapons
{
    public interface IWeaponHandler
    {
        Transform CameraTransform { get; }

        Vector3 TransformPoint(Vector3 attackPointOffset);
    }
}