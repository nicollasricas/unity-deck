using System;

namespace UnityStreamDeck
{
    public class MessageHandler
    {
        public Type Type { get; set; }

        public Delegate Handler { get; set; }
    }
}
