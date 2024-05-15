using Frever.GameLoop;
using Frever.UI.ItemList;
using Source.UI;
using UnityEngine;

namespace Frever.UI.CameraRecording
{
    public class CameraRecordingUIController : IInitializable
    {
        private readonly CameraRecordingUIConfig _config;

        private CameraRecordingUIPrefab _ui;
        private UIItemListView _recordListView;

        public CameraRecordingUIController(CameraRecordingUIConfig config)
        {
            _config = config;
        }
        
        public void Initialize()
        {
            _ui = GameObject.Instantiate(_config.cameraRecordingUIPrefab);
            
            _recordListView = new UIItemListView(_ui.recordListRoot, _config.itemListConfig);
            
            _recordListView.AddItem("Test0");
            _recordListView.AddItem("Test1");
            _recordListView.AddItem("Test2");

            SetButtonActive(_ui.playButton, false);
            SetButtonActive(_ui.stopButton, false);
            SetButtonActive(_ui.recordButton, true);
        }

        private static void SetButtonActive(UIButtonPrefab button, bool isActive)
        {
            Color iconColor = button.icon.color;
            iconColor.a = isActive ? 1 : 0.5f;
            button.icon.color = iconColor;
            button.button.interactable = isActive;
        }
    }
}