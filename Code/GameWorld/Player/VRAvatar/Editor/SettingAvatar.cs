using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace VRCore.Editor
{
#if UNITY_EDITOR
    public class SettingAvatar : EditorWindow
    {
        public GameObject go;

        private Vector3 _eyeOffset;
        private Vector2 _scrollPosition;
        private string _nameAvatar;
        private bool _isChangeAvatar;

        private bool _isExpandedLeft = false;
        private bool _isExpandedRight = false;
        private bool _isExpandedHead = false;

        [MenuItem("Tools/Settings VR avatar")]
        public static void ShowMyWindow() =>
            GetWindow(typeof(SettingAvatar), false, "Settings VR avatar");

        [MenuItem("Tools/Setting For VR")]
        public static void SettingVR() =>
            SettingForVRStatic(Selection.gameObjects[0].GetComponent<Animator>());

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            {
                if (Selection.gameObjects.Length == 1)
                {
                    go = Selection.gameObjects[0];
                    go = EditorGUILayout.ObjectField("Avatar", go, typeof(GameObject), true) as GameObject;
                    EditorGUILayout.Space(20f);

                    if (go.TryGetComponent(out Animator animator) && animator.avatar != null)
                    {
                        if (!go.name.Equals(_nameAvatar))
                        {
                            _isChangeAvatar = true;
                            _nameAvatar = go.name;
                        }
                        Rigging(animator);
                        VRControllers();
                    }
                }
                _isChangeAvatar = false;
            }
            EditorGUILayout.EndScrollView();
        }

        private void SettingForVR(Animator animator)
        {
            RigBuilder rigBuilder = go.AddComponent<RigBuilder>();

            GameObject rigObject = new GameObject("Rig");
            rigObject.transform.SetParent(go.transform);
            rigObject.transform.localPosition = Vector3.zero;
            rigObject.transform.localRotation = Quaternion.identity;

            RigLayer rigLayer = new RigLayer(rigObject.AddComponent<Rig>(), true);
            var layers = rigBuilder.layers;
            layers.Add(rigLayer);
            rigBuilder.layers = layers;

            Transform leftTarget = ArmSetting(animator, rigObject, "Left", HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand);
            Transform rightTarget = ArmSetting(animator, rigObject, "Right", HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand);
            Transform headTarget = HeadSetting(animator, rigObject);

            var avatarController = go.AddComponent<VRAvatarController>();

            avatarController.LeftHand.ikTarget = leftTarget;
            avatarController.RightHand.ikTarget = rightTarget;
            avatarController.Head.ikTarget = headTarget;

            CreateConfigFiles(avatarController);

            go.AddComponent<VRAnimatorController>();
        }

        private static void SettingForVRStatic(Animator animator)
        {
            RigBuilder rigBuilder = animator.gameObject.AddComponent<RigBuilder>();

            GameObject rigObject = new GameObject("Rig");
            rigObject.transform.SetParent(animator.gameObject.transform);
            rigObject.transform.localPosition = Vector3.zero;
            rigObject.transform.localRotation = Quaternion.identity;

            RigLayer rigLayer = new RigLayer(rigObject.AddComponent<Rig>(), true);
            var layers = rigBuilder.layers;
            layers.Add(rigLayer);
            rigBuilder.layers = layers;

            Transform leftTarget = ArmSetting(animator, rigObject, "Left", HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand);
            Transform rightTarget = ArmSetting(animator, rigObject, "Right", HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand);
            Transform headTarget = HeadSetting(animator, rigObject);

            var avatarController = animator.gameObject.AddComponent<VRAvatarController>();

            avatarController.LeftHand.ikTarget = leftTarget;
            avatarController.RightHand.ikTarget = rightTarget;
            avatarController.Head.ikTarget = headTarget;

            CreateConfigFiles(avatarController);

            animator.gameObject.AddComponent<VRAnimatorController>();
        }

        private void Rigging(Animator animator)
        {
            if (go.TryGetComponent(out RigBuilder rigBuilder))
            {
                Rig rig = rigBuilder.layers[0].rig;
                if (rig == null) return;
                var armConstraints = rig.GetComponentsInChildren<TwoBoneIKConstraint>();
                float weight = EditorGUILayout.Slider(new GUIContent("Hint weight"), armConstraints[0].data.hintWeight, 0, 1);
                foreach (var armConstraint in armConstraints)
                    armConstraint.data.hintWeight = weight;
            }
            else
            {
                if (GUILayout.Button("Setting for VR"))
                    SettingForVR(animator);
            }
        }

        private void VRControllers()
        {
            if (go.TryGetComponent(out VRAvatarController avatarController))
            {
                if (_isChangeAvatar)
                    _eyeOffset = avatarController.CameraHeadOffset;
                _eyeOffset = EditorGUILayout.Vector3Field(new GUIContent("Camera head offset"), _eyeOffset);
                avatarController.CameraHeadOffset = _eyeOffset;
                
                EditorGUILayout.Space(15f);
                _isExpandedLeft = EditorGUILayout.Foldout(_isExpandedLeft, "Left hand");
                if (_isExpandedLeft)
                {
                    avatarController.LeftHand.trackingOffset.trackingPositionOffset = 
                        EditorGUILayout.Vector3Field(new GUIContent("Left hand position"), avatarController.LeftHand.trackingOffset.trackingPositionOffset);
                    avatarController.LeftHand.trackingOffset.trackingRotationOffset = 
                        EditorGUILayout.Vector3Field(new GUIContent("Left hand rotation"), avatarController.LeftHand.trackingOffset.trackingRotationOffset);
                }
                
                EditorGUILayout.Space(15f);
                _isExpandedRight = EditorGUILayout.Foldout(_isExpandedRight, "Right hand");
                if (_isExpandedRight)
                {
                    avatarController.RightHand.trackingOffset.trackingPositionOffset = 
                        EditorGUILayout.Vector3Field(new GUIContent("Right hand position"), avatarController.RightHand.trackingOffset.trackingPositionOffset);
                    avatarController.RightHand.trackingOffset.trackingRotationOffset = 
                        EditorGUILayout.Vector3Field(new GUIContent("Right hand rotation"), avatarController.RightHand.trackingOffset.trackingRotationOffset);
                }
                
                EditorGUILayout.Space(15f);
                _isExpandedHead = EditorGUILayout.Foldout(_isExpandedHead, "Head");
                if (_isExpandedHead)
                {
                    avatarController.Head.trackingOffset.trackingPositionOffset = 
                        EditorGUILayout.Vector3Field(new GUIContent("Head position"), avatarController.Head.trackingOffset.trackingPositionOffset);
                    avatarController.Head.trackingOffset.trackingRotationOffset = 
                        EditorGUILayout.Vector3Field(new GUIContent("Head rotation"), avatarController.Head.trackingOffset.trackingRotationOffset);
                }
            }
        }

        private static Transform ArmSetting(Animator animator, GameObject rigObject, string name, params HumanBodyBones[] bones)
        {
            GameObject constraintObject = new GameObject($"IK{name}Arm");
            constraintObject.transform.SetParent(rigObject.transform);
            constraintObject.transform.localPosition = Vector3.zero;
            constraintObject.transform.localRotation = Quaternion.identity;

            GameObject hint = new GameObject("Hint");
            hint.transform.SetParent(constraintObject.transform);
            hint.transform.position = animator.GetBoneTransform(bones[1]).position;
            hint.transform.rotation = animator.GetBoneTransform(bones[1]).rotation;

            GameObject target = new GameObject("Target");
            target.transform.SetParent(constraintObject.transform);
            target.transform.position = animator.GetBoneTransform(bones[2]).position;
            target.transform.rotation = animator.GetBoneTransform(bones[2]).rotation;

            TwoBoneIKConstraint constraintArm = constraintObject.AddComponent<TwoBoneIKConstraint>();
            constraintArm.data.root = animator.GetBoneTransform(bones[0]);
            constraintArm.data.mid = animator.GetBoneTransform(bones[1]);
            constraintArm.data.tip = animator.GetBoneTransform(bones[2]);
            constraintArm.data.target = target.transform;
            constraintArm.data.hint = hint.transform;
            constraintArm.data.hintWeight = 0.3f;

            return target.transform;
        }

        private static Transform HeadSetting(Animator animator, GameObject rigObject)
        {
            GameObject constraintObject = new GameObject("IKHead");
            constraintObject.transform.SetParent(rigObject.transform);
            constraintObject.transform.localPosition = animator.GetBoneTransform(HumanBodyBones.Head).position;
            constraintObject.transform.localRotation = animator.GetBoneTransform(HumanBodyBones.Head).rotation;

            MultiParentConstraint constraintHead = constraintObject.AddComponent<MultiParentConstraint>();
            constraintHead.data.constrainedObject = animator.GetBoneTransform(HumanBodyBones.Head);

            WeightedTransformArray weightedTransforms = constraintHead.data.sourceObjects;
            weightedTransforms.Add(new WeightedTransform(constraintObject.transform, 1));
            constraintHead.data.sourceObjects = weightedTransforms;

            return constraintObject.transform;
        }

        private static void CreateConfigFiles(VRAvatarController avatarController)
        {
            if (!Directory.Exists(Application.dataPath + "/Core/Config/Tracking/" + $"{avatarController.gameObject.name}Offset/"))
                Directory.CreateDirectory(Application.dataPath + "/Core/Config/Tracking/" + $"{avatarController.gameObject.name}Offset/");

            var leftHandOffset = ScriptableObject.CreateInstance<TrackingOffset>();
            avatarController.LeftHand.trackingOffset = leftHandOffset;
            AssetDatabase.CreateAsset(leftHandOffset, $"Assets/Core/Config/Tracking/{avatarController.gameObject.name}Offset/LeftHandOffset.asset");
            AssetDatabase.SaveAssets();

            var rightHandOffset = ScriptableObject.CreateInstance<TrackingOffset>();
            avatarController.RightHand.trackingOffset = rightHandOffset;
            AssetDatabase.CreateAsset(rightHandOffset, $"Assets/Core/Config/Tracking/{avatarController.gameObject.name}Offset/RightHandOffset.asset");
            AssetDatabase.SaveAssets();

            var headOffset = ScriptableObject.CreateInstance<TrackingOffset>();
            avatarController.Head.trackingOffset = headOffset;
            AssetDatabase.CreateAsset(headOffset, $"Assets/Core/Config/Tracking/{avatarController.gameObject.name}Offset/HeadOffset.asset");
            AssetDatabase.SaveAssets();
        }
    }
#endif
}