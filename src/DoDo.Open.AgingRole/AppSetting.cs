namespace DoDo.Open.AgingRole
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
        /// 周卡配置
        /// </summary>
        public WeekCard WeekCard { get; set; }

        /// <summary>
        /// 月卡配置
        /// </summary>
        public MonthCard MonthCard { get; set; }

        /// <summary>
        /// 查询配置
        /// </summary>
        public Query Query { get; set; }
    }

    public class WeekCard
    {
        /// <summary>
        /// 周卡指令
        /// </summary>
        public string Command { get; set; }
    }

    public class MonthCard
    {
        /// <summary>
        /// 月卡指令
        /// </summary>
        public string Command { get; set; }
    }

    public class Query
    {
        /// <summary>
        /// 查询指令
        /// </summary>
        public string Command { get; set; }
    }
}