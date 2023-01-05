using DoDo.Open.Sdk.Models.ChannelMessages;
using DoDo.Open.Sdk.Models.Messages;
using Quartz;

namespace DoDo.Open.LuckDraw.Services
{
    public class JobService : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var openApiService = AppEnvironment.OpenApiService;

            try
            {
                #region 监控抽奖消息

                var filePaths = Directory.GetFiles($"{Environment.CurrentDirectory}\\data\\luck_draw");
                foreach (var filePath in filePaths)
                {
                    var islandId = Path.GetFileNameWithoutExtension(filePath);
                    var luckDrawDataPath = $"{Environment.CurrentDirectory}\\data\\luck_draw\\{islandId}.txt";
                    var memberDataPath = $"{Environment.CurrentDirectory}\\data\\member\\{islandId}.txt";
                    var messageIdList = DataHelper.ReadSections(filePath);
                    foreach (var messageId in messageIdList)
                    {
                        try
                        {
                            var status = DataHelper.ReadValue<int>(luckDrawDataPath, messageId, "Status");
                            var cardEndTime = DataHelper.ReadValue<long>(luckDrawDataPath, messageId, "EndTime");

                            //卡片状态为填写中时，校验是否填写超时
                            if (status == 1 && DateTime.Now.GetTimeStamp() >= cardEndTime)
                            {
                                await openApiService.SetChannelMessageWithdrawAsync(new SetChannelMessageWithdrawInput
                                {
                                    MessageId = messageId,
                                    Reason = "抽奖内容填写超时"
                                });

                                DataHelper.DeleteSection(luckDrawDataPath, messageId);
                            }
                            //卡片状态为抽奖中时，校验是否抽奖结束，若结束，则从参与抽奖的用户中随机抽取一名幸运用户
                            else if (status == 2 && DateTime.Now.GetTimeStamp() >= cardEndTime)
                            {
                                var cardContent = (DataHelper.ReadValue<string>(luckDrawDataPath, messageId, "Content") ?? "").Replace("\\n", "\n");
                                var cardParticipants = DataHelper.ReadValue<string>(luckDrawDataPath, messageId, "Participants") ?? "";
                                var cardParticipantList = new List<string>();
                                if (!string.IsNullOrWhiteSpace(cardParticipants))
                                {
                                    cardParticipantList = cardParticipants.Split("|").ToList();
                                }

                                var card = new MessageModelCard
                                {
                                    Type = "card",
                                    Title = "抽奖结束",
                                    Theme = "default",
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

                                if (cardParticipantList.Count > 0)
                                {
                                    var dodoId = cardParticipantList[new Random().Next(0, cardParticipantList.Count)];

                                    card.Components.Add(new
                                    {
                                        type = "section",
                                        text = new
                                        {
                                            type = "dodo-md",
                                            content = $"恭喜 [ {dodoId} ] [ {DataHelper.ReadValue<string>(memberDataPath, dodoId, "NickName")} ] 获得本次大奖！"
                                        }
                                    });

                                    card.Components.Add(new
                                    {
                                        type = "image-group",
                                        elements = new List<object>
                                 {
                                    new
                                    {
                                        type = "image",
                                        src = DataHelper.ReadValue<string>(memberDataPath,dodoId,"AvatarUrl")
                                    }
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
                                }
                                else
                                {
                                    card.Components.Add(new
                                    {
                                        type = "section",
                                        text = new
                                        {
                                            type = "dodo-md",
                                            content = "无人参与抽奖！"
                                        }
                                    });
                                }

                                await openApiService.SetChannelMessageEditAsync(new SetChannelMessageEditInput<MessageBodyCard>
                                {
                                    MessageId = messageId,
                                    MessageBody = new MessageBodyCard
                                    {
                                        Card = card
                                    }
                                });

                                //抽奖完毕后，将抽奖记录删除，防止定时任务重复判断
                                DataHelper.DeleteSection(luckDrawDataPath, messageId);

                                //由于卡片编辑接口限制1秒1次，因此这里调用完编辑接口延迟1秒钟，避免下次编辑失败
                                Thread.Sleep(1000);

                            }
                        }
                        catch (Exception e)
                        {
                            // ignored
                        }

                    }
                } 

                #endregion
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
