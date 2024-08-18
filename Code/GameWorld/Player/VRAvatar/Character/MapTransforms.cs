using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class MapTransforms
{
    public Transform vrTarget;
    public Transform ikTarget;

    public TrackingOffset trackingOffset;

    public void VRMapping()
    {
        if (vrTarget == null) return;
        ikTarget.position = vrTarget.TransformPoint(trackingOffset.trackingPositionOffset);
        ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingOffset.trackingRotationOffset);
    }
}
