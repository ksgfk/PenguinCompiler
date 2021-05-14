namespace KSGFK
{
    /// <summary>
    /// https://github.com/howmanybots/onebot/blob/master/v11/specs/event/README.md
    /// OneBot 消息类
    /// </summary>
    public static class OneBotEvent
    {
        /// <summary>
        /// 事件发生的时间戳
        /// <para>number (int64)</para>
        /// </summary>
        public const string Time = "time";

        /// <summary>
        /// 收到事件的机器人 QQ 号
        /// <para>number (int64)</para>
        /// </summary>
        public const string SelfId = "self_id";

        /// <summary>
        /// 事件类型
        /// <para>message:消息事件<see cref="EventTypeMessage"/></para>
        /// <para>notice:通知事件<see cref="EventTypeNotice"/></para>
        /// <para>request:请求事件<see cref="EventTypeRequest"/></para>
        /// <para>meta_event:元事件<see cref="EventTypeMeta"/></para>
        /// <para>string</para>
        /// </summary>
        public const string PostType = "post_type";

        public const string EventTypeMessage = "message";
        public const string EventTypeNotice = "notice";
        public const string EventTypeRequest = "request";
        public const string EventTypeMeta = "meta_event";

        /// <summary>
        /// 消息类型
        /// <para>private:私聊<see cref="MessageEventTypePrivate"/></para>
        /// <para>group:群聊<see cref="MessageEventTypeGroup"/></para>
        /// <para>string</para>
        /// </summary>
        public const string MessageType = "message_type";

        public const string MessageEventTypePrivate = "private";
        public const string MessageEventTypeGroup = "group";

        /// <summary>
        /// 消息子类型，如果是好友则是 friend，如果是群临时会话则是 group
        /// <para>string</para>
        /// </summary>
        public const string SubType = "sub_type";

        /// <summary>
        /// 消息 ID
        /// <para>number (int32)</para>
        /// </summary>
        public const string MessageId = "message_id";

        /// <summary>
        /// 发送者 QQ 号
        /// <para>number (int64)</para>
        /// </summary>
        public const string UserId = "user_id";

        /// <summary>
        /// 消息内容
        /// <para>message</para>
        /// </summary>
        public const string Message = "message";

        /// <summary>
        /// 原始消息内容
        /// <para>string</para>
        /// </summary>
        public const string RawMessage = "raw_message";

        /// <summary>
        /// 字体
        /// <para>number (int32)</para>
        /// </summary>
        public const string Font = "font";

        /// <summary>
        /// 发送人信息
        /// object
        /// </summary>
        public const string Sender = "sender";

        /// <summary>
        /// 发送者 QQ 号
        /// <para>number (int64)</para>
        /// </summary>
        public const string SenderUserId = "user_id";

        /// <summary>
        /// 昵称
        /// <para>string</para>
        /// </summary>
        public const string SenderNickName = "nickname";

        /// <summary>
        /// 性别，male 或 female 或 unknown
        /// <para>string</para>
        /// </summary>
        public const string SenderSex = "sex";

        /// <summary>
        /// 年龄
        /// <para>number (int32)</para>
        /// </summary>
        public const string SenderAge = "age";

        public const string GroupId = "group_id";
    }
}