namespace DoDo.Open.ChatGPT
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
        /// 规则列表
        /// </summary>
        public List<Rule> RuleList { get; set; }
    }

    public class Rule
    {
        /// <summary>
        /// 关键词，支持正则匹配
        /// </summary>
        public string KeyWord { get; set; }

        /// <summary>
        /// 回复
        /// </summary>
        public string Reply { get; set; }
    }

}