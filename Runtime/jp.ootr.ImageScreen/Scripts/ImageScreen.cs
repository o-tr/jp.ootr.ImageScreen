using System;
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

        public ulong lastImageUpdated;

        private readonly int _animatorIsLoading = Animator.StringToHash("IsLoading");
        private readonly int _animatorShowScreenName = Animator.StringToHash("ShowScreenName");
        private readonly int _animatorShowSplash = Animator.StringToHash("ShowSplash");

        private readonly string[] _imageScreenPrefixes = { "ImageScreen" };

        private bool _isInitialized;

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
                OnSourceLoadFailed(error);
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
            ConsoleDebug($"laod image: {source}, {fileName}", _imageScreenPrefixes);
            _siSource = source;
            _siFileName = fileName;
            lastImageUpdated = DateTime.Now.ToUnixTime();
            Sync();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            base.OnPlayerJoined(player);
            if (!Networking.IsOwner(gameObject) || player.isLocal) return;
            if (_siSource.IsNullOrEmpty()) return;
            Sync();
        }

        public override void _OnDeserialization()
        {
            if ((_siSource == _siLocalSource && _siFileName == _siLocalFileName) || _siSource.IsNullOrEmpty()) return;
            ConsoleDebug($"_OnDeserialization: {_siSource}, {_siFileName}", _imageScreenPrefixes);
            SetLoading(true);
            controller.CcReleaseTexture(_siLocalSource, _siLocalFileName);

            _siFileName.ParseFileName(out var type, out var options);

            LLIFetchImage(_siSource, type, options);
        }

        public override void OnSourceLoadSuccess(string source, string[] fileNames)
        {
            base.OnSourceLoadSuccess(source, fileNames);
            ConsoleDebug($"image load success: {source}, {string.Join(",", fileNames)}", _imageScreenPrefixes);
            if (source != _siSource) return;
            if (!fileNames.Has(_siFileName)) return;
            _siLocalSource = source;
            _siLocalFileName = _siFileName;
            
            controller.LoadFile(this, source, _siFileName, 100);
        }

        public override void OnSourceLoadFailed(LoadError error)
        {
            base.OnSourceLoadFailed(error);
            ConsoleError($"image load failed: {error.GetString()}", _imageScreenPrefixes);
            SetLoading(false);
        }

        public override void OnFileLoadSuccess(string source, string fileUrl, string channel)
        {
            base.OnFileLoadSuccess(source, fileUrl, channel);
            ConsoleDebug($"image file load success: {source}, {fileUrl}, {channel}", _imageScreenPrefixes);
            var texture = controller.CcGetTexture(_siLocalSource, _siLocalFileName);
            if (texture != null)
            {
                image.texture = texture;
                aspectRatioFitter.aspectRatio = (float)texture.width / texture.height;
            }

            SetLoading(false);
            if (_isInitialized) return;
            _isInitialized = true;
            animator.SetBool(_animatorShowSplash, false);
        }

        protected virtual void SetLoading(bool loading)
        {
            _isLoading = loading;
            animator.SetBool(_animatorIsLoading, loading);
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            base.OnPlayerJoined(player);
            if (player.isLocal || !Networking.IsOwner(gameObject))
            {
                return;
            }
            
            Sync();
        }

        public override bool IsCastableDevice()
        {
            return true;
        }
    }
}
