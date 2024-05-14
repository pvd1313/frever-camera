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

        private Transform _camera;
        private Vector2 _angle;
        private Vector2 _touchPosition;

        public CameraController(InputController input, CameraConfig config)
        {
            _input = input;
            _config = config;
        }
        
        public void Initialize()
        {
            _camera = GameObject.Instantiate(_config.cameraPrefab).transform;
        }

        public void Update()
        {
            TouchState touch = _input.GetTouchState();

            if (touch.button.HasFlag(ButtonState.PressedThisFrame))
            {
                _touchPosition = touch.viewportPosition;
            }
            
            Vector2 touchAngle = Vector2.zero;

            if (touch.button.HasFlag(ButtonState.HeldDown) || touch.button.HasFlag(ButtonState.ReleasedThisFrame))
            {
                touchAngle = (touch.viewportPosition - _touchPosition) * _config.sensitivity;
                (touchAngle.x, touchAngle.y) = (touchAngle.y, touchAngle.x);
                touchAngle.x *= -1;
            }

            Vector2 displayAngle = _angle + touchAngle;
            displayAngle.x = Mathf.Clamp(displayAngle.x, _config.xAxisClamp.x, _config.xAxisClamp.y);
            
            Vector3 viewDirection = Quaternion.Euler(displayAngle.XY0()) * (Vector3.forward * _config.zoomRadius);
            
            _camera.transform.rotation = Quaternion.LookRotation(viewDirection);
            _camera.transform.position = viewDirection * -1;

            if (touch.button.HasFlag(ButtonState.ReleasedThisFrame))
            {
                _angle = displayAngle;
            }
        }
    }
}