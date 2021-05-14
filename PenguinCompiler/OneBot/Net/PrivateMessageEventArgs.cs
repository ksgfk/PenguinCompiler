using System.Text.Json;

namespace KSGFK
{
    public class PrivateMessageEventArgs : ChatMessageEventArgs
    {
        public PrivateMessageEventArgs(JsonDocument data, Utf8JsonWriter response) : base(data, response) { }
    }
}