using UnityEditor;
using FairyGUI;

namespace FairyGUIEditor
{
    [CustomEditor(typeof(StageCamera))]
    public class StageCameraEditor : Editor
    {
        private string[] propertyToExclude;

        private void OnEnable()
        {
            propertyToExclude = new string[] {"m_Script"};
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, propertyToExclude);

            if (serializedObject.ApplyModifiedProperties())
                (target as StageCamera).ApplyModifiedProperties();
        }
    }
}