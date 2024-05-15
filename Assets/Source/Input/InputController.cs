using UnityEngine;

namespace Frever.Input
{
    public class InputController : IInputController
    {
        public TouchState GetTouchState()
        {
            Vector3 mousePosition = UnityEngine.Input.mousePosition;
            
            return new TouchState
            {
                button = GetMouseButtonState(0),
                viewportPosition = new Vector2
                {
                    x = mousePosition.x / Screen.width,
                    y = 1 - mousePosition.y / Screen.height
                }
            };
        }

        private ButtonState GetMouseButtonState(int buttonId)
        {
            ButtonState state = default;
            
            if (UnityEngine.Input.GetMouseButton(buttonId))
            {
                state |= ButtonState.HeldDown;
            }

            if (UnityEngine.Input.GetMouseButtonDown(buttonId))
            {
                state |= ButtonState.PressedThisFrame;
            }

            if (UnityEngine.Input.GetMouseButtonUp(buttonId))
            {
                state |= ButtonState.ReleasedThisFrame;
            }

            return state;
        }
    }
}