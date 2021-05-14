using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace KSGFK
{
    public static class OneBotHttpApi
    {
        public const string ApiSendPrivateMsg = "send_private_msg";
        public const string ApiSendGroupMsg = "send_group_msg";
        public const string UserId = "user_id";
        public const string Message = "message";
        public const string GroupId = "group_id";

        private static Uri _serverUri;

        public static void SetServerUri(Uri uri) { _serverUri = uri; }

        public static HttpWebRequest CreateRequest(string api)
        {
            if (!Uri.TryCreate(_serverUri, api, out var apiUri))
            {
                throw new ArgumentException("无效地址");
            }
            var request = (HttpWebRequest) WebRequest.Create(apiUri);
            request.Method = "POST";
            request.ContentType = "application/json";
            return request;
        }

        public static async Task<JsonDocument> GetResponseAsync(HttpWebRequest request)
        {
            var response = request.GetResponse();
            JsonDocument result = null;
            await using (var responseStream = response.GetResponseStream())
            {
                if (responseStream != null)
                {
                    result = await JsonDocument.ParseAsync(responseStream);
                }
            }
            request.Abort();
            return result;
        }

        public static async Task<JsonDocument> SendPrivateMsgAsync(long userId, IReadOnlyList<OneBotMessageSegment> msg)
        {
            var request = CreateRequest(ApiSendPrivateMsg);
            await using (var postDataStream = request.GetRequestStream())
            {
                await using var writer = new Utf8JsonWriter(postDataStream);
                writer.WriteStartObject();
                writer.WriteNumber(UserId, userId);
                writer.WritePropertyName(Message);
                OneBotMessageSegment.WriteArray(writer, msg);
                writer.WriteEndObject();
            }
            return await GetResponseAsync(request);
        }

        public static async Task<JsonDocument> SendGroupMsgAsync(long groupId, IReadOnlyList<OneBotMessageSegment> msg)
        {
            var request = CreateRequest(ApiSendGroupMsg);
            await using (var postDataStream = request.GetRequestStream())
            {
                await using var writer = new Utf8JsonWriter(postDataStream);
                writer.WriteStartObject();
                writer.WriteNumber(GroupId, groupId);
                writer.WritePropertyName(Message);
                OneBotMessageSegment.WriteArray(writer, msg);
                writer.WriteEndObject();
            }
            return await GetResponseAsync(request);
        }
    }
}