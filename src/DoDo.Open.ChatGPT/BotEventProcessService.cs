using DoDo.Open.Sdk.Models;
using DoDo.Open.Sdk.Models.Channels;
using DoDo.Open.Sdk.Models.Events;
using DoDo.Open.Sdk.Models.Messages;
using DoDo.Open.Sdk.Services;

namespace DoDo.Open.ChatGPT
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
            try
            {
                var eventBody = input.Data.EventBody;

                if (eventBody.MessageBody is MessageBodyText messageBodyText)
                {
                    var content = messageBodyText.Content.Replace(" ", "");
                    var reply = "";

                    #region 关键词回复

                    reply = _appSetting.ChatGPTConfig.Model;


                    #endregion

                    if (reply != "")
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
            catch (Exception e)
            {
                Exception(e.Message);
            }
        }
    }
}
