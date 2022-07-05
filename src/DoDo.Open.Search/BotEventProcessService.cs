using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using DoDo.Open.Sdk.Models.Channels;
using DoDo.Open.Sdk.Models.Events;
using DoDo.Open.Sdk.Models.Messages;
using DoDo.Open.Sdk.Services;

namespace DoDo.Open.Search
{
    public class BotEventProcessService : EventProcessService
    {
        private readonly OpenApiService _openApiService;
        private readonly AppSetting _appSetting;

        public BotEventProcessService(OpenApiService openApiService, AppSetting appSetting)
        {
            _openApiService = openApiService;
            _appSetting = appSetting;
        }

        public override void Connected(string message)
        {
            Console.WriteLine($"\n{message}\n");
        }

        public override void Disconnected(string message)
        {
            Console.WriteLine(message);
        }

        public override void Reconnected(string message)
        {
            Console.WriteLine(message);
        }

        public override void Exception(string message)
        {
            Console.WriteLine(message);
        }

        public override void ChannelMessageEvent<T>(EventSubjectOutput<EventSubjectDataBusiness<EventBodyChannelMessage<T>>> input)
        {
            var eventBody = input.Data.EventBody;

            #region 检索

            if (eventBody.MessageBody is MessageBodyText messageBodyText)
            {
                var content = messageBodyText.Content.Replace(" ", "");
                var defaultReply = $"<@!{eventBody.DodoId}>";
                var reply = defaultReply;

                var rule = _appSetting.RuleList.FirstOrDefault(x => Regex.IsMatch(content, $"{x.Command}(.*)"));
                if (rule != null)
                {
                    var matchResult = Regex.Match(content, $"{rule.Command}(.*)");
                    reply = rule.Reply
                        .Replace("{DoDoId}",eventBody.DodoId)
                        .Replace("{KeyWord}", UrlEncoder.Default.Encode(matchResult.Groups[1].Value));
                }

                #endregion

                if (reply != defaultReply)
                {
                    _openApiService.SetChannelMessageSend(new SetChannelMessageSendInput<MessageBodyText>
                    {
                        ChannelId = eventBody.ChannelId,
                        MessageBody = new MessageBodyText
                        {
                            Content = reply
                        }
                    });
                }

            }

        }
    }
}
