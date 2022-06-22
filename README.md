
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

## 开发工具

[Visual Studio 2022](https://visualstudio.microsoft.com/zh-hans/vs/)

安装时，请勾选ASP.NET和Web开发组件，其他组件按需安装

## 项目介绍

本项目包含了DoDo开放平台机器人开发相关Demo，Demo基于DoDo开放平台官方 [.Net SDK](https://github.com/dodo-open/dodo-open-net)

## 项目列表

#### [DoDo.Open.Info](https://github.com/Rhyheart/dodo-open-demo/tree/main/src/DoDo.Open.Info)

`群基础信息获取机器人项目`，包含：[取群信息](https://open.imdodo.com/api/island/info.html)、[取频道列表](https://open.imdodo.com/api/channel/list.html)、[取身份组列表](https://open.imdodo.com/api/role/list.html)

运行项目前请先维护 [配置文件](https://github.com/Rhyheart/dodo-open-demo/blob/main/src/DoDo.Open.Info/appsettings.json)，对于`ClientId`和`Token`，请从[DoDo开放平台](https://open.imdodo.com/go/introduction/deployment.html)中获取

#### [DoDo.Open.NftRole](https://github.com/Rhyheart/dodo-open-demo/tree/main/src/DoDo.Open.NftRole)

`NFT身份组领取机器人项目`，包含：NFT身份组领取规则配置、NFT身份组领取规则触发逻辑、NFT身份组领取规则解析

运行项目前请先维护 [配置文件](https://github.com/Rhyheart/dodo-open-demo/blob/main/src/DoDo.Open.NftRole/appsettings.json)，对于`ChannelId`和`RoleId`，请运行`群基础信息获取机器人项目`，从中获取
