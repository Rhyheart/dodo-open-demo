using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using DoDo.Open.Sdk.Models;
using DoDo.Open.Sdk.Models.Channels;
using DoDo.Open.Sdk.Models.Events;
using DoDo.Open.Sdk.Models.Messages;
using DoDo.Open.Sdk.Services;
using RestSharp;

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

            #region 初始化

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
            try
            {
                var eventBody = input.Data.EventBody;

                if (eventBody.MessageBody is MessageBodyText messageBodyText)
                {
                    var content = messageBodyText.Content.Replace(" ", "");
                    var reply = "";

                    #region ChatGPT

                    var dataPath = $"{Environment.CurrentDirectory}\\data\\{eventBody.DodoSourceId}.txt";

                    var sectionList = DataHelper.ReadSections(dataPath)
                        .Select(x => Convert.ToInt64(x))
                        .OrderBy(x => x)
                        .Select(x => Convert.ToString(x))
                        .ToList();

                    var messageBuilder = new StringBuilder();

                    for (var i = 0; i < sectionList.Count; i++)
                    {
                        var section = sectionList[i];
                        var getKeyWord = DataHelper.ReadValue<string>(dataPath, section, "KeyWord");
                        var getReply = DataHelper.ReadValue<string>(dataPath, section, "Reply").Replace("\\n", "\n");

                        messageBuilder.Append($"\n{eventBody.DodoSourceId}:{getKeyWord}");
                        messageBuilder.Append($"\nAi:{getReply}");

                        if (i >= 10)
                        {
                            DataHelper.DeleteSection(dataPath, sectionList[i - 10]);
                        }

                    }

                    var setKeyWord = content;

                    messageBuilder.Append($"\n{eventBody.DodoSourceId}:{setKeyWord}");

                    var client = new RestClient();

                    var request = new RestRequest("https://api.openai.com/v1/completions");

                    request.AddHeader("Authorization", $"Bearer {_appSetting.ChatGPTConfig.Token}");

                    var jsonSerializerOptions = new JsonSerializerOptions
                    {
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    var json = new
                    {
                        model = _appSetting.ChatGPTConfig.Model,
                        prompt = $"{messageBuilder}",
                        temperature = 0.9,
                        max_tokens = _appSetting.ChatGPTConfig.MaxTokens,
                        top_p = 1,
                        frequency_penalty = 0,
                        presence_penalty = 0.6,
                        stop = new[] {$"{eventBody.DodoSourceId}:"}
                    };

                    request.AddJsonBody(json);

                    _openApiOptions.Log?.Invoke($"ChatGPT-Request: {JsonSerializer.Serialize(json, jsonSerializerOptions)}");

                    var setReply = "";
                    try
                    {
                        var response = client.Post<ChatGPTOutput>(request);

                        _openApiOptions.Log?.Invoke($"ChatGPT-Response: {response.Content}");

                        setReply = response.Data.Choices[0].Text.Replace("Ai:", "");
                    }
                    catch (Exception e)
                    {
                        setReply = "让我休息会吧~";
                    }

                    if(string.IsNullOrWhiteSpace(setReply))
                    {
                        setReply = "让我休息会吧~";
                    }

                    reply += setReply;

                    var setSection = DateTime.Now.ToString("yyyyMMddhhmmss");

                    DataHelper.WriteValue(dataPath, setSection, "KeyWord", setKeyWord);
                    DataHelper.WriteValue(dataPath, setSection, "Reply", setReply.Replace("\n", ""));

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
