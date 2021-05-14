using System.Collections.Generic;
using System.Text.Json;

namespace KSGFK
{
    /// <summary>
    /// 消息段数据
    /// </summary>
    public abstract record OneBotMessageSegment
    {
        public abstract string Type { get; }

        public static void WriteArray(Utf8JsonWriter writer, IReadOnlyList<OneBotMessageSegment> segments)
        {
            writer.WriteStartArray();
            foreach (var segment in segments)
            {
                segment.WriteToJson(writer);
            }
            writer.WriteEndArray();
        }

        public void WriteToJson(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteString(OneBotMessage.Type, Type);
            writer.WritePropertyName(OneBotMessage.Data);
            WriteData(writer);
            writer.WriteEndObject();
        }

        protected abstract void WriteData(Utf8JsonWriter writer);
    }

    /// <summary>
    /// 纯文本
    /// </summary>
    public record OneBotMessageSegmentText : OneBotMessageSegment
    {
        public override string Type => OneBotMessage.Text;
        public string Text { get; init; }

        protected override void WriteData(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteString(OneBotMessage.Text, Text);
            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// QQ表情
    /// </summary>
    public record OneBotMessageSegmentFace : OneBotMessageSegment
    {
        public override string Type => OneBotMessage.Face;
        public string Id { get; init; }

        protected override void WriteData(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteString(OneBotMessage.Id, Id);
            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// @某人
    /// </summary>
    public record OneBotMessageSegmentAt : OneBotMessageSegment
    {
        public override string Type => OneBotMessage.At;
        public string QQ { get; init; }

        protected override void WriteData(
            Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteString(OneBotMessage.QQ, QQ);
            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// 回复
    /// </summary>
    public record OneBotMessageSegmentReply : OneBotMessageSegment
    {
        public override string Type => OneBotMessage.Reply;

        /// <summary>
        /// 回复时引用的消息 ID
        /// </summary>
        public string Id { get; init; }

        protected override void WriteData(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteString(OneBotMessage.Id, Id);
            writer.WriteEndObject();
        }
    }
}