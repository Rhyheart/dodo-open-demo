using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using DoDo.Open.Sdk.Models;
using DoDo.Open.Sdk.Models.Channels;
using DoDo.Open.Sdk.Models.Events;
using DoDo.Open.Sdk.Models.Messages;
using DoDo.Open.Sdk.Services;

namespace DoDo.Open.LuckDraw
{
    public class BotEventProcessService : EventProcessService
    {
        private readonly OpenApiService _openApiService;
        private readonly OpenApiOptions _openApiOptions;
        private readonly AppSetting _appSetting;

        public BotEventProcessService(OpenApiService openApiService, AppSetting appSetting)
        {
            _openApiService = openApiService;
            _openApiOptions = openApiService.GetBotOptions();
            _appSetting = appSetting;
        }

        public override void Connected(string message)
        {
            _openApiOptions.Log?.Invoke($"Connected: {message}");

            #region 抽奖初始化

            if (!Directory.Exists($"{Environment.CurrentDirectory}\\data"))
            {
                Directory.CreateDirectory($"{Environment.CurrentDirectory}\\data");
            }

            #endregion

        }

        public override void Disconnected(string message)
        {
            _openApiOptions.Log?.Invoke($"Disconnected: {message}");
        }

        public override void Reconnected(string message)
        {
            _openApiOptions.Log?.Invoke($"Reconnected: {message}");
        }

        public override void Exception(string message)
        {
            _openApiOptions.Log?.Invoke($"Exception: {message}");
        }

        public override void Received(string message)
        {
            _openApiOptions.Log?.Invoke($"Received: {message}");
        }

        public override async void ChannelMessageEvent<T>(EventSubjectOutput<EventSubjectDataBusiness<EventBodyChannelMessage<T>>> input)
        {
            var eventBody = input.Data.EventBody;

            if (eventBody.MessageBody is MessageBodyText messageBodyText)
            {
                var content = messageBodyText.Content.Replace(" ", "");
                var defaultReply = $"<@!{eventBody.DodoId}>";
                var reply = defaultReply;

                var dataPath = $"{Environment.CurrentDirectory}\\data\\{eventBody.IslandId}.txt";

                #region 抽奖

                if (Regex.IsMatch(content, _appSetting.LuckDraw.Command))
                {
                    var components = new List<object>();

                    components.Add(new
                    {
                        type = "remark",
                        elements = new List<object>
                        {
                            new
                            {
                                type = "image",
                                src = "https://img.imdodo.com/upload/cdn/4803E0BBF8678A657EBD762D7AC45710_1660189645624.png"
                            }
                        }
                    });

                    await _openApiService.SetChannelMessageSendAsync(new SetChannelMessageSendInput<MessageBodyCard>
                    {
                        ChannelId = eventBody.ChannelId,
                        MessageBody = new MessageBodyCard
                        {
                            Card = new Card
                            {
                                Type = "card",
                                Title = "发起抽奖",
                                Theme = "default",
                                Components = components
                            }
                        }
                    });
                }

                #endregion

                if (reply != defaultReply)
                {
                    await _openApiService.SetChannelMessageSendAsync(new SetChannelMessageSendInput<MessageBodyText>
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
