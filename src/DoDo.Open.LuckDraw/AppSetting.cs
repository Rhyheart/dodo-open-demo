namespace DoDo.Open.LuckDraw
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
        /// 抽奖配置
        /// </summary>
        public LuckDraw LuckDraw { get; set; }
    }

    public class LuckDraw
    {
        /// <summary>
        /// 周卡指令
        /// </summary>
        public string Command { get; set; }
    }
}