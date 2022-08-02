namespace DoDo.Open.RoleReceive
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
        /// 群号
        /// </summary>
        public string IslandId { get; set; }

        /// <summary>
        /// 频道号
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 消息ID
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// 规则列表
        /// </summary>
        public List<Rule> RuleList { get; set; }
    }

    public class Rule
    {
        /// <summary>
        /// 表情
        /// </summary>
        public string Emoji { get; set; }

        /// <summary>
        /// 身份组ID
        /// </summary>
        public string RoleId { get; set; }
    }
}
