using System.Text.RegularExpressions;
using DoDo.Open.Sdk.Models.Channels;
using DoDo.Open.Sdk.Models.Messages;
using DoDo.Open.Sdk.Models.Roles;
using DoDo.Open.Sdk.Services;
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
                var filePaths = Directory.GetFiles($"{Environment.CurrentDirectory}\\data\\luck_draw");
                foreach (var filePath in filePaths)
                {
                    var islandId = Path.GetFileNameWithoutExtension(filePath);
                    var luckDrawDataPath = $"{Environment.CurrentDirectory}\\data\\luck_draw\\{islandId}.txt";
                    var memberDataPath = $"{Environment.CurrentDirectory}\\data\\member\\{islandId}.txt";
                    var messageIdList = DataHelper.ReadSections(filePath);
                    foreach (var messageId in messageIdList)
                    {
                        var status = DataHelper.ReadValue<int>(luckDrawDataPath, messageId, "Status");
                        var cardEndTime = DataHelper.ReadValue<long>(luckDrawDataPath, messageId, "EndTime");

                        if (status == 1 && DateTime.Now.GetTimeStamp() >= cardEndTime)
                        {
                            await openApiService.SetChannelMessageWithdrawAsync(new SetChannelMessageWithdrawInput
                            {
                                MessageId = messageId,
                                Reason = "抽奖内容填写超时"
                            }, true);

                            DataHelper.DeleteSection(luckDrawDataPath, messageId);
                        }
                        else if (status == 2 && DateTime.Now.GetTimeStamp() >= cardEndTime)
                        {
                            var cardContent = DataHelper.ReadValue<string>(luckDrawDataPath, messageId, "Content") ?? "";
                            var cardParticipants = DataHelper.ReadValue<string>(luckDrawDataPath, messageId, "Participants") ?? "";
                            var cardParticipantList = new List<string>();
                            if (!string.IsNullOrWhiteSpace(cardParticipants))
                            {
                                cardParticipantList = cardParticipants.Split("|").ToList();
                            }

                            var card = new Card
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
                            },true);

                            DataHelper.DeleteSection(luckDrawDataPath, messageId);
                        }

                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
