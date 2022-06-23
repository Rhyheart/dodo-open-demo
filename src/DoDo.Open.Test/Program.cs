using DoDo.Open.Sdk.Models;
using DoDo.Open.Sdk.Services;
using DoDo.Open.Test;
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
//事件处理服务，可自定义，只要继承EventProcessService抽象类即可
var eventProcessService = new DemoEventProcessService(openApiService);
//事件服务
var openEventService = new OpenEventService(openApiService, eventProcessService, new OpenEventOptions
{
    IsReconnect = true,
    IsAsync = true
});
//接收事件消息
await openEventService.ReceiveAsync();

Console.ReadKey();