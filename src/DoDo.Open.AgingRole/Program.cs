using System.Text.Json;
using DoDo.Open.AgingRole;
using DoDo.Open.Sdk.Models;
using DoDo.Open.Sdk.Services;
using Microsoft.Extensions.Configuration;

/*var filePath = "C:\\Users\\Rhyheart\\Desktop\\test.ini";

DataHelper.WriteValue(filePath, "section1", "key1", "value1");
DataHelper.WriteValue(filePath, "section1", "key2", "value2");
DataHelper.WriteValue(filePath, "section1", "key3", "value3");
DataHelper.WriteValue(filePath, "section2", "key1", "value1");
DataHelper.WriteValue(filePath, "section2", "key2", "value2");
DataHelper.WriteValue(filePath, "section2", "key3", "value3");
DataHelper.WriteValue(filePath, "section3", "key1", "value1");
DataHelper.WriteValue(filePath, "section3", "key2", "value2");
DataHelper.WriteValue(filePath, "section3", "key3", "value3");

var value = DataHelper.ReadValue<string>(filePath, "section1", "key1");
Console.WriteLine(value);

var keys = DataHelper.ReadKeys(filePath, "section2");
Console.WriteLine($"keys:{JsonSerializer.Serialize(keys)}");

var sections = DataHelper.ReadSections(filePath);
Console.WriteLine($"sections:{JsonSerializer.Serialize(sections)}");

Console.ReadKey();*/

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
    Token = appSetting.Token,
    Log = message =>
    {
        Console.WriteLine(message);
        Console.WriteLine();
    }
});

//事件处理服务 - 自定义
var eventProcessService = new BotEventProcessService(openApiService, appSetting);

//事件服务
var openEventService = new OpenEventService(openApiService, eventProcessService, new OpenEventOptions
{
    IsReconnect = true,
    IsAsync = true
});

//开始接收事件消息
await openEventService.ReceiveAsync();

Console.ReadKey();