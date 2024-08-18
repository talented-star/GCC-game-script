using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "TrackingOffset", menuName = "ScriptableObjects/Create tracking offset")]
public class TrackingOffset : ScriptableObject
{
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;
}
