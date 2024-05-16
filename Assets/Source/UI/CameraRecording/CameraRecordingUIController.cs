using System;
using System.Collections.Generic;
using System.IO;
using Frever.Cinematics;
using Frever.GameLoop;
using Frever.UI.ItemList;
using Source.UI;
using UnityEngine;

namespace Frever.UI.CameraRecording
{
    public class CameraRecordingUIController : IInitializable, IGizmosDrawer, IDisposable
    {
        private const string FileExtension = ".rec";
        
        private readonly CameraRecordingUIConfig _config;
        private readonly CameraController _camera;

        private CameraRecordingUIPrefab _ui;
        private UIItemListView _recordListView;

        private FileStream _recordStream;
        private bool _isRecording;
        private bool _isPlaying;

        private List<Vector3> _gizmosTrajectory;

        public CameraRecordingUIController(CameraRecordingUIConfig config, CameraController camera)
        {
            _config = config;
            _camera = camera;
        }
        
        public void Initialize()
        {
            _ui = GameObject.Instantiate(_config.cameraRecordingUIPrefab);
            _ui.recordButton.button.onClick.AddListener(OnRecordButtonPressed);
            _ui.stopButton.button.onClick.AddListener(OnStopButtonPressed);
            _ui.playButton.button.onClick.AddListener(OnPlayButtonPressed);
            
            _recordListView = new UIItemListView(_ui.recordListRoot, _config.itemListConfig);
            _recordListView.selectionChange += OnListItemSelectionChanged;
            
            string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*" + FileExtension);
            foreach (string filePath in filePaths)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                _recordListView.AddItem(fileName);
            }

            UpdateUIState();
        }

        private void OnListItemSelectionChanged(int selectedIndex)
        {
            UpdateUIState();
            
            UpdateGizmosTrajectory();
        }

        private void OnRecordButtonPressed()
        {
            string fileName = GenerateFileName();
            string filePath = GetFilePath(fileName);

            _recordStream = File.Create(filePath);
            _isRecording = true;

            int itemIndex = _recordListView.AddItem(fileName);
            _recordListView.SelectItem(itemIndex);
            
            _camera.StartRecording(_recordStream);
            
            UpdateUIState();
        }

        private void OnStopButtonPressed()
        {
            if (_isRecording)
            {
                _camera.StopRecording();
                
                _isRecording = false;
                _recordStream.Flush();
                _recordStream.Dispose();
                _recordStream = null;
                
                UpdateGizmosTrajectory();
            }

            if (_isPlaying)
            {
                _camera.StopPlaying();
                
                _isPlaying = false;
                _recordStream.Dispose();
                _recordStream = null;
            }
            
            UpdateUIState();
        }

        private void OnCameraStopPlaying()
        {
            _isPlaying = false;
            _recordStream.Dispose();
            _recordStream = null;
            
            UpdateUIState();
        }

        private void OnPlayButtonPressed()
        {
            string fileName = _recordListView.GetItem(_recordListView.selectedItemIndex);
            string filePath = GetFilePath(fileName);
            _recordStream = File.OpenRead(filePath);
            _isPlaying = true;
            
            UpdateUIState();

            _camera.StartPlaying(_recordStream, OnCameraStopPlaying);
        }

        private void UpdateUIState()
        {
            bool anyFileSelected = _recordListView.selectedItemIndex != -1;
            
            SetButtonActive(_ui.recordButton, !_isRecording && !_isPlaying);
            SetButtonActive(_ui.playButton, !_isRecording && anyFileSelected && !_isPlaying);
            SetButtonActive(_ui.stopButton, _isRecording || _isPlaying);

            _recordListView.SetInteractable(!_isRecording && !_isPlaying);

            _ui.playingLabel.gameObject.SetActive(_isPlaying);
            _ui.recordingLabel.gameObject.SetActive(_isRecording);
        }
        
        private static void SetButtonActive(UIButtonPrefab button, bool isActive)
        {
            Color iconColor = button.icon.color;
            iconColor.a = isActive ? 1 : 0.5f;
            button.icon.color = iconColor;
            button.button.interactable = isActive;
        }

        private void UpdateGizmosTrajectory()
        {
            int selectedIndex = _recordListView.selectedItemIndex;
            if (selectedIndex == -1)
            {
                return;
            }

            string fileName = _recordListView.GetItem(selectedIndex);
            string filePath = GetFilePath(fileName);
            using FileStream fileStream = File.OpenRead(filePath);

            _gizmosTrajectory = _camera.BuildTrajectory(fileStream);
        }

        private static string GenerateFileName()
        {
            return DateTime.Now.ToString("dd-MMM-yy HH-mm-ss");
        }

        private static string GetFilePath(string fileName)
        {
            return Path.Combine(Application.persistentDataPath, fileName + FileExtension);
        }
        
        public void DrawGizmos()
        {
            if (_gizmosTrajectory == null)
            {
                return;
            }
            
            Gizmos.color = Color.yellow;

            for (int i = 1; i < _gizmosTrajectory.Count; i++)
            {
                Gizmos.DrawLine(_gizmosTrajectory[i - 1], _gizmosTrajectory[i]);
            }
        }

        public void Dispose()
        {
            if (_recordStream != null)
            {
                _recordStream.Dispose();
                _recordStream = null;
            }
        }
    }
}