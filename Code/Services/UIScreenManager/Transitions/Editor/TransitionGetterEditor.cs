using UnityEditor;

namespace GrabCoin.UI.ScreenManager.Transitions
{
    [CustomEditor(typeof(TransitionGetter))]
    public class TransitionGetterEditor : Editor
    {
        private const string TransitionSorucePropertyName = "_transitionSource";
        private const string TransitionTypePropertyName = "_transitionType";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var transitionSourceProperty = serializedObject.FindProperty(TransitionSorucePropertyName);
            bool manualSelect = transitionSourceProperty.enumValueIndex == (int)TransitionSource.ManualSelect;

            if (manualSelect)
                DrawDefaultInspector();
            else
                DrawPropertiesExcluding(serializedObject, TransitionTypePropertyName);

            serializedObject.ApplyModifiedProperties();
        }
    }
}