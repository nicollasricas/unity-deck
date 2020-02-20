using System;

namespace UnityStreamDeck.Messages
{
    [Serializable]
    public class RotateObjectMessage
    {
        public string Axis;

        public float Angle;
    }
}
