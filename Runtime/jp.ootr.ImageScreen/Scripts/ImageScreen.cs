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

        private readonly int _animatorIsLoading = Animator.StringToHash("IsLoading");
        private readonly int _animatorShowScreenName = Animator.StringToHash("ShowScreenName");

        private bool _isLoading;
        [UdonSynced] private string _siFileName;
        private string _siLocalFileName;
        private string _siLocalSource;

        [UdonSynced] private string _siSource;

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
            animator.SetTrigger(_animatorShowScreenName);
        }

        public virtual void LoadImage(VRCUrl tmpUrl)
        {
            var tmpUrlStr = tmpUrl.ToString();
            if (_isLoading || _siSource == tmpUrlStr || tmpUrlStr.IsNullOrEmpty()) return;
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
            _siSource = source;
            _siFileName = fileName;
            Sync();
        }

        public override void _OnDeserialization()
        {
            if ((_siSource == _siLocalSource && _siFileName == _siLocalFileName) || _siSource.IsNullOrEmpty()) return;
            ConsoleDebug($"[_OnDeserialization] source: {_siSource}, fileName: {_siFileName}");
            SetLoading(true);
            controller.CcReleaseTexture(_siLocalSource, _siLocalFileName);
            LLIFetchImage(_siSource, _siSource == _siFileName ? URLType.Image : URLType.TextZip);
        }

        public override void OnFilesLoadSuccess(string source, string[] fileNames)
        {
            base.OnFilesLoadSuccess(source, fileNames);
            ConsoleDebug($"[OnFilesLoadSuccess] source: {source}");
            if (source != _siSource) return;
            if (!fileNames.Has(_siFileName)) return;
            _siLocalSource = source;
            _siLocalFileName = _siFileName;
            var texture = controller.CcGetTexture(_siLocalSource, _siLocalFileName);
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
            _isLoading = loading;
            animator.SetBool(_animatorIsLoading, loading);
        }

        public override bool IsCastableDevice()
        {
            return true;
        }
    }
}