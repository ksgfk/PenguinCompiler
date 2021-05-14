using System.Text.Json;

namespace KSGFK
{
    public class GroupMessageEventArgs : ChatMessageEventArgs
    {
        public GroupMessageEventArgs(JsonDocument data, Utf8JsonWriter response) : base(data, response) { }

        public long GetGroupId()
        {
            var root = Data.RootElement;
            var idProperty = root.GetProperty(OneBotEvent.GroupId);
            return idProperty.GetInt64();
        }
    }
}