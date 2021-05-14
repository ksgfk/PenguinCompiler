using System.Collections.Generic;
using System.Text.Json;

namespace KSGFK
{
    /// <summary>
    /// 消息
    /// </summary>
    public static class OneBotMessage
    {
        public const string Type = "type";
        public const string Data = "data";
        public const string Text = "text";
        public const string Face = "face";
        public const string Id = "id";
        public const string At = "at";
        public const string QQ = "qq";
        public const string Reply = "reply";

        public class MessageBuilder
        {
            private readonly List<OneBotMessageSegment> _segments;

            internal MessageBuilder() { _segments = new List<OneBotMessageSegment>(); }

            public IReadOnlyList<OneBotMessageSegment> Build() { return _segments; }

            public MessageBuilder AddText(string text)
            {
                var segment = new OneBotMessageSegmentText
                {
                    Text = text
                };
                _segments.Add(segment);
                return this;
            }

            public MessageBuilder AddFace(string id)
            {
                var segment = new OneBotMessageSegmentFace
                {
                    Id = id
                };
                _segments.Add(segment);
                return this;
            }

            public MessageBuilder AddAt(string qq)
            {
                var segment = new OneBotMessageSegmentAt
                {
                    QQ = qq
                };
                _segments.Add(segment);
                return this;
            }

            public MessageBuilder AddReply(string messageId)
            {
                var segment = new OneBotMessageSegmentReply
                {
                    Id = messageId
                };
                _segments.Add(segment);
                return this;
            }
        }

        public static MessageBuilder Builder(Utf8JsonWriter writer) { return new(); }
    }
}