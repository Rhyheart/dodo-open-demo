using System.Text.RegularExpressions;
using DoDo.Open.Sdk.Models;
using DoDo.Open.Sdk.Models.Channels;
using DoDo.Open.Sdk.Models.Events;
using DoDo.Open.Sdk.Models.Members;
using DoDo.Open.Sdk.Models.Messages;
using DoDo.Open.Sdk.Services;

namespace DoDo.Open.ProhibitWordManage
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
                    var defaultReply = $"<@!{eventBody.DodoSourceId}>";
                    var reply = defaultReply;

                    #region 违禁词管理

                    //获取匹配到的规则列表，并按照禁言时间倒序
                    var matchRuleList = _appSetting.RuleList
                        .Where(x => Regex.IsMatch(content, x.KeyWord))
                        .OrderByDescending(x => x.MuteDuration)
                        .ToList();

                    if (matchRuleList.Count > 0)
                    {
                        reply += "\n触发违禁词";

                        //取规则列表第一条数据，按规则进行违禁词管理操作
                        var matchRule = matchRuleList[0];

                        if (matchRule.IsWithdraw == 1)
                        {
                            try //若调用接口成功，则提示，若失败，则不提示
                            {
                                await _openApiService.SetChannelMessageWithdrawAsync(new SetChannelMessageWithdrawInput
                                {
                                    MessageId = eventBody.MessageId,
                                    Reason = "触发违禁词"
                                }, true);

                                reply += "，撤回违禁消息";
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                        if (matchRule.MuteDuration > 0)
                        {
                            try
                            {
                                await _openApiService.SetMemberMuteAddAsync(new SetMemberMuteAddInput
                                {
                                    IslandSourceId = eventBody.IslandSourceId,
                                    DodoSourceId = eventBody.DodoSourceId,
                                    Duration = matchRule.MuteDuration * 60,
                                    Reason = "触发违禁词"
                                }, true);

                                reply += $"，禁言{matchRule.MuteDuration}分钟";
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
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
