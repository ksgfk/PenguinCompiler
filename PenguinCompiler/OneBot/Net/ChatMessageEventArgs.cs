using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace KSGFK
{
    public abstract class ChatMessageEventArgs
    {
        public const string QuickOpReply = "reply";

        public record Segment(string Type, in JsonElement Data);

        private string _msg;
        private Segment[] _segments;
        private readonly object _lock = new();

        public JsonDocument Data { get; }
        public Utf8JsonWriter Response { get; }

        public ChatMessageEventArgs(JsonDocument data, Utf8JsonWriter response)
        {
            Data = data;
            Response = response;
        }

        public string GetStringMessage()
        {
            if (_msg != null)
            {
                return _msg;
            }

            var seg = GetMessageSegments();
            lock (_lock)
            {
                var builder = new StringBuilder();
                foreach (var segment in seg)
                {
                    builder.Append(GetSimpleString(segment));
                }
                _msg = builder.ToString();
            }
            return _msg;
        }

        public Segment[] GetMessageSegments()
        {
            if (_segments != null)
            {
                return _segments;
            }

            lock (_lock)
            {
                var root = Data.RootElement;
                var msgProperty = root.GetProperty(OneBotEvent.Message);

                static Segment GetSegment(in JsonElement msg)
                {
                    var typeProperty = msg.GetProperty(OneBotMessage.Type);
                    var dataProperty = msg.GetProperty(OneBotMessage.Data);
                    var segment = new Segment(typeProperty.GetString(), in dataProperty);
                    return segment;
                }

                switch (msgProperty.ValueKind)
                {
                    case JsonValueKind.Array:
                    {
                        var arr = new List<Segment>();
                        foreach (var i in msgProperty.EnumerateArray())
                        {
                            arr.Add(GetSegment(in i));
                        }
                        _segments = arr.ToArray();
                        break;
                    }
                    case JsonValueKind.Object:
                        _segments = new Segment[1];
                        _segments[0] = GetSegment(in msgProperty);
                        break;
                    default:
                    {
                        if (msgProperty.ValueKind == JsonValueKind.Object)
                        {
                            throw new NotImplementedException("未实现 string 类型 message解析");
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                }
            }
            return _segments;
        }

        public long GetUserId()
        {
            var root = Data.RootElement;
            var idProperty = root.GetProperty(OneBotEvent.UserId);
            return idProperty.GetInt64();
        }

        public int GetMessageId()
        {
            var root = Data.RootElement;
            var idProperty = root.GetProperty(OneBotEvent.MessageId);
            return idProperty.GetInt32();
        }

        /// <summary>
        /// 立刻回复，请保证输入是符合One Bot规范的消息
        /// </summary>
        public void Reply(in JsonElement message)
        {
            Response.WriteStartObject();
            Response.WritePropertyName(QuickOpReply);
            Response.WriteStartArray();
            message.WriteTo(Response);
            Response.WriteEndArray();
            Response.WriteEndObject();
        }

        /// <summary>
        /// 立刻回复
        /// </summary>
        public void Reply(IReadOnlyList<OneBotMessageSegment> segments)
        {
            Response.WriteStartObject();
            Response.WritePropertyName(QuickOpReply);
            OneBotMessageSegment.WriteArray(Response, segments);
            Response.WriteEndObject();
        }

        public static string GetSimpleString(Segment segment)
        {
            var builder = new StringBuilder();
            foreach (var property in segment.Data.EnumerateObject())
            {
                builder.Append(property.Value.GetString());
            }
            return builder.ToString();
        }
    }
}