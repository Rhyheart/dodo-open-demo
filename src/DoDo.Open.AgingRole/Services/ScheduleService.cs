using Quartz;
using Quartz.Impl;

namespace DoDo.Open.AgingRole.Services
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

            var roleJob = JobBuilder.Create<AgingRoleJobService>()
                .WithIdentity("RoleJob", "Role")
                .Build();
            var roleTrigger = TriggerBuilder.Create()
                .WithIdentity("RoleTrigger", "Role")
                .WithCronSchedule("0 */30 * * * ?")
                .Build();
            await scheduler.ScheduleJob(roleJob, roleTrigger);
        }
    }

}
