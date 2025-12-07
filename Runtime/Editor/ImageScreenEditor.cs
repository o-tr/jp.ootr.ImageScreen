#if UNITY_EDITOR
using jp.ootr.common;
using jp.ootr.ImageDeviceController.Editor;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using VRC.SDKBase.Editor.BuildPipeline;

namespace jp.ootr.ImageScreen.Editor
{
    [CustomEditor(typeof(ImageScreen))]
    public class ImageScreenEditor : CommonDeviceEditor
    {
        protected override string GetScriptName()
        {
            return "ImageScreen";
        }

        protected override VisualElement GetContentTk()
        {
            var root = new VisualElement();
            return root;
        }
    }

    [InitializeOnLoad]
    public class PlayModeNotifier
    {
        static PlayModeNotifier()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                var scripts = ComponentUtils.GetAllComponents<ImageScreen>();
                ImageScreenUtils.UpdateScreenNames(scripts.ToArray());
            }
        }
    }

    public class ScreenNameUpdater : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            var scripts = ComponentUtils.GetAllComponents<ImageScreen>();
            ImageScreenUtils.UpdateScreenNames(scripts.ToArray());
        }
    }

    public class SetObjectReferences : UnityEditor.Editor, IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => 10;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            return CommonDeviceUtils.SetupDevices();
        }
    }

    public static class ImageScreenUtils
    {
        public static void UpdateScreenNames(ImageScreen[] scripts)
        {
            foreach (var script in scripts) UpdateScreenName(script);
        }

        private static void UpdateScreenName(ImageScreen script)
        {
            var so = new SerializedObject(script);
            var tmp = (TextMeshProUGUI)so.FindProperty("inputField").objectReferenceValue;
            if (tmp == null) return;
            var tmpSo = new SerializedObject(tmp);
            tmpSo.Update();
            tmpSo.FindProperty("m_text").stringValue = script.deviceName;
            tmpSo.ApplyModifiedProperties();
        }
    }
}
#endif
