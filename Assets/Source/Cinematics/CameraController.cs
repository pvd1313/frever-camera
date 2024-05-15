using System;
using System.Collections.Generic;
using System.IO;
using Frever.GameLoop;
using Frever.Input;
using Source.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Frever.Cinematics
{
    public class CameraController : IInitializable, IUpdatable
    {
        private struct CameraAngleRecord
        {
            public float time;
            public Vector2 angle;
        }
        
        private readonly InputController _input;
        private readonly CameraConfig _config;

        private Transform _cameraTransform;
        private Camera _camera;
        private Vector2 _angle;
        private Vector2 _touchPosition;
        private bool _touchActive;

        private bool _isRecording;
        private double _recordStartTime;
        private double _lastRecordTime;
        private BinaryWriter _streamWriter;
        private Vector2 _lastRecordAngle;

        private bool _isPlaying;
        private BinaryReader _streamReader;
        private Action _playingCallback;
        private CameraAngleRecord _nextPlayingRecord;
        private Vector2 _playingTargetAngle;
        private Vector2 _playingAngleVelocity;
        private Vector2 _playingTempAngle;
        
        private PointerEventData _pointerEventData;
        private List<RaycastResult> _pointerRaycastResult;

        public CameraController(InputController input, CameraConfig config)
        {
            _input = input;
            _config = config;
        }
        
        public void Initialize()
        {
            CameraPrefab cameraInstance = GameObject.Instantiate(_config.cameraPrefab);

            _pointerRaycastResult = new List<RaycastResult>();
            
            _cameraTransform = cameraInstance.transform;
            _camera = cameraInstance.camera;
            _angle = _config.initialAngle;
            _lastRecordAngle = new Vector2(float.MinValue, float.MaxValue);
        }

        public void StartPlaying(Stream stream, Action onFinish)
        {
            _streamReader = new BinaryReader(stream);
            _recordStartTime = Time.realtimeSinceStartupAsDouble;
            
            if (TryReadCameraRecord(out _nextPlayingRecord))
            {
                _playingTargetAngle = _nextPlayingRecord.angle;
                _playingTempAngle = _nextPlayingRecord.angle;
                
                ApplyCameraRotation(_playingTargetAngle);
                
                _playingCallback = onFinish;
                _isPlaying = true;
            }
            else
            {
                _streamReader = null;
                _playingCallback?.Invoke();
            }
        }

        public void StopPlaying()
        {
            if (_isPlaying)
            {
                _isPlaying = false;
                _playingCallback = null;
                _streamReader = null;
            }
        }

        private bool TryReadCameraRecord(out CameraAngleRecord record)
        {
            record = default;
            
            try
            {
                record.time = _streamReader.ReadSingle();
                record.angle.x = _streamReader.ReadSingle();
                record.angle.y = _streamReader.ReadSingle();

                return true;
            }
            catch (EndOfStreamException)
            {
                return false;
            }
        }

        public void StartRecording(Stream stream)
        {
            _isRecording = true;
            _recordStartTime = Time.realtimeSinceStartupAsDouble;
            _lastRecordTime = -1;
            _streamWriter = new BinaryWriter(stream);
        }

        public void StopRecording()
        {
            if (_isRecording)
            {
                _isRecording = false;
                _streamWriter.Flush();
                _streamWriter = null;
            }
        }

        public void Update()
        {
            if (_isPlaying)
            {
                float smoothTime = 1f / _config.recordingTickRate;

                _playingTempAngle.x = Mathf.SmoothDampAngle(
                    _playingTempAngle.x, 
                    _playingTargetAngle.x, 
                    ref _playingAngleVelocity.x,
                    smoothTime);

                _playingTempAngle.y = Mathf.SmoothDampAngle(
                    _playingTempAngle.y,
                    _playingTargetAngle.y,
                    ref _playingAngleVelocity.y,
                    smoothTime);
                
                ApplyCameraRotation(_playingTempAngle);
                
                float localTime = (float) (Time.realtimeSinceStartupAsDouble - _recordStartTime);
                if (localTime > _nextPlayingRecord.time)
                {
                    _playingTargetAngle = _nextPlayingRecord.angle;

                    if (!TryReadCameraRecord(out _nextPlayingRecord))
                    {
                        _isPlaying = false;
                        _streamReader = null;
                        _playingCallback?.Invoke();
                    }
                }
                
                return;
            }
            
            TouchState touch = _input.GetTouchState();

            if (touch.button.HasFlag(ButtonState.PressedThisFrame) && !CheckPointOverlapsUI(touch.viewportPosition))
            {
                _touchActive = true;
                _touchPosition = touch.viewportPosition;
            }
            
            Vector2 touchAngle = Vector2.zero;

            if (_touchActive)
            {
                touchAngle = (touch.viewportPosition - _touchPosition) * _config.sensitivity;
                (touchAngle.x, touchAngle.y) = (touchAngle.y, touchAngle.x);
            }

            Vector2 displayAngle = _angle + touchAngle;
            displayAngle.x = Mathf.Clamp(displayAngle.x, _config.xAxisClamp.x, _config.xAxisClamp.y);
            displayAngle.y = Mathf.Repeat(displayAngle.y, 360);

            ApplyCameraRotation(displayAngle.XY0());

            if (_touchActive && touch.button.HasFlag(ButtonState.ReleasedThisFrame))
            {
                _angle = displayAngle;
                _touchActive = false;
            }

            if (_isRecording)
            {
                float deltaTick = 1f / _config.recordingTickRate;
                double timeNow = Time.realtimeSinceStartupAsDouble;

                bool anglesAreSame = Mathf.Approximately(_lastRecordAngle.x, displayAngle.x) &&
                                     Mathf.Approximately(_lastRecordAngle.y, displayAngle.y);
                
                if (timeNow - _lastRecordTime >= deltaTick && !anglesAreSame)
                {
                    float recordLocalTime = (float)(timeNow - _recordStartTime);

                    _streamWriter.Write(recordLocalTime);
                    _streamWriter.Write(displayAngle.x);
                    _streamWriter.Write(displayAngle.y);

                    _lastRecordTime = timeNow;
                    _lastRecordAngle = displayAngle;
                }
            }
        }

        private void ApplyCameraRotation(Vector2 angle)
        {
            Vector3 viewDirection = Quaternion.Euler(angle.XY0()) * (Vector3.forward * _config.zoomRadius);
            _cameraTransform.transform.rotation = Quaternion.LookRotation(viewDirection);
            _cameraTransform.transform.position = viewDirection * -1;
        }

        private bool CheckPointOverlapsUI(Vector2 viewportPoint)
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            _pointerEventData ??= new PointerEventData(EventSystem.current);
            _pointerEventData.position = new Vector2(viewportPoint.x * Screen.width, (1 - viewportPoint.y) * Screen.height);

            EventSystem.current.RaycastAll(_pointerEventData, _pointerRaycastResult);

            return _pointerRaycastResult.Count > 0;
        }
    }
}