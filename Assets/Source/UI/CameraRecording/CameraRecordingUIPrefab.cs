using Source.UI;
using UnityEngine;

namespace Frever.UI.CameraRecording
{
    public class CameraRecordingUIPrefab : MonoBehaviour
    {
        public RectTransform recordListRoot;
        public UIButtonPrefab playButton;
        public UIButtonPrefab stopButton;
        public UIButtonPrefab recordButton;
        public RectTransform playingLabel;
        public RectTransform recordingLabel;
    }
}