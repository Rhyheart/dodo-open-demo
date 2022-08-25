using System.Text.Json;
using DoDo.Open.Sdk.Models;
using DoDo.Open.Sdk.Models.Channels;
using DoDo.Open.Sdk.Models.Events;
using DoDo.Open.Sdk.Models.Messages;
using DoDo.Open.Sdk.Models.Roles;
using DoDo.Open.Sdk.Services;

namespace DoDo.Open.RoleReceive
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

            #region 身份组领取初始化

            var settingFilePath = Environment.CurrentDirectory + "/appsettings.json";
            var setting = File.ReadAllText(settingFilePath);

            var dataFilePath = Environment.CurrentDirectory + "/appdatas.json";
            var data = "";
            if (File.Exists(dataFilePath))
            {
                data = File.ReadAllText(dataFilePath);
            }

            //校验是否发送过身份组领取消息
            if (!string.IsNullOrWhiteSpace(data))
            {
                //校验配置是否更新
                var appData = JsonSerializer.Deserialize<AppData>(data);
                if (appData?.Setting == setting)
                {
                    //发送过消息 且 配置未更新，则直接使用原消息ID
                    _appSetting.MessageId = appData.MessageId;
                }
            }

            //未取到 或 未使用 原消息ID，则需要重新发送身份组领取消息
            if (string.IsNullOrWhiteSpace(_appSetting.MessageId))
            {
                var roleList = _openApiService.GetRoleList(new GetRoleListInput
                {
                    IslandId = _appSetting.IslandId
                }, true);

                var content = $"{_appSetting.Message}\n\n";

                foreach (var item in _appSetting.RuleList)
                {
                    content += $"选择{item.Emoji}，获得身份组 [ {roleList.FirstOrDefault(x => x.RoleId == item.RoleId)?.RoleName} ]\n";
                }

                //发送身份组领取消息
                var setChannelMessageSendOutput = _openApiService.SetChannelMessageSend(new SetChannelMessageSendInput<MessageBodyText>
                {
                    ChannelId = _appSetting.ChannelId,
                    MessageBody = new MessageBodyText
                    {
                        Content = content
                    }
                }, true);

                //为身份组领取消息添加对应表情反应
                foreach (var item in _appSetting.RuleList)
                {
                    _openApiService.SetChannelMessageReactionAdd(new SetChannelMessageReactionAddInput
                    {
                        MessageId = setChannelMessageSendOutput.MessageId,
                        Emoji = new MessageModelEmoji
                        {
                            Type = 1,
                            Id = $"{char.ConvertToUtf32(item.Emoji, 0)}"
                        }
                    });
                }

                //若身份组领取消息发送成功，则记录并存储当前消息ID
                _appSetting.MessageId = setChannelMessageSendOutput.MessageId;
                using var writer = new StreamWriter(dataFilePath, false);
                writer.Write(JsonSerializer.Serialize(new AppData
                {
                    MessageId = _appSetting.MessageId,
                    Setting = setting
                }));
            }

            if (!string.IsNullOrWhiteSpace(_appSetting.MessageId))
            {
                Console.WriteLine($"初始化成功，获取到消息ID为：{_appSetting.MessageId}\n");
            }
            else
            {
                Console.WriteLine("初始化失败，未获取到消息ID\n");
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

        public override async void MessageReactionEvent(EventSubjectOutput<EventSubjectDataBusiness<EventBodyMessageReaction>> input)
        {
            try
            {
                var eventBody = input.Data.EventBody;

                #region 身份组领取

                //校验当前反应的消息是否为配置的身份组领取消息
                if (_appSetting.MessageId == eventBody.ReactionTarget.Id)
                {
                    //筛选当前反应的表情对应的规则列表
                    var ruleList = _appSetting.RuleList.Where(x => $"{char.ConvertToUtf32(x.Emoji, 0)}" == eventBody.ReactionEmoji.Id).ToList();

                    //根据规则列表赋予用户相应身份组
                    foreach (var item in ruleList)
                    {
                        try
                        {
                            if (eventBody.ReactionType == 1)
                            {
                                await _openApiService.SetRoleMemberAddAsync(new SetRoleMemberAddInput
                                {
                                    IslandId = eventBody.IslandId,
                                    DodoId = eventBody.DodoId,
                                    RoleId = item.RoleId
                                }, true);
                            }
                            else
                            {
                                await _openApiService.SetRoleMemberRemoveAsync(new SetRoleMemberRemoveInput
                                {
                                    IslandId = eventBody.IslandId,
                                    DodoId = eventBody.DodoId,
                                    RoleId = item.RoleId
                                }, true);
                            }
                        }
                        catch (Exception)
                        {
                            //若身份组赋予失败，则取消他此时反应的表情
                            await _openApiService.SetChannelMessageReactionRemoveAsync(new SetChannelMessageReactionRemoveInput
                            {
                                MessageId = _appSetting.MessageId,
                                Emoji = eventBody.ReactionEmoji,
                                DodoId = eventBody.DodoId
                            });
                        }
                    }

                }

                #endregion

            }
            catch (Exception e)
            {
                Exception(e.Message);
            }

        }
    }
}
