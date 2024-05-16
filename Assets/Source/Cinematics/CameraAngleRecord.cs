using System.IO;
using UnityEngine;

namespace Frever.Cinematics
{
    public struct CameraAngleRecord
    {
        public const int RecordSizeBytes = 12;
        
        public float time;
        public Vector2 angle;

        public bool TryReadFromStream(BinaryReader reader)
        {
            try
            {
                time = reader.ReadSingle();
                angle.x = reader.ReadSingle();
                angle.y = reader.ReadSingle();

                return true;
            }
            catch (EndOfStreamException)
            {
                return false;
            }
        }

        public void WriteToStream(BinaryWriter writer)
        {
            writer.Write(time);
            writer.Write(angle.x);
            writer.Write(angle.y);
        }
    }
}