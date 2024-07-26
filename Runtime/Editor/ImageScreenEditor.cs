using jp.ootr.common;
using jp.ootr.ImageDeviceController.Editor;
using UnityEditor;

namespace jp.ootr.ImageScreen.Editor
{
    [CustomEditor(typeof(ImageScreen))]
    public class ImageScreenEditor : CommonDeviceEditor
    {
        private bool debug;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (Debug)
            {
                return;
            }
            var script = (ImageScreen)target;
            
            EditorGUILayout.LabelField("Device Name");
            var lastDeviceName = script.deviceName;
            script.deviceName = EditorGUILayout.TextField(script.deviceName);
            script.inputField.text = script.deviceName;

            if (lastDeviceName != script.deviceName)
            {
                EditorUtility.SetDirty(script);
            }
        }
        public override void ShowScriptName()
        {
            EditorGUILayout.LabelField("ImageScreen", EditorStyle.UiTitle);
        }
    }
}