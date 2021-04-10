using System;

namespace UnityStreamDeck
{
    [Serializable]
    public class Message
    {
        public string Id;
    }

    [Serializable]
    public class ExecuteMenuMessage : Message
    {
        public string Path;
    }

    [Serializable]
    public class PasteComponentMessage : Message
    {
        public bool AsNew;
    }

    [Serializable]
    public class ResetObjectMessage : Message
    {
    }

    [Serializable]
    public class RotateObjectMessage : Message
    {
        public string Axis;

        public float Angle;
    }

    [Serializable]
    public class SelectObjectMessage : Message
    {
        public string Name;

        public string Tag;
    }

    [Serializable]
    public class AddComponentMessage : Message
    {
        public string Name;
    }

    public enum ObjectState
    {
        Active = 0,
        Static = 1
    }

    [Serializable]
    public class ToggleObjectStateMessage : Message
    {
        public ObjectState State;
    }

    [Serializable]
    public class TogglePauseMessage : Message
    {
    }

    [Serializable]
    public class ToggleSceneGameMessage : Message
    {
    }
}
