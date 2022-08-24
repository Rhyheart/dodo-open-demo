using Quartz;
using Quartz.Impl;

namespace DoDo.Open.LuckDraw.Services
{
    public class ScheduleService
    {
        public void Init()
        {
            Task.Factory.StartNew(InitAsync);
        }

        private async void InitAsync()
        {
            var scheduler = await StdSchedulerFactory.GetDefaultScheduler();

            await scheduler.Start();

            var jobService = JobBuilder.Create<JobService>()
                .WithIdentity("RoleJob", "Role")
                .Build();
            var roleTrigger = TriggerBuilder.Create()
                .WithIdentity("RoleTrigger", "Role")
                .WithCronSchedule("0 */30 * * * ?")
                .Build();
            await scheduler.ScheduleJob(jobService, roleTrigger);
        }
    }

}
