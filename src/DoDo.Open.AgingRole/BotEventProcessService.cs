using System;
using System.Text.RegularExpressions;
using DoDo.Open.Sdk.Models;
using DoDo.Open.Sdk.Models.Channels;
using DoDo.Open.Sdk.Models.Events;
using DoDo.Open.Sdk.Models.Members;
using DoDo.Open.Sdk.Models.Messages;
using DoDo.Open.Sdk.Models.Roles;
using DoDo.Open.Sdk.Services;

namespace DoDo.Open.AgingRole
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

            #region 失效身份组初始化

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
            var dodoId = eventBody.DodoId;
            var nickName = eventBody.Member.NickName;

            if (eventBody.MessageBody is MessageBodyText messageBodyText)
            {
                var content = messageBodyText.Content.Replace(" ", "");
                var defaultReply = $"<@!{eventBody.DodoId}>";
                var reply = defaultReply;

                var dataPath = $"{Environment.CurrentDirectory}\\data\\{eventBody.IslandId}.txt";

                #region 时效身份组

                if (Regex.IsMatch(content, _appSetting.WeekCard.Command) || Regex.IsMatch(content, _appSetting.MonthCard.Command))
                {
                    var memberRoleList = await _openApiService.GetMemberRoleListAsync(new GetMemberRoleListInput
                    {
                        IslandId = eventBody.IslandId,
                        DodoId = eventBody.DodoId
                    });
                    var isAdmin = memberRoleList.FirstOrDefault(x => x.RoleName == "超级管理员") != null;

                    if (isAdmin)
                    {
                        var day = 0;
                        var dayShow = "";
                        var keyWord = "";

                        if (Regex.IsMatch(content, _appSetting.WeekCard.Command))
                        {
                            day = 7;
                            dayShow = "一周";
                            keyWord = @"^.*<@!(\d+)>(.*)$";
                        }
                        else if (Regex.IsMatch(content, _appSetting.MonthCard.Command))
                        {
                            day = 30;
                            dayShow = "一个月";
                            keyWord = @"^.*<@!(\d+)>(.*)$";
                        }

                        var matchResult = Regex.Match(content, keyWord);
                        var otherDoDoId = matchResult.Groups[1].Value;
                        var roleName = matchResult.Groups[2].Value;

                        if (!string.IsNullOrWhiteSpace(otherDoDoId) && !string.IsNullOrWhiteSpace(roleName))
                        {
                            var otherMemberInfo = await _openApiService.GetMemberInfoAsync(new GetMemberInfoInput
                            {
                                IslandId = eventBody.IslandId,
                                DodoId = otherDoDoId
                            });

                            if (otherMemberInfo != null)
                            {
                                var roleList = await _openApiService.GetRoleListAsync(new GetRoleListInput
                                {
                                    IslandId = eventBody.IslandId
                                });
                                var role = roleList.FirstOrDefault(x => x.RoleName == roleName);
                                if (role != null)
                                {
                                    var result = await _openApiService.SetRoleMemberAddAsync(new SetRoleMemberAddInput
                                    {
                                        IslandId = eventBody.IslandId,
                                        DodoId = otherDoDoId,
                                        RoleId = role.RoleId
                                    });

                                    if (result)
                                    {
                                        DateTime expirationTime;

                                        var oldEntityKey = $"{otherDoDoId}";
                                        var oldEntityExpirationTime = DataHelper.GetValue<string>(dataPath, oldEntityKey, "ExpirationTime");

                                        if (oldEntityExpirationTime != "")
                                        {
                                            if (Convert.ToDateTime(oldEntityExpirationTime) > DateTime.UtcNow)
                                            {
                                                expirationTime = Convert.ToDateTime(oldEntityExpirationTime).AddDays(day);
                                            }
                                            else
                                            {
                                                expirationTime = DateTime.UtcNow.AddDays(day);
                                            }

                                            oldEntityExpirationTime = expirationTime.ToString("yyyy-MM-dd HH:mm:ss");

                                            DataHelper.SetValue(dataPath, oldEntityKey, role.RoleId, oldEntityExpirationTime);
                                        }
                                        else
                                        {
                                            expirationTime = DateTime.UtcNow.AddDays(day);

                                            DataHelper.SetValue(dataPath, oldEntityKey, role.RoleId, expirationTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                        }

                                        reply += "\n**操作成功**";
                                        reply += $"\n成功为<@!{otherDoDoId}> 添加【{roleName}】身份组，时效增加{dayShow}，现到期时间为：{expirationTime:yyyy-MM-dd HH:mm:ss}";
                                    }
                                    else
                                    {
                                        reply += "\n**操作失败**";
                                        reply += $"\n赋予身份组失败，请核实本机器人是否具有管理该身份组权限！";
                                    }
                                }
                                else
                                {
                                    reply += "\n**操作失败**";
                                    reply += $"\n本群不存在【{roleName}】身份组！";
                                }
                            }
                            else
                            {
                                reply += "\n**操作失败**";
                                reply += "\n该用户未在本群内!";
                            }
                        }
                        else
                        {
                            reply += "\n**操作失败**";
                            reply += "\n您发送的指令格式有误！";
                        }
                    }
                    else
                    {
                        reply += "\n**操作失败**";
                        reply += "\n您未拥有【超级管理员】身份组，无权限为用户提供时效身份组！";
                    }
                }
                else if (Regex.IsMatch(content, _appSetting.Query.Command))
                {
                    var targetDoDoId = content.GetMiddle("<@!", ">");
                    var isAdmin = false;
                    if (!string.IsNullOrWhiteSpace(targetDoDoId))
                    {
                        var memberRoleList = await _openApiService.GetMemberRoleListAsync(new GetMemberRoleListInput
                        {
                            IslandId = eventBody.IslandId,
                            DodoId = eventBody.DodoId
                        });
                        isAdmin = memberRoleList.FirstOrDefault(x => x.RoleName == "超级管理员") != null;

                        var memberInfo = await _openApiService.GetMemberInfoAsync(new GetMemberInfoInput
                        {
                            IslandId = eventBody.IslandId,
                            DodoId = targetDoDoId
                        });
                        nickName = memberInfo?.NickName;
                    }
                    else
                    {
                        targetDoDoId = dodoId;
                    }

                    if (targetDoDoId == dodoId || isAdmin)
                    {
                        /*var list = dbContext.BotRoles.Where(x => x.ClientId == clientId && x.IslandId == islandId && x.DoDoId == targetDoDoId).ToList();
                        if (list.Count > 0)
                        {
                            reply = $"{nickName} 拥有的时效身份组如下：";
                            foreach (var item in list)
                            {
                                reply += $"\n【{item.RoleName}】{item.ExpirationTime.AddHours(timeZone):yyyy-MM-dd HH:mm:ss}";
                            }
                        }
                        else
                        {
                            reply += "\n**查询失败**";
                            reply += $"\n未查询到{(targetDoDoId == dodoId ? "您" : "对方")}的时效身份组信息！";
                        }*/
                    }
                    else
                    {
                        reply += "\n**查询失败**";
                        reply += "\n您未拥有【超级管理员】身份组，无权限查询他人的时效身份组信息！";
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
