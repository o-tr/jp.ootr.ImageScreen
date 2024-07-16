using jp.ootr.common;
using UnityEditor;

namespace jp.ootr.ImageScreen.Editor
{
    [CustomEditor(typeof(ImageScreen))]
    public class ImageScreenEditor : UnityEditor.Editor
    {
        private bool debug;
        public override void OnInspectorGUI()
        {
            var script = (ImageScreen)target;
            
            debug = EditorGUILayout.ToggleLeft("Debug", debug);
            if (debug)
            {
                base.OnInspectorGUI();
                return;
            }

            EditorGUILayout.LabelField("ImageScreen", EditorStyle.UiTitle);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Device Name");
            var lastDeviceName = script.deviceName;
            script.deviceName = EditorGUILayout.TextField(script.deviceName);
            script.inputField.text = script.deviceName;

            if (lastDeviceName != script.deviceName)
            {
                EditorUtility.SetDirty(script);
            }
        }
    }
}