namespace DoDo.Open.Sign
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
        /// 签到配置
        /// </summary>
        public Sign Sign { get; set; }

        /// <summary>
        /// 查询配置
        /// </summary>
        public Query Query { get; set; }

        /// <summary>
        /// 转账配置
        /// </summary>
        public Transfer Transfer { get; set; }
    }

    public class Sign
    {
        /// <summary>
        /// 签到指令
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// 获得积分
        /// </summary>
        public string Integral { get; set; }
    }

    public class Query
    {
        /// <summary>
        /// 查询指令
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// 签到回复
        /// </summary>
        public string Reply { get; set; }
    }

    public class Transfer
    {
        /// <summary>
        /// 转账指令
        /// </summary>
        public string Command { get; set; }
    }
}