
<p align="center">
  <a href="https://open.imdodo.com">
    <img src="https://open.imdodo.com/images/hero.png" width="200" height="200" alt="dodo-open">
  </a>
</p>

<div align="center">

  # dodo-open-demo

  _✨ 基于最新 C# .NET 6 开发，支持Windows、MacOS、Linux、Docker，完美跨平台。 ✨_

  <a href="https://github.com/Rhyheart/dodo-open-demo/blob/main/LICENSE">
    <img src="https://img.shields.io/github/license/Rhyheart/dodo-open-demo" alt="license">
  </a>
  <a href="https://github.com/Rhyheart/dodo-open-demo/releases">
    <img src="https://img.shields.io/github/v/release/Rhyheart/dodo-open-demo?color=blueviolet&include_prereleases"
      alt="release">
  </a>

</div>


## 项目介绍

本项目包含了DoDo开放平台机器人开发相关Demo，Demo基于DoDo开放平台官方 [.Net SDK](https://github.com/dodo-open/dodo-open-net)


## 开发工具

[Visual Studio 2022](https://visualstudio.microsoft.com/zh-hans/vs/)

安装时，请勾选ASP.NET和Web开发组件，其他组件按需安装


## 执行程序

对于不具有开发能力的普通用户，本项目提供了编译完成的Windows执行程序，可以从 [Release](https://github.com/Rhyheart/dodo-open-demo/releases) 中进行下载，本执行程序依赖.Net 6运行环境，因此您需要先下载安装 [dotnet-runtime-6.0.6-win-x64.exe](https://download.visualstudio.microsoft.com/download/pr/7989338b-8ae9-4a5d-8425-020148016812/c26361fde7f706279265a505b4d1d93a/dotnet-runtime-6.0.6-win-x64.exe) 到您的电脑中！


## 项目列表

<details>
<summary>测试机器人</summary>

#### 项目地址

[DoDo.Open.Test](https://github.com/Rhyheart/dodo-open-demo/tree/main/src/DoDo.Open.Test)

#### 项目介绍

本机器人项目用于测试DoDo开放平台相关接口，包含大量SDK自带的测试用例

#### 使用步骤

0、[视频教程](https://www.bilibili.com/video/BV1wB4y1x7qH?p=1)

1、访问 [DoDo开放平台](https://open.imdodo.com/go/introduction/deployment.html)，按照教程创建机器人，获取机器人的`ClientId`和`Token`

2、将机器人拉入测试群

3、下载解压执行程序

4、维护 [配置文件](https://github.com/Rhyheart/dodo-open-demo/blob/main/src/DoDo.Open.Test/appsettings.json)

5、启动程序

6、测试群内发送`菜单`指令即可查看所有测试功能

</details>

<details>
<summary>基础信息机器人</summary>

#### 项目地址

[DoDo.Open.Info](https://github.com/Rhyheart/dodo-open-demo/tree/main/src/DoDo.Open.Info)

#### 项目介绍

本机器人项目用于获取DoDo群相关基础信息，包含 [获取群信息](https://open.imdodo.com/dev/api/island.html#%E8%8E%B7%E5%8F%96%E7%BE%A4%E4%BF%A1%E6%81%AF)、[获取频道列表](https://open.imdodo.com/dev/api/channel.html#%E8%8E%B7%E5%8F%96%E9%A2%91%E9%81%93%E5%88%97%E8%A1%A8)、[获取身份组列表](https://open.imdodo.com/dev/api/role.html#%E8%8E%B7%E5%8F%96%E8%BA%AB%E4%BB%BD%E7%BB%84%E5%88%97%E8%A1%A8) 等功能

#### 使用步骤

1、创建机器人

2、将机器人拉入测试群

3、下载解压执行程序

2、维护 [配置文件](https://github.com/Rhyheart/dodo-open-demo/blob/main/src/DoDo.Open.Info/appsettings.json)

3、启动程序

4、通过程序控制台可获取到DoDo群相关基础信息

#### 视频教程

[视频教程](https://www.bilibili.com/video/BV1wB4y1x7qH?p=2)

</details>

<details>
<summary>NFT身份组机器人</summary>

#### 项目地址
  
[DoDo.Open.NftRole](https://github.com/Rhyheart/dodo-open-demo/tree/main/src/DoDo.Open.NftRole)

#### 项目介绍

本机器人项目用于实现NFT身份组领取功能，用户通过对NFT身份组领取消息添加对应表情反应，从而获取对应身份组

#### 使用步骤

1、创建机器人

2、将机器人拉入测试群

3、下载解压执行程序

4、维护 [配置文件](https://github.com/Rhyheart/dodo-open-demo/blob/main/src/DoDo.Open.NftRole/appsettings.json)，对于`ChannelId`和`RoleId`，请从`基础信息机器人`中获取

5、启动程序

6、前往测试频道领取对应身份组

#### 视频教程

[视频教程](https://www.bilibili.com/video/BV1wB4y1x7qH?p=3)

</details>

<details>
<summary>签到机器人</summary>

#### 项目地址

[DoDo.Open.Sign](https://github.com/Rhyheart/dodo-open-demo/tree/main/src/DoDo.Open.Sign)

#### 项目介绍

本机器人项目用于实现用户签到相关功能，包含签到、查询、转账

#### 使用步骤

1、创建机器人

2、将机器人拉入测试群

3、下载解压执行程序

4、维护 [配置文件](https://github.com/Rhyheart/dodo-open-demo/blob/main/src/DoDo.Open.Sign/appsettings.json)

5、启动程序

6、测试群内发送`签到`、`查询`、`转账 @成员 金额`指令即可，所有指令均可通过修改配置文件实现自定义

#### 视频教程

[视频教程](https://www.bilibili.com/video/BV1wB4y1x7qH?p=4)

</details>


<details>
<summary>检索机器人</summary>

#### 项目地址

[DoDo.Open.Search](https://github.com/Rhyheart/dodo-open-demo/tree/main/src/DoDo.Open.Search)

#### 项目介绍

本机器人项目用于实现各类资源检索

#### 使用步骤

1、创建机器人

2、将机器人拉入测试群

3、下载解压执行程序

4、维护 [配置文件](https://github.com/Rhyheart/dodo-open-demo/blob/main/src/DoDo.Open.Search/appsettings.json)

5、启动程序

6、测试群内发送`百科 关键词`即可

#### 视频教程

[视频教程](https://www.bilibili.com/video/BV1wB4y1x7qH?p=5)

</details>
