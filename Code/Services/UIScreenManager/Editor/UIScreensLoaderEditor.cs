using UnityEditor;

namespace GrabCoin.UI.ScreenManager
{
    [CustomEditor(typeof(UIScreensLoader))]
    public class UIScreensLoaderEditor : Editor
    {
        private const string SearchInallAssembliesProperyName = "_searchInAllAssemblies";
        private const string AssemblyNamesProperyName = "_assemblyNames";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var searchInAllAssembliesProperty = serializedObject.FindProperty(SearchInallAssembliesProperyName);
            bool searchInAllAssemblies = searchInAllAssembliesProperty.boolValue;

            if (!searchInAllAssemblies)
                DrawDefaultInspector();
            else
                DrawPropertiesExcluding(serializedObject, AssemblyNamesProperyName);

            serializedObject.ApplyModifiedProperties();
        }
    }
}