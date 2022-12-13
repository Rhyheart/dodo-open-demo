﻿namespace DoDo.Open.ChatGPT
{
    public class AppSetting
    {
        /// <summary>
        /// 机器人唯一标识
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// 机器人鉴权Token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// ChatGPT配置
        /// </summary>
        public ChatGPT ChatGPT { get; set; }
    }

    public class ChatGPT
    {
        /// <summary>
        /// Token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 语言模型
        /// </summary>
        public string Model { get; set; }
    }

}