using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using DoDo.Open.Sdk.Models;
using DoDo.Open.Sdk.Models.Bots;
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
        private string? botId;

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

                    if (string.IsNullOrWhiteSpace(botId))
                    {
                        botId = (await _openApiService.GetBotInfoAsync(new GetBotInfoInput()))?.DodoSourceId;
                    }

                    if (Regex.IsMatch(content, @".*(<@!\d+>).*"))
                    {
                        content = Regex.Replace(content, @"<@!\d+>", "");
                    }
                    else
                    {
                        return;
                    }

                    /*if (content.Contains($"<@!{botId}>"))
                    {
                        content = content.Replace($"<@!{botId}>", "");
                    }
                    else
                    {
                        return;
                    }*/

                    var dataPath = $"{Environment.CurrentDirectory}\\data\\{eventBody.DodoSourceId}.txt";

                    var sectionList = DataHelper.ReadSections(dataPath)
                        .Select(x => Convert.ToInt64(x))
                        .OrderByDescending(x => x)
                        .Select(x => Convert.ToString(x))
                        .ToList();

                    var setKeyWord = content;

                    var maxTokens = 1500;

                    var messageBuilder = new StringBuilder();

                    messageBuilder.Append($"\n{eventBody.DodoSourceId}:{setKeyWord}");

                    for (var i = 0; i < sectionList.Count; i++)
                    {
                        var section = sectionList[i];
                        var getKeyWord = DataHelper.ReadValue<string>(dataPath, section, "KeyWord");
                        var getReply = DataHelper.ReadValue<string>(dataPath, section, "Reply").Replace("\\n", "\n");

                        if (getReply.Length > 500)
                        {
                            getReply = getReply.Substring(0, 500);
                        }

                        var tempMessage = $"\n{eventBody.DodoSourceId}:{getKeyWord}\nAi:{getReply}";

                        if (messageBuilder.Length + tempMessage.Length < 4000 - maxTokens)
                        {
                            messageBuilder.Insert(0, tempMessage);
                        }

                        if (i >= 20)
                        {
                            DataHelper.DeleteSection(dataPath, sectionList[i]);
                        }

                    }

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
                        max_tokens = maxTokens,
                        top_p = 1,
                        frequency_penalty = 0,
                        presence_penalty = 0.6
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
                        // ignored
                    }

                    if (!string.IsNullOrWhiteSpace(setReply))
                    {
                        reply += setReply;

                        var setSection = DateTime.Now.ToString("yyyyMMddhhmmss");

                        DataHelper.WriteValue(dataPath, setSection, "KeyWord", setKeyWord);
                        DataHelper.WriteValue(dataPath, setSection, "Reply", setReply.Replace("\n", ""));
                    }
                    else
                    {
                        reply += "让我休息会吧~";
                    }

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
