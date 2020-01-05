using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Quartz;

namespace Jerry.CMS.Job
{
    public class TestJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            string serverName = dataMap.GetString("ServerName");
            if (string.IsNullOrEmpty(serverName))
            {
                serverName = "kong";
            }
            Debug.WriteLine($"{DateTime.Now.ToString()}--TestJob execute!");
            //logger.Error($"Hello, {serverName},at {DateTime.Now.ToString()}");
            await Task.CompletedTask;
        }
    }
}
