using DoDo.Open.Info;
using DoDo.Open.Sdk.Models;
using DoDo.Open.Sdk.Models.Channels;
using DoDo.Open.Sdk.Models.Islands;
using DoDo.Open.Sdk.Models.Roles;
using DoDo.Open.Sdk.Services;
using Microsoft.Extensions.Configuration;

//获取配置
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false)
    .Build();
var appSetting = configuration.Get<AppSetting>();

//接口服务
var openApiService = new OpenApiService(new OpenApiOptions
{
    BaseApi = "https://botopen.imdodo.com",
    ClientId = appSetting.ClientId,
    Token = appSetting.Token
});

string reply;

#region 取群信息

Console.WriteLine("[ 1、取群信息 ]\n");

var getIslandInfoOutput = openApiService.GetIslandInfo(new GetIslandInfoInput
{
    IslandId = appSetting.IslandId
});

reply = "";
reply += $"群号：{getIslandInfoOutput.IslandId}\n";
reply += $"群名称：{getIslandInfoOutput.IslandName}\n";
reply += $"群头像：{getIslandInfoOutput.CoverUrl}\n";
reply += $"成员数：{getIslandInfoOutput.MemberCount}\n";
reply += $"在线成员数：{getIslandInfoOutput.OnlineMemberCount}\n";
reply += $"群描述：{getIslandInfoOutput.Description}";
reply += $"默认进入频道：{getIslandInfoOutput.DefaultChannelId}\n";
reply += $"系统通知频道：{getIslandInfoOutput.SystemChannelId}\n";

Console.WriteLine(reply);

#endregion

#region 取频道列表

Console.WriteLine("[ 2、取频道列表 ]\n");

var getChannelListOutput = openApiService.GetChannelList(new GetChannelListInput
{
    IslandId = appSetting.IslandId
});

reply = "";

foreach (var output in getChannelListOutput)
{
    reply += $"频道号：{output.ChannelId}\n";
    reply += $"频道名称：{output.ChannelName}\n";
    reply += $"频道类型：{output.ChannelType}\n";
    reply += "\n";
}

Console.WriteLine(reply);

#endregion

#region 取身份组列表

Console.WriteLine("[ 3、取身份组列表 ]\n");

var getRoleListOutput = openApiService.GetRoleList(new GetRoleListInput
{
    IslandId = appSetting.IslandId
});

reply = "";

foreach (var output in getRoleListOutput)
{
    reply += $"身份组名称：{output.RoleId}\n";
    reply += $"身份组ID：{output.RoleName}\n";
    reply += $"身份组颜色：{output.RoleColor}\n";
    reply += "\n";
}

Console.WriteLine(reply); 

#endregion

Console.ReadKey();