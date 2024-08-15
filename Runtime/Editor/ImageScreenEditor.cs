using jp.ootr.common;
using jp.ootr.ImageDeviceController.Editor;
using UnityEditor;

namespace jp.ootr.ImageScreen.Editor
{
    [CustomEditor(typeof(ImageScreen))]
    public class ImageScreenEditor : CommonDeviceEditor
    {
        protected override void ShowScriptName()
        {
            EditorGUILayout.LabelField("ImageScreen", EditorStyle.UiTitle);
        }
    }
}