using System.Text.RegularExpressions;
using DoDo.Open.Sdk.Models;
using DoDo.Open.Sdk.Models.ChannelMessages;
using DoDo.Open.Sdk.Models.Events;
using DoDo.Open.Sdk.Models.Messages;
using DoDo.Open.Sdk.Services;

namespace DoDo.Open.Solitaire
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

            #region 接龙初始化

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
                    var defaultReply = $"<@!{eventBody.DodoSourceId}>";
                    var reply = defaultReply;

                    var dataPath = $"{Environment.CurrentDirectory}\\data\\{eventBody.IslandSourceId}.txt";

                    #region 接龙

                    if (Regex.IsMatch(content, _appSetting.StartSolitaire.Command))//发起接龙
                    {
                        var matchResult = Regex.Match(content, _appSetting.StartSolitaire.Command);
                        var solitaireContent = matchResult.Groups[1].Value;
                        if (!string.IsNullOrWhiteSpace(solitaireContent))
                        {
                            var solitaireReply = $"<@!{eventBody.DodoSourceId}>";
                            var showReply = $"1. {solitaireReply}";
                            var output = await _openApiService.SetChannelMessageSendAsync(new SetChannelMessageSendInput<MessageBodyText>()
                            {
                                ChannelId = eventBody.ChannelId,
                                MessageBody = new MessageBodyText
                                {
                                    Content = $">#接龙 {solitaireContent}\n\n{showReply}"
                                }
                            });

                            if (!string.IsNullOrWhiteSpace(output.MessageId))
                            {
                                DataHelper.WriteValue(dataPath, output.MessageId, "IslandId", eventBody.IslandSourceId);
                                DataHelper.WriteValue(dataPath, output.MessageId, "DoDoId", eventBody.DodoSourceId);
                                DataHelper.WriteValue(dataPath, output.MessageId, "Content", solitaireContent.Replace("\\n", "\n"));
                                DataHelper.WriteValue(dataPath, output.MessageId, "Reply", solitaireReply.Replace("\\n", "\n"));
                                DataHelper.WriteValue(dataPath, output.MessageId, "CreateTime", DateTime.Now);
                            }
                        }
                        else
                        {
                            reply += "\n**发起接龙失败**";
                            reply += "\n您发送的指令格式有误！";
                        }
                    }
                    else if (Regex.IsMatch(content, _appSetting.JoinSolitaire.Command))//加入接龙
                    {
                        var matchResult = Regex.Match(content, _appSetting.JoinSolitaire.Command);
                        var solitaireReply = matchResult.Groups[2].Value;
                        if (!string.IsNullOrWhiteSpace(eventBody.Reference.MessageId))
                        {
                            var oldEntityMessageId = eventBody.Reference.MessageId;
                            var oldEntityIslandId = DataHelper.ReadValue<string>(dataPath, oldEntityMessageId, "IslandId");
                            var oldEntityDoDoId = DataHelper.ReadValue<string>(dataPath, oldEntityMessageId, "DoDoId");
                            var oldEntityContent = DataHelper.ReadValue<string>(dataPath, oldEntityMessageId, "Content").Replace("\\n", "\n");
                            var oldEntityReply = DataHelper.ReadValue<string>(dataPath, oldEntityMessageId, "Reply").Replace("\\n", "\n");
                            var oldEntityCreateTime = DataHelper.ReadValue<DateTime>(dataPath, oldEntityMessageId, "CreateTime");

                            if (oldEntityIslandId != "")
                            {
                                if (oldEntityCreateTime.AddHours(12) > DateTime.Now)
                                {
                                    if (!oldEntityReply.Contains($"<@!{eventBody.DodoSourceId}>"))
                                    {
                                        solitaireReply = solitaireReply.Replace("\n", "");

                                        if (!string.IsNullOrWhiteSpace(oldEntityReply))
                                        {
                                            var list = oldEntityReply.Split("\n").ToList();
                                            list.Add($"<@!{eventBody.DodoSourceId}> {solitaireReply}");

                                            oldEntityReply = string.Join("\n", list);

                                            var showReply = "";
                                            for (var i = 0; i < list.Count; i++)
                                            {
                                                showReply += $"{i + 1}. {list[i]}\n";
                                            }

                                            var output = await _openApiService.SetChannelMessageSendAsync(new SetChannelMessageSendInput<MessageBodyText>()
                                            {
                                                ChannelId = eventBody.ChannelId,
                                                MessageBody = new MessageBodyText
                                                {
                                                    Content = $">#接龙 {oldEntityContent}\n\n{showReply}"
                                                }
                                            });

                                            if (!string.IsNullOrWhiteSpace(output.MessageId))
                                            {
                                                await _openApiService.SetChannelMessageWithdrawAsync(new SetChannelMessageWithdrawInput
                                                {
                                                    MessageId = oldEntityMessageId,
                                                    Reason = "接龙撤回"
                                                });

                                                //删除原配置
                                                DataHelper.DeleteSection(dataPath, oldEntityMessageId);

                                                //新增新配置
                                                DataHelper.WriteValue(dataPath, output.MessageId, "IslandId", oldEntityIslandId);
                                                DataHelper.WriteValue(dataPath, output.MessageId, "DoDoId", oldEntityDoDoId);
                                                DataHelper.WriteValue(dataPath, output.MessageId, "Content", oldEntityContent.Replace("\n", "\\n"));
                                                DataHelper.WriteValue(dataPath, output.MessageId, "Reply", oldEntityReply.Replace("\n", "\\n"));
                                                DataHelper.WriteValue(dataPath, output.MessageId, "CreateTime", oldEntityCreateTime);
                                            }
                                        }
                                        else
                                        {
                                            reply += "\n**加入接龙失败**";
                                            reply += "\n您要加入的接龙消息已失效！";
                                        }
                                    }
                                    else
                                    {
                                        reply += "\n**加入接龙失败**";
                                        reply += "\n您已加入过该条接龙！";
                                    }
                                }
                                else
                                {
                                    reply += "\n**加入接龙失败**";
                                    reply += "\n您要加入的接龙消息已失效！";
                                }
                            }
                            else
                            {
                                reply += "\n**加入接龙失败**";
                                reply += "\n您回复的消息并非接龙消息！";
                            }
                        }
                        else
                        {
                            reply += "\n**加入接龙失败**";
                            reply += "\n请回复您要加入的接龙消息！";
                        }
                    }
                    else if (Regex.IsMatch(content, _appSetting.LeaveSolitaire.Command))//退出接龙
                    {
                        if (!string.IsNullOrWhiteSpace(eventBody.Reference.MessageId))
                        {
                            var oldEntityMessageId = eventBody.Reference.MessageId;
                            var oldEntityIslandId = DataHelper.ReadValue<string>(dataPath, oldEntityMessageId, "IslandId");
                            var oldEntityDoDoId = DataHelper.ReadValue<string>(dataPath, oldEntityMessageId, "DoDoId");
                            var oldEntityContent = DataHelper.ReadValue<string>(dataPath, oldEntityMessageId, "Content").Replace("\\n", "\n");
                            var oldEntityReply = DataHelper.ReadValue<string>(dataPath, oldEntityMessageId, "Reply").Replace("\\n", "\n");
                            var oldEntityCreateTime = DataHelper.ReadValue<DateTime>(dataPath, oldEntityMessageId, "CreateTime");

                            if (oldEntityIslandId != "")
                            {
                                if (oldEntityCreateTime.AddHours(12) > DateTime.Now)
                                {
                                    if (oldEntityReply.Contains($"<@!{eventBody.DodoSourceId}>"))
                                    {
                                        var list = oldEntityReply.Split("\n")
                                            .ToList()
                                            .Where(x => !x.StartsWith($"<@!{eventBody.DodoSourceId}>"))
                                            .ToList();

                                        oldEntityReply = string.Join("\n", list);

                                        var showReply = "";

                                        if (list.Count > 0)
                                        {
                                            for (var i = 0; i < list.Count; i++)
                                            {
                                                showReply += $"{i + 1}. {list[i]}\n";
                                            }
                                        }
                                        else
                                        {
                                            showReply = "该条接龙已失效！";
                                        }

                                        var output = await _openApiService.SetChannelMessageSendAsync(new SetChannelMessageSendInput<MessageBodyText>()
                                        {
                                            ChannelId = eventBody.ChannelId,
                                            MessageBody = new MessageBodyText
                                            {
                                                Content = $">#接龙 {oldEntityContent}\n\n{showReply}"
                                            }
                                        });

                                        if (!string.IsNullOrWhiteSpace(output.MessageId))
                                        {
                                            await _openApiService.SetChannelMessageWithdrawAsync(new SetChannelMessageWithdrawInput
                                            {
                                                MessageId = oldEntityMessageId,
                                                Reason = "接龙撤回"
                                            });

                                            //删除原配置
                                            DataHelper.DeleteSection(dataPath, oldEntityMessageId);

                                            if (list.Count > 0)
                                            {
                                                //新增新配置
                                                DataHelper.WriteValue(dataPath, output.MessageId, "IslandId", oldEntityIslandId);
                                                DataHelper.WriteValue(dataPath, output.MessageId, "DoDoId", oldEntityDoDoId);
                                                DataHelper.WriteValue(dataPath, output.MessageId, "Content", oldEntityContent);
                                                DataHelper.WriteValue(dataPath, output.MessageId, "Reply", oldEntityReply);
                                                DataHelper.WriteValue(dataPath, output.MessageId, "CreateTime", oldEntityCreateTime);
                                            }
                                        }

                                    }
                                    else
                                    {
                                        reply += "\n**退出接龙失败**";
                                        reply += "\n您尚未加入该条接龙！";
                                    }
                                }
                                else
                                {
                                    reply += "\n**退出接龙失败**";
                                    reply += "\n您要退出的接龙消息已失效！";
                                }
                            }
                            else
                            {
                                reply += "\n**退出接龙失败**";
                                reply += "\n您回复的消息并非接龙消息！";
                            }
                        }
                        else
                        {
                            reply += "\n**退出接龙失败**";
                            reply += "\n请回复您要退出的接龙消息！";
                        }
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
            catch (Exception e)
            {
                Exception(e.Message);
            }
        }
    }
}
