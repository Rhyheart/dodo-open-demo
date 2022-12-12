using DoDo.Open.Sdk.Models.Roles;
using Quartz;

namespace DoDo.Open.AgingRole.Services
{
    public class JobService : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var openApiService = AppEnvironment.OpenApiService;

            try
            {
                var filePaths = Directory.GetFiles($"{Environment.CurrentDirectory}\\data");
                foreach (var filePath in filePaths)
                {
                    var islandId = Path.GetFileNameWithoutExtension(filePath);
                    var dodoIdList = DataHelper.ReadSections(filePath);

                    foreach (var dodoId in dodoIdList)
                    {
                        var roleIdList = DataHelper.ReadKeys(filePath, dodoId);

                        foreach (var roleId in roleIdList)
                        {
                            var expirationTime = DataHelper.ReadValue<DateTime>(filePath, dodoId, roleId);
                            if (expirationTime < DateTime.Now)
                            {
                                await openApiService.SetRoleMemberRemoveAsync(new SetRoleMemberRemoveInput
                                {
                                    IslandSourceId = islandId,
                                    DodoSourceId = dodoId,
                                    RoleId = roleId
                                });
                                DataHelper.DeleteKey(filePath, dodoId, roleId);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
