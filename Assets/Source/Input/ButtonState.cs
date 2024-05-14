using System;

namespace Frever.Input
{
    [Flags]
    public enum ButtonState
    {
        Invalid,
        HeldDown = 1,
        PressedThisFrame = 2,
        ReleasedThisFrame = 4
    }
}