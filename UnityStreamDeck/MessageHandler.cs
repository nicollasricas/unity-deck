using System;

namespace UnityStreamDeck
{
    public class MessageHandler
    {
        public Type Type { get; set; }

        public Action<Message> Handler { get; set; }

        public MessageHandler(Type type, Action<Message> handler)
        {
            Type = type;
            Handler = handler;
        }
    }
}
