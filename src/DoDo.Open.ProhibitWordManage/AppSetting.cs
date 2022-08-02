namespace DoDo.Open.ProhibitWordManage
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
        /// 违禁词，支持正则匹配
        /// </summary>
        public string KeyWord { get; set; }

        /// <summary>
        /// 是否撤回，是否撤回,0：是，1：否
        /// </summary>
        public int IsWithdraw { get; set; }

        /// <summary>
        /// 禁言时长，单位分钟，为0时则代表不禁言
        /// </summary>
        public int MuteDuration { get; set; }
    }

}