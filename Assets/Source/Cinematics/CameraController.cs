using System.Collections.Generic;
using Frever.GameLoop;
using Frever.Input;
using Source.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Frever.Cinematics
{
    public class CameraController : IInitializable, IUpdatable
    {
        private readonly InputController _input;
        private readonly CameraConfig _config;

        private Transform _cameraTransform;
        private Camera _camera;
        private Vector2 _angle;
        private Vector2 _touchPosition;
        private bool _touchActive;
        
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
        }

        public void Update()
        {
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
            
            Vector3 viewDirection = Quaternion.Euler(displayAngle.XY0()) * (Vector3.forward * _config.zoomRadius);
            
            _cameraTransform.transform.rotation = Quaternion.LookRotation(viewDirection);
            _cameraTransform.transform.position = viewDirection * -1;

            if (_touchActive && touch.button.HasFlag(ButtonState.ReleasedThisFrame))
            {
                _angle = displayAngle;
                _touchActive = false;
            }
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