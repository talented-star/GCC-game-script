using UnityEngine;


namespace GrabCoin.GameWorld.Player
{
    public abstract class PivotBasedCameraRig : AbstractTargetFollower
    {
        // This script is designed to be placed on the root object of a camera rig,
        // comprising 3 gameobjects, each parented to the next:

        // 	Camera Rig
        // 		Pivot
        // 			Camera

        protected Transform _cam; // the transform of the camera
        protected Transform _pivot; // the point at which the camera pivots around
        protected Vector3 _lastTargetPosition;


        protected virtual void Awake()
        {
            // find the camera in the object hierarchy
            _cam = GetComponentInChildren<Camera>().transform;
            _pivot = _cam.parent;
        }
    }
}
