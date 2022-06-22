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

#region 群基础信息获取

try
{
    Console.WriteLine("\n\n[ 取群信息 ]\n");

    openApiService.GetIslandInfo(new GetIslandInfoInput
    {
        IslandId = appSetting.IslandId
    }, true);

    Console.WriteLine("\n[ 取频道列表 ]\n");

    openApiService.GetChannelList(new GetChannelListInput
    {
        IslandId = appSetting.IslandId
    }, true);

    Console.WriteLine("\n[ 取身份组列表 ]\n");

    openApiService.GetRoleList(new GetRoleListInput
    {
        IslandId = appSetting.IslandId
    }, true);

    Console.ReadKey();
}
catch (Exception e)
{
    Console.WriteLine("发生异常：" + e.Message);
}

#endregion

Console.ReadKey();
