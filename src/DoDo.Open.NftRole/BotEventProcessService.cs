using DoDo.Open.Sdk.Models.Channels;
using DoDo.Open.Sdk.Models.Events;
using DoDo.Open.Sdk.Models.Members;
using DoDo.Open.Sdk.Models.Messages;
using DoDo.Open.Sdk.Models.Personals;
using DoDo.Open.Sdk.Models.Roles;
using DoDo.Open.Sdk.Services;
using Newtonsoft.Json;

namespace DoDo.Open.NftRole
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

            #region 初始化

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
                var appData = JsonConvert.DeserializeObject<AppData>(data);
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
                    content += $"选择{item.Emoji}，获得身份组 [ {roleList.FirstOrDefault(x => x.RoleId == item.RoleId)?.RoleName} ]，";
                    if (string.IsNullOrWhiteSpace(item.Series))
                    {
                        content += $"需要拥有发行商 [ {item.Issuer} ] 的数字藏品。\n";
                    }
                    else
                    {
                        content += $"需要拥有发行商 [ {item.Issuer} ] 下 [ {item.Series} ] 系列的数字藏品。\n";
                    }
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
                        ReactionEmoji = new MessageModelEmoji
                        {
                            Type = 1,
                            Id = $"{char.ConvertToUtf32(item.Emoji, 0)}"
                        }
                    });
                }

                //若身份组领取消息发送成功，则记录并存储当前消息ID
                _appSetting.MessageId = setChannelMessageSendOutput.MessageId;
                using var writer = new StreamWriter(dataFilePath, false);
                writer.Write(JsonConvert.SerializeObject(new AppData
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

        public override void MessageReactionEvent(EventSubjectOutput<EventSubjectDataBusiness<EventBodyMessageReaction>> input)
        {
            var eventBody = input.Data.EventBody;

            //校验当前反应的消息是否为配置的身份组领取消息
            if (_appSetting.MessageId == eventBody.ReactionTarget.Id)
            {
                //筛选当前反应的表情对应的规则列表
                var ruleList = _appSetting.RuleList.Where(x => $"{char.ConvertToUtf32(x.Emoji, 0)}" == eventBody.ReactionEmoji.Id).ToList();

                try
                {
                    //根据规则列表赋予用户相应身份组
                    foreach (var item in ruleList)
                    {
                        if (eventBody.ReactionType == 1)
                        {
                            if (string.IsNullOrWhiteSpace(item.Series))
                            {
                                item.Series = "全部";
                            }

                            var getMemberUPowerchainInfoOutput = _openApiService.GetMemberUPowerchainInfo(new GetMemberUPowerchainInfoInput
                            {
                                IslandId = eventBody.IslandId,
                                DoDoId = eventBody.DodoId,
                                Issuer = item.Issuer,
                                Series = item.Series
                            }, true);

                            if (item.Series == "全部")
                            {
                                getMemberUPowerchainInfoOutput.IsHaveSeries = 1;
                            }

                            if (getMemberUPowerchainInfoOutput.IsHaveIssuer == 0)
                            {
                                throw new Exception($"您没有发行商 [ {item.Issuer} ] 的数字藏品");
                            }

                            if (getMemberUPowerchainInfoOutput.IsHaveSeries == 0)
                            {
                                throw new Exception($"您没有发行商 [ {item.Issuer} ] 下 [ {item.Series} ] 系列的数字藏品");
                            }

                            _openApiService.SetRoleMemberAdd(new SetRoleMemberAddInput
                            {
                                IslandId = eventBody.IslandId,
                                DoDoId = eventBody.DodoId,
                                RoleId = item.RoleId
                            }, true);
                        }
                        else
                        {
                            _openApiService.SetRoleMemberRemove(new SetRoleMemberRemoveInput
                            {
                                IslandId = eventBody.IslandId,
                                DoDoId = eventBody.DodoId,
                                RoleId = item.RoleId
                            }, true);
                        }
                    }
                }
                catch (Exception e)
                {
                    //若用户不满足规则要求，则取消他此时反应的表情
                    _openApiService.SetChannelMessageReactionRemove(new SetChannelMessageReactionRemoveInput
                    {
                        MessageId = _appSetting.MessageId,
                        ReactionEmoji = eventBody.ReactionEmoji,
                        DoDoId = eventBody.DodoId
                    });

                    //私聊告知用户不满足规则要求的具体原因
                    _openApiService.SetPersonalMessageSend(new SetPersonalMessageSendInput<MessageBodyText>
                    {
                        DoDoId = eventBody.DodoId,
                        MessageBody = new MessageBodyText
                        {
                            Content = e.Message.Replace("当前用户", "您")
                        }
                    });
                }
            }

        }
    }
}
