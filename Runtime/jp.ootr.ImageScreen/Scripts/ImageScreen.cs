using jp.ootr.common;
using jp.ootr.ImageDeviceController;
using jp.ootr.ImageDeviceController.CommonDevice;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace jp.ootr.ImageScreen
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ImageScreen : CommonDevice
    {
        [SerializeField] protected RawImage image;
        [SerializeField] protected AspectRatioFitter aspectRatioFitter;

        [SerializeField] public TextMeshProUGUI inputField;

        protected readonly int AnimatorIsLoading = Animator.StringToHash("IsLoading");
        protected readonly int AnimatorShowScreenName = Animator.StringToHash("ShowScreenName");

        protected bool IsLoading;
        [UdonSynced] protected string SiFileName;
        protected string SiLocalFileName;
        protected string SiLocalSource;

        [UdonSynced] protected string SiSource;

        public override string GetClassName()
        {
            return "jp.ootr.ImageScreen.ImageScreen";
        }

        public override string GetDisplayName()
        {
            return "ImageScreen";
        }

        public override void ShowScreenName()
        {
            animator.SetTrigger(AnimatorShowScreenName);
        }

        public virtual void LoadImage(VRCUrl tmpUrl)
        {
            var tmpUrlStr = tmpUrl.ToString();
            if (IsLoading || SiSource == tmpUrlStr || tmpUrlStr.IsNullOrEmpty()) return;
            if (!tmpUrlStr.IsValidUrl(out var error))
            {
                OnFilesLoadFailed(error);
                return;
            }

            controller.UsAddUrl(tmpUrl);
            SetLoading(true);
        }

        public virtual void UsOnUrlSynced(string source)
        {
            LoadImage(source, source);
        }

        public override void LoadImage(string source, string fileName, bool shouldPushHistory = false)
        {
            ConsoleDebug($"[LoadImage] source: {source}, fileName: {fileName}");
            SiSource = source;
            SiFileName = fileName;
            Sync();
        }

        public override void _OnDeserialization()
        {
            if ((SiSource == SiLocalSource && SiFileName == SiLocalFileName) || SiSource.IsNullOrEmpty()) return;
            ConsoleDebug($"[_OnDeserialization] source: {SiSource}, fileName: {SiFileName}");
            SetLoading(true);
            controller.CcReleaseTexture(SiLocalSource, SiLocalFileName);
            LLIFetchImage(SiSource, SiSource == SiFileName ? URLType.Image : URLType.TextZip);
        }

        public override void OnFilesLoadSuccess(string source, string[] fileNames)
        {
            base.OnFilesLoadSuccess(source, fileNames);
            ConsoleDebug($"[OnFilesLoadSuccess] source: {source}");
            if (source != SiSource) return;
            if (!fileNames.Has(SiFileName)) return;
            SiLocalSource = source;
            SiLocalFileName = SiFileName;
            var texture = controller.CcGetTexture(SiLocalSource, SiLocalFileName);
            image.texture = texture;
            aspectRatioFitter.aspectRatio = (float)texture.width / texture.height;
            SetLoading(false);
        }

        public override void OnFilesLoadFailed(LoadError error)
        {
            base.OnFilesLoadFailed(error);
            SetLoading(false);
        }

        protected virtual void SetLoading(bool loading)
        {
            IsLoading = loading;
            animator.SetBool(AnimatorIsLoading, loading);
        }

        public override bool IsCastableDevice()
        {
            return true;
        }
    }
}