using System.Text.RegularExpressions;
using DoDo.Open.Sdk.Models;
using DoDo.Open.Sdk.Models.Channels;
using DoDo.Open.Sdk.Models.Events;
using DoDo.Open.Sdk.Models.Messages;
using DoDo.Open.Sdk.Services;

namespace DoDo.Open.Sign
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

            #region 签到初始化

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

            #region 签到

            if (eventBody.MessageBody is MessageBodyText messageBodyText)
            {
                var content = messageBodyText.Content.Replace(" ", "");
                var defaultReply = $"<@!{eventBody.DodoId}>";
                var reply = defaultReply;

                var dataPath = $"{Environment.CurrentDirectory}\\data\\{eventBody.IslandId}.txt";

                if (content == (_appSetting.Sign.Command))//签到
                {
                    var signTime = DataHelper.GetValue<string>(dataPath, eventBody.DodoId, "SignTime");
                    if (signTime == "" || Convert.ToDateTime(signTime).Date != DateTime.Now.Date)
                    {
                        var integral = DataHelper.GetValue<long>(dataPath, eventBody.DodoId, "Integral");
                        var signCount = DataHelper.GetValue<long>(dataPath, eventBody.DodoId, "SignCount");

                        integral += _appSetting.Sign.GetIntegral;
                        signCount++;
                        signTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        DataHelper.SetValue(dataPath, eventBody.DodoId, "NickName", eventBody.Member.NickName);
                        DataHelper.SetValue(dataPath, eventBody.DodoId, "Integral", integral);
                        DataHelper.SetValue(dataPath, eventBody.DodoId, "SignCount", signCount);
                        DataHelper.SetValue(dataPath, eventBody.DodoId, "SignTime", signTime);

                        reply = _appSetting.Sign.Reply
                            .Replace("{DoDoId}", eventBody.DodoId)
                            .Replace("{IntegralName}", _appSetting.Sign.IntegralName)
                            .Replace("{GetIntegral}", $"{_appSetting.Sign.GetIntegral}")
                            .Replace("{Integral}", $"{integral}")
                            .Replace("{SignCount}", $"{signCount}")
                            .Replace("{SignTime}", $"{signTime}");

                    }
                    else
                    {
                        reply += "\n**签到失败**";
                        reply += "\n您今天已经签到过了！";
                    }
                }
                else if (content == (_appSetting.Query.Command))//查询
                {
                    var signTime = DataHelper.GetValue<string>(dataPath, eventBody.DodoId, "SignTime");
                    if (signTime != "")
                    {
                        var integral = DataHelper.GetValue<long>(dataPath, eventBody.DodoId, "Integral");
                        var signCount = DataHelper.GetValue<long>(dataPath, eventBody.DodoId, "SignCount");

                        reply = _appSetting.Query.Reply
                            .Replace("{DoDoId}", eventBody.DodoId)
                            .Replace("{NickName}", eventBody.Member.NickName)
                            .Replace("{IntegralName}", _appSetting.Sign.IntegralName)
                            .Replace("{Integral}", $"{integral}")
                            .Replace("{SignCount}", $"{signCount}")
                            .Replace("{SignTime}", $"{signTime}");
                    }
                    else
                    {
                        reply += "\n**查询失败**";
                        reply += "\n您的账户为空，请先签到开户吧！";
                    }
                }
                else if (content.StartsWith(_appSetting.Transfer.Command))//转账
                {
                    var matchResult = Regex.Match(content, $"{_appSetting.Transfer.Command}<@!(\\d+)>(\\d+)");

                    var targetDoDoId = "";
                    long transferIntegral = 0;

                    if (matchResult.Groups.Count > 1)
                    {
                        targetDoDoId = matchResult.Groups[1].Value;
                    }

                    if (matchResult.Groups.Count > 2)
                    {
                        long.TryParse(matchResult.Groups[2].Value, out transferIntegral);
                    }

                    if (!string.IsNullOrEmpty(targetDoDoId) && transferIntegral > 0)
                    {
                        var signTime = DataHelper.GetValue<string>(dataPath, eventBody.DodoId, "SignTime");

                        if (signTime != "")
                        {
                            var integral = DataHelper.GetValue<long>(dataPath, eventBody.DodoId, "Integral");
                            if (integral >= transferIntegral)
                            {
                                if (eventBody.DodoId != targetDoDoId)
                                {
                                    var targetSignTime = DataHelper.GetValue<string>(dataPath, targetDoDoId, "SignTime");
                                    if (targetSignTime != "")
                                    {
                                        var targetIntegral = DataHelper.GetValue<long>(dataPath, eventBody.DodoId, "Integral");

                                        integral -= transferIntegral;
                                        DataHelper.SetValue(dataPath, eventBody.DodoId, "Integral", integral);
                                        targetIntegral += transferIntegral;
                                        DataHelper.SetValue(dataPath, targetDoDoId, "Integral", targetIntegral);

                                        reply = _appSetting.Transfer.Reply
                                            .Replace("{DoDoId}", eventBody.DodoId)
                                            .Replace("{NickName}", eventBody.Member.NickName)
                                            .Replace("{TargetDoDoId}", $"{targetDoDoId}")
                                            .Replace("{TransferIntegral}", $"{transferIntegral}")
                                            .Replace("{IntegralName}", _appSetting.Sign.IntegralName);
                                    }
                                    else
                                    {
                                        reply += "\n**转账失败**";
                                        reply += "\n未查询到对方的账户信息，请先邀请对方签到开户吧！";
                                    }
                                }
                                else
                                {
                                    reply += "\n**转账失败**";
                                    reply += "\n转账对象不能是自己！";
                                }
                            }
                            else
                            {
                                reply += "\n**转账失败**";
                                reply += $"\n您的账户余额不足{transferIntegral}{_appSetting.Sign.IntegralName}！";
                            }
                        }
                        else
                        {
                            reply += "\n**转账失败**";
                            reply += "\n未查询到您的账户信息，请先签到！";
                        }
                    }
                    else
                    {
                        reply += "\n**转账失败**";
                        reply += "\n您发送的指令格式有误！";
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
    }
}
