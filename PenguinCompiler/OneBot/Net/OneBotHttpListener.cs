using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KSGFK
{
    public class OneBotHttpListener : Service<HttpListenerContext>
    {
        private readonly HttpListener _listener;

        public OneBotHttpListener(string uri, int maxRequest, int maxTask) : base(maxRequest, maxTask)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(uri);
        }

        protected override void Start()
        {
            _listener.Start();
            Console.WriteLine("start listening...");
        }

        protected override async Task<HttpListenerContext> GetRequestAsync()
        {
            try
            {
                var result = await _listener.GetContextAsync();
                return result;
            }
            catch (HttpListenerException)
            {
                throw new TaskCanceledException();
            }
            catch (ObjectDisposedException)
            {
                throw new TaskCanceledException();
            }
        }

        protected override async Task RunTaskAsync(HttpListenerContext context)
        {
            var request = context.Request;
            JsonDocument document = null;
            await using (var input = request.InputStream)
            {
                try
                {
                    document = await JsonDocument.ParseAsync(input, cancellationToken: CancelSource.Token);
                }
                catch (JsonException e)
                {
                    Console.WriteLine(e);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(e);
                }
            }

            var response = context.Response;
            response.ContentType = "application/json";
            response.ContentEncoding = Encoding.UTF8;
            response.AppendHeader("Content-Type", "application/json");
            await using (var responseStream = response.OutputStream)
            {
                if (document == null)
                {
                    response.StatusCode = 400;
                }
                else
                {
                    response.StatusCode = 200;
                    await using var writer = new Utf8JsonWriter(responseStream);
                    SubmitEvent(document, writer);
                }
            }
            response.Close();
        }

        public override void Dispose()
        {
            base.Dispose();
            _listener.Close();
            _listener.Abort();
            Console.WriteLine("stop listening.bye...");
        }

        private void SubmitEvent(JsonDocument eventArgs, Utf8JsonWriter response)
        {
            var root = eventArgs.RootElement;
            var postTypeProperty = root.GetProperty(OneBotEvent.PostType);
            var postType = postTypeProperty.GetString();
            switch (postType)
            {
                case OneBotEvent.EventTypeMessage:
                {
                    var msgTypeProperty = root.GetProperty(OneBotEvent.MessageType);
                    var msgType = msgTypeProperty.GetString();
                    switch (msgType)
                    {
                        case OneBotEvent.MessageEventTypePrivate:
                            OnPrivateMessageEvent(new PrivateMessageEventArgs(eventArgs, response));
                            break;
                        case OneBotEvent.MessageEventTypeGroup:
                            OnGroupMessageEvent(new GroupMessageEventArgs(eventArgs, response));
                            break;
                        default:
                            Console.WriteLine($"不支持的消息事件类型 {msgType}");
                            break;
                    }
                    break;
                }
                case OneBotEvent.EventTypeNotice:
                    break;
                case OneBotEvent.EventTypeRequest:
                    break;
                case OneBotEvent.EventTypeMeta:
                    break;
                default:
                    Console.WriteLine($"不支持的事件类型 {postType}");
                    break;
            }
        }

        protected virtual void OnPrivateMessageEvent(PrivateMessageEventArgs args) { }

        protected virtual void OnGroupMessageEvent(GroupMessageEventArgs args) { }
    }
}