﻿#if UNITY_EDITOR
using jp.ootr.common;
using jp.ootr.ImageDeviceController.Editor;
using UnityEditor;
using VRC.SDKBase.Editor.BuildPipeline;

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

    [InitializeOnLoad]
    public class PlayModeNotifier
    {
        static PlayModeNotifier()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            var scripts = ComponentUtils.GetAllComponents<ImageScreen>();
            ImageScreenUtils.UpdateScreenNames(scripts.ToArray());
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
            foreach (var script in scripts)
            {
                UpdateScreenName(script);
            }
        }
        
        private static void UpdateScreenName(ImageScreen script)
        {
            script.inputField.text = script.deviceName;
        }
    }
}
#endif
