using DoDo.Open.Sdk.Models.Channels;
using DoDo.Open.Sdk.Models.Events;
using DoDo.Open.Sdk.Models.Messages;
using DoDo.Open.Sdk.Services;
using Newtonsoft.Json;

namespace DoDo.Open.Sign
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

            if (eventBody.MessageBody is MessageBodyText messageBodyText)
            {
                var content = messageBodyText.Content.Replace(" ", "");
                var reply = "";

                var dataPath = $"{Environment.CurrentDirectory}\\data\\{eventBody.IslandId}.txt";

                if (content == (_appSetting.Sign.Command))//签到
                {
                    var integral = DataHelper.GetValue<long>(dataPath, eventBody.DodoId, "Integral");
                    Console.WriteLine(integral);
                    integral++;
                    DataHelper.SetValue(dataPath, eventBody.DodoId, "Integral", integral);
                    reply = $"{integral}";
                }
                else if (content == (_appSetting.Query.Command))//查询
                {

                }
                else if (content.StartsWith(_appSetting.Transfer.Command))//转账
                {

                }

                if (reply != "")
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
