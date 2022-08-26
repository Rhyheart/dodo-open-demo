using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;
using DoDo.Open.Sdk.Models;
using DoDo.Open.Sdk.Models.Channels;
using DoDo.Open.Sdk.Models.Events;
using DoDo.Open.Sdk.Models.Messages;
using DoDo.Open.Sdk.Services;

namespace DoDo.Open.LuckDraw
{
    public class BotEventProcessService : EventProcessService
    {
        private readonly OpenApiService _openApiService;
        private readonly OpenApiOptions _openApiOptions;
        private readonly AppSetting _appSetting;
        private readonly ActionBlock<ActionBlockModel> _actionBlock;

        public BotEventProcessService(OpenApiService openApiService, AppSetting appSetting)
        {
            _openApiService = openApiService;
            _openApiOptions = openApiService.GetBotOptions();
            _appSetting = appSetting;
            _actionBlock = new ActionBlock<ActionBlockModel>(async request =>
            {
                try
                {
                    #region 编辑抽奖卡片

                    var luckDrawDataPath = $"{Environment.CurrentDirectory}\\data\\luck_draw\\{request.IslandId}.txt";
                    if (DataHelper.ReadValue<int>(luckDrawDataPath, request.MessageId, "status") == 2)
                    {
                        var cardParticipants = DataHelper.ReadValue<string>(luckDrawDataPath, request.MessageId, "Participants") ?? "";
                        //必须是抽奖中且是最后一次抽奖请求，才会更新抽奖卡片状态
                        if (Regex.IsMatch(cardParticipants, $".*{request.DodoId}$"))
                        {
                            await _openApiService.SetChannelMessageEditAsync(request.Input);
                            //由于卡片编辑接口限制1秒1次，因此这里调用完编辑接口延迟1秒钟，避免下次编辑失败
                            Thread.Sleep(1000);
                        }
                    } 

                    #endregion
                }
                catch (Exception e)
                {
                    Exception(e.Message);
                }
               
            });
        }

        public class ActionBlockModel
        {
            public string IslandId { get; set; }
            public string DodoId { get; set; }
            public string MessageId { get; set; }
            public SetChannelMessageEditInput<MessageBodyCard> Input { get; set; }
        }

        public override void Connected(string message)
        {
            _openApiOptions.Log?.Invoke($"Connected: {message}");

            #region 抽奖初始化

            if (!Directory.Exists($"{Environment.CurrentDirectory}\\data"))
            {
                Directory.CreateDirectory($"{Environment.CurrentDirectory}\\data");
            }
            if (!Directory.Exists($"{Environment.CurrentDirectory}\\data\\luck_draw"))
            {
                Directory.CreateDirectory($"{Environment.CurrentDirectory}\\data\\luck_draw");
            }
            if (!Directory.Exists($"{Environment.CurrentDirectory}\\data\\member"))
            {
                Directory.CreateDirectory($"{Environment.CurrentDirectory}\\data\\member");
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
                    var defaultReply = $"<@!{eventBody.DodoId}>";
                    var reply = defaultReply;

                    #region 发起抽奖

                    var luckDrawDataPath = $"{Environment.CurrentDirectory}\\data\\luck_draw\\{eventBody.IslandId}.txt";
                    var memberDataPath = $"{Environment.CurrentDirectory}\\data\\member\\{eventBody.IslandId}.txt";

                    if (content == "发起抽奖")
                    {
                        //发起抽奖指令触发抽奖卡片发出

                        var cardEndTime = DateTime.Now.AddMinutes(10).GetTimeStamp();

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
                                content = $"[ {eventBody.DodoId} ] [ {eventBody.Member.NickName} ] 发起了抽奖"
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
                                    click = new
                                    {
                                        action = "form",
                                        value = ""
                                    },
                                    color = "grey",
                                    name = "请填写抽奖内容发起抽奖",
                                    form = new
                                    {
                                        title = "填写抽奖内容",
                                        elements = new List<object>
                                        {
                                            new
                                            {
                                                type = "input",
                                                key = "duration",
                                                title = "抽奖倒计时（分钟）",
                                                rows = 1,
                                                placeholder = "请输入数字，默认10分钟，最大可填写6000分钟",
                                                minChar = 0,
                                                maxChar = 4
                                            },
                                            new
                                            {
                                                type = "input",
                                                key = "content",
                                                title = "抽奖内容",
                                                rows = 4,
                                                placeholder = "最多可填写4000字符",
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
                            title = "发起抽奖时，10分钟内未填写将结束",
                            style = "hour",
                            endTime = cardEndTime
                        });

                        //发送卡片消息
                        var setChannelMessageSendOutput = await _openApiService.SetChannelMessageSendAsync(
                            new SetChannelMessageSendInput<MessageBodyCard>
                            {
                                ChannelId = eventBody.ChannelId,
                                MessageBody = new MessageBodyCard
                                {
                                    Card = card
                                }
                            }, true);

                        //记录卡片状态，1：填写中，2：抽奖中
                        DataHelper.WriteValue(luckDrawDataPath, setChannelMessageSendOutput.MessageId, "Status", 1);
                        DataHelper.WriteValue(luckDrawDataPath, setChannelMessageSendOutput.MessageId, "EndTime", cardEndTime);
                        DataHelper.WriteValue(luckDrawDataPath, setChannelMessageSendOutput.MessageId, "Sponsor", eventBody.DodoId);
                        DataHelper.WriteValue(memberDataPath, eventBody.DodoId, "NickName", eventBody.Member.NickName);
                        DataHelper.WriteValue(memberDataPath, eventBody.DodoId, "AvatarUrl", eventBody.Personal.AvatarUrl);

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

        public override void CardMessageButtonClickEvent(EventSubjectOutput<EventSubjectDataBusiness<EventBodyCardMessageButtonClick>> input)
        {
            try
            {
                var eventBody = input.Data.EventBody;

                #region 参与抽奖

                var luckDrawDataPath = $"{Environment.CurrentDirectory}\\data\\luck_draw\\{eventBody.IslandId}.txt";
                var memberDataPath = $"{Environment.CurrentDirectory}\\data\\member\\{eventBody.IslandId}.txt";

                //必须是抽奖中状态，用户才可参与抽奖
                if (DataHelper.ReadValue<int>(luckDrawDataPath, eventBody.MessageId, "status") == 2)
                {
                    var cardEndTime = DataHelper.ReadValue<long>(luckDrawDataPath, eventBody.MessageId, "EndTime");

                    //必须在有效期内，用户才可参与抽奖
                    if (cardEndTime >= DateTime.Now.GetTimeStamp())
                    {
                        var cardContent = (DataHelper.ReadValue<string>(luckDrawDataPath, eventBody.MessageId, "Content") ?? "").Replace("\\n", "\n");
                        var cardParticipants = DataHelper.ReadValue<string>(luckDrawDataPath, eventBody.MessageId, "Participants") ?? "";
                        var cardParticipantList = new List<string>();
                        if (!string.IsNullOrWhiteSpace(cardParticipants))
                        {
                            cardParticipantList = cardParticipants.Split("|").ToList();
                        }

                        //用户不可重复抽奖
                        if (!cardParticipantList.Contains(eventBody.DodoId))
                        {
                            cardParticipantList.Add(eventBody.DodoId);

                            DataHelper.WriteValue(luckDrawDataPath, eventBody.MessageId, "Participants", string.Join("|", cardParticipantList));

                            DataHelper.WriteValue(memberDataPath, eventBody.DodoId, "NickName", eventBody.Member.NickName);
                            DataHelper.WriteValue(memberDataPath, eventBody.DodoId, "AvatarUrl", eventBody.Personal.AvatarUrl);

                            var card = new Card
                            {
                                Type = "card",
                                Title = "抽奖",
                                Theme = "green",
                                Components = new List<object>()
                            };

                            card.Components.Add(new
                            {
                                type = "section",
                                text = new
                                {
                                    type = "dodo-md",
                                    content = cardContent
                                }
                            });

                            card.Components.Add(new
                            {
                                type = "divider"
                            });

                            var remarkElements = new List<object>();

                            foreach (var cardParticipant in cardParticipantList)
                            {
                                remarkElements.Add(new
                                {
                                    type = "image",
                                    src = DataHelper.ReadValue<string>(memberDataPath, cardParticipant, "AvatarUrl")
                                });
                                remarkElements.Add(new
                                {
                                    type = "dodo-md",
                                    content = DataHelper.ReadValue<string>(memberDataPath, cardParticipant, "NickName")
                                });
                            }

                            card.Components.Add(new
                            {
                                type = "remark",
                                elements = remarkElements
                            });

                            card.Components.Add(new
                            {
                                type = "divider"
                            });

                            card.Components.Add(new
                            {
                                type = "countdown",
                                title = "抽奖倒计时",
                                style = "hour",
                                endTime = cardEndTime
                            });

                            card.Components.Add(new
                            {
                                type = "button-group",
                                elements = new List<object>
                                {
                                    new
                                    {
                                        type = "button",
                                        click = new
                                        {
                                            action = "call_back",
                                            value = "回传参数"
                                        },
                                        color = "green",
                                        name = "点击此处参与抽奖，每人只能点击一次"
                                    }
                                }
                            });

                            //这里通过ActionBlock组件将并发执行转成顺序执行，防止并发修改导致卡片消息混乱
                            _actionBlock.Post(new ActionBlockModel
                            {
                                IslandId = eventBody.IslandId,
                                DodoId = eventBody.DodoId,
                                MessageId = eventBody.MessageId,
                                Input = new SetChannelMessageEditInput<MessageBodyCard>
                                {
                                    MessageId = eventBody.MessageId,
                                    MessageBody = new MessageBodyCard
                                    {
                                        Card = card
                                    }
                                }
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

        public override async void CardMessageFormSubmitEvent(EventSubjectOutput<EventSubjectDataBusiness<EventBodyCardMessageFormSubmit>> input)
        {
            try
            {
                var eventBody = input.Data.EventBody;

                #region 填写抽奖内容

                var luckDrawDataPath = $"{Environment.CurrentDirectory}\\data\\luck_draw\\{eventBody.IslandId}.txt";

                //抽奖状态必须是填写中且用户必须是抽奖发起人才允许更新抽奖内容
                if (DataHelper.ReadValue<int>(luckDrawDataPath, eventBody.MessageId, "status") == 1 && DataHelper.ReadValue<string>(luckDrawDataPath, eventBody.MessageId, "Sponsor") == eventBody.DodoId)
                {
                    var cardEndTime = DataHelper.ReadValue<long>(luckDrawDataPath, eventBody.MessageId, "EndTime");

                    //抽奖内容必须在有效期内填写
                    if (cardEndTime >= DateTime.Now.GetTimeStamp())
                    {
                        var formDurationItem = eventBody.FormData.FirstOrDefault(x => x.Key == "duration")?.value ?? "";
                        int.TryParse(formDurationItem, out var formDuration);
                        if (formDuration > 6000)
                        {
                            formDuration = 6000;
                        }
                        else if (formDuration == 0)
                        {
                            formDuration = 10;
                        }
                        cardEndTime = DateTime.Now.AddMinutes(formDuration).GetTimeStamp();
                        var cardContent = eventBody.FormData.FirstOrDefault(x => x.Key == "content")?.value ?? "";

                        var card = new Card
                        {
                            Type = "card",
                            Title = "抽奖",
                            Theme = "green",
                            Components = new List<object>()
                        };

                        card.Components.Add(new
                        {
                            type = "section",
                            text = new
                            {
                                type = "dodo-md",
                                content = cardContent
                            }
                        });

                        card.Components.Add(new
                        {
                            type = "divider"
                        });

                        card.Components.Add(new
                        {
                            type = "countdown",
                            title = "抽奖倒计时",
                            style = "hour",
                            endTime = cardEndTime
                        });

                        card.Components.Add(new
                        {
                            type = "button-group",
                            elements = new List<object>
                            {
                                new
                                {
                                    type = "button",
                                    click = new
                                    {
                                        action = "call_back",
                                        value = "回传参数"
                                    },
                                    color = "green",
                                    name = "点击此处参与抽奖，每人只能点击一次"
                                }
                            }
                        });

                        await _openApiService.SetChannelMessageEditAsync(new SetChannelMessageEditInput<MessageBodyCard>
                        {
                            MessageId = eventBody.MessageId,
                            MessageBody = new MessageBodyCard
                            {
                                Card = card
                            }
                        }, true);

                        //抽奖内容填写完毕后，将卡片状态更改为抽奖中
                        DataHelper.WriteValue(luckDrawDataPath, eventBody.MessageId, "Status", 2);
                        DataHelper.WriteValue(luckDrawDataPath, eventBody.MessageId, "EndTime", cardEndTime);
                        DataHelper.WriteValue(luckDrawDataPath, eventBody.MessageId, "Content", cardContent.Replace("\n", "\\n"));
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
