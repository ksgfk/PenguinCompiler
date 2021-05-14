namespace KSGFK
{
    public class OneBotHttpEventDispatcher : OneBotHttpListener
    {
        private readonly PenguinCompilerService _penguin;

        public OneBotHttpEventDispatcher(string uri, PenguinCompilerService penguin, int maxRequest, int maxTask)
            : base(uri,
                maxRequest,
                maxTask)
        {
            _penguin = penguin;
        }

        protected override void OnPrivateMessageEvent(PrivateMessageEventArgs args) { _penguin.AddRequest(args); }

        protected override void OnGroupMessageEvent(GroupMessageEventArgs args) { _penguin.AddRequest(args); }
    }
}