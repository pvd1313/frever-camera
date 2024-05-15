using System;
using UnityEngine;

namespace Frever.Cinematics
{
    [Serializable]
    public class CameraConfig
    {
        public CameraPrefab cameraPrefab;
        public Vector2 sensitivity;
        public Vector2 xAxisClamp;
        public Vector2 initialAngle;
        public float zoomRadius;
        public int recordingTickRate;
    }
}