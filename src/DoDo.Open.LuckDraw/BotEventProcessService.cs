using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using DoDo.Open.Sdk.Models;
using DoDo.Open.Sdk.Models.Channels;
using DoDo.Open.Sdk.Models.Events;
using DoDo.Open.Sdk.Models.Messages;
using DoDo.Open.Sdk.Services;
using Microsoft.Extensions.FileSystemGlobbing;
using Quartz.Util;
using static System.Net.Mime.MediaTypeNames;

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

                if (Regex.IsMatch(content, "发起抽奖"))
                {
                    var card = new Card
                    {
                        Type = "card",
                        Title = "发起抽奖",
                        Theme = "default",
                        Components = new List<object>()
                    };

                    card.Components.Add(new
                    {
                        type = "remark",
                        elements = new List<object>
                        {
                            new
                            {
                                type = "image",
                                src = eventBody.Personal.AvatarUrl
                            },
                            new
                            {
                                type = "dodo-md",
                                content = eventBody.Member.NickName
                            }
                        }
                    });

                    card.Components.Add(new
                    {
                        type = "section",
                        text = new
                        {
                            type = "dodo-md",
                            content = $"[{eventBody.DodoId}][{eventBody.Member.NickName}]发起的抽奖。"
                        }
                    });

                    card.Components.Add(new
                    {
                        type = "button-group",
                        elements = new List<object>
                        {
                            new
                            {
                                type = "button",
                                interactCustomId = "交互自定义id4",
                                click = new
                                {
                                    action = "form",
                                    value = ""
                                },
                                color = "grey",
                                name = "填写抽奖内容发起抽奖",
                                form = new
                                {
                                    title = "表单标题",
                                    elements = new List<object>
                                    {
                                        new
                                        {
                                            type = "input",
                                            key = "选项自定义id2",
                                            title = "填写抽奖时间，填1时为1分钟。",
                                            rows = 1,
                                            placeholder = "请输入阿拉伯数字，1000字符限制",
                                            minChar = 0,
                                            maxChar = 1000
                                        },
                                        new
                                        {
                                            type = "input",
                                            key = "选项自定义id1",
                                            title = "填写抽奖的标题和内容",
                                            rows = 4,
                                            placeholder = "4000字符限制",
                                            minChar = 1,
                                            maxChar = 4000
                                        }
                                    }
                                }
                            }
                        }
                    });

                    card.Components.Add(new
                    {
                        type = "countdown",
                        title = "发起抽奖时，倒计时10分钟结束后失效",
                        style = "hour",
                        endTime = DateTime.Now.AddMinutes(10).GetTimeStamp()
                    });

                    await _openApiService.SetChannelMessageSendAsync(new SetChannelMessageSendInput<MessageBodyCard>
                    {
                        ChannelId = eventBody.ChannelId,
                        MessageBody = new MessageBodyCard
                        {
                            Card = card
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
