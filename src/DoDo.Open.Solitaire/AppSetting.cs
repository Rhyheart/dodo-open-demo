namespace DoDo.Open.Solitaire
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
        /// 发起接龙配置
        /// </summary>
        public StartSolitaire StartSolitaire { get; set; }

        /// <summary>
        /// 开始接龙配置
        /// </summary>
        public JoinSolitaire JoinSolitaire { get; set; }

        /// <summary>
        /// 退出接龙配置
        /// </summary>
        public LeaveSolitaire LeaveSolitaire { get; set; }
    }

    public class StartSolitaire
    {
        /// <summary>
        /// 开始接龙指令
        /// </summary>
        public string Command { get; set; }
    }

    public class JoinSolitaire
    {
        /// <summary>
        /// 加入接龙指令
        /// </summary>
        public string Command { get; set; }
    }

    public class LeaveSolitaire
    {
        /// <summary>
        /// 退出接龙指令
        /// </summary>
        public string Command { get; set; }
    }
}