/**
*┌──────────────────────────────────────────────────────────────┐
*│　描    述：任务调度中心                                                    
*│　作    者：jerry.si                                             
*│　版    本：1.0                                                 
*│　创建时间：2019/12/13 10:15:45                             
*└──────────────────────────────────────────────────────────────┘
*┌──────────────────────────────────────────────────────────────┐
*│　命名空间： Jerry.CMS.Quartz                                   
*│　类    名： ScheduleCenter                                      
*└──────────────────────────────────────────────────────────────┘
*/
using Jerry.CMS.ViewModels.ResultModel;
using Quartz;
using Quartz.Impl;
using Quartz.Util;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NLog;

namespace Jerry.CMS.Quartz
{
    public class ScheduleCenter
    {
        public static Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly object Locker = new object();

        /// <summary>
        /// 任务计划
        /// </summary>
        private IScheduler Scheduler;

        public ScheduleCenter()
        {
            NameValueCollection parms = new NameValueCollection()
            {
                ////scheduler名字
                ["quartz.scheduler.instanceName"] = "TestScheduler",
                //序列化类型
                ["quartz.serializer.type"] = "binary",//json,切换为数据库存储的时候需要设置json
                ////自动生成scheduler实例ID，主要为了保证集群中的实例具有唯一标识
                //["quartz.scheduler.instanceId"] = "AUTO",
                ////是否配置集群
                //["quartz.jobStore.clustered"] = "true",
                ////线程池个数
                ["quartz.threadPool.threadCount"] = "20",
                ////类型为JobStoreXT,事务
                //["quartz.jobStore.type"] = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz",
                ////以下配置需要数据库表配合使用，表结构sql地址：https://github.com/quartznet/quartznet/tree/master/database/tables
                ////JobDataMap中的数据都是字符串
                ////["quartz.jobStore.useProperties"] = "true",
                ////数据源名称
                //["quartz.jobStore.dataSource"] = "mySS",
                ////数据表名前缀
                //["quartz.jobStore.tablePrefix"] = "QRTZ_",
                ////使用Sqlserver的Ado操作代理类
                //["quartz.jobStore.driverDelegateType"] = "Quartz.Impl.AdoJobStore.SqlServerDelegate, Quartz",
                ////数据源连接字符串
                //["quartz.dataSource.mySS.connectionString"] = "Server=localhost;Database=CzarAbpDemo;User Id = sa;Password = 1;Trusted_Connection=True;MultipleActiveResultSets=true",
                ////数据源的数据库
                //["quartz.dataSource.mySS.provider"] = "SqlServer"
            };
            //从factory中获取Schedule实例
            var factory = new StdSchedulerFactory(parms);
            Scheduler = factory.GetScheduler().GetAwaiter().GetResult();
        }

        public async Task<ScheduleResult> AddJobAsync(string jobName, string jobGroup, string jobNameSpaceAndClassName
            , string jobAssemblyName, string cropExpress)
        {
            var result = new ScheduleResult();
            try
            {
                //校验参数
                if (jobName.IsNullOrWhiteSpace() || jobGroup.IsNullOrWhiteSpace() ||
                    jobNameSpaceAndClassName.IsNullOrWhiteSpace() || jobAssemblyName.IsNullOrWhiteSpace() ||
                    cropExpress.IsNullOrWhiteSpace())
                {
                    result.ResultCode = -3;
                    result.ResultMsg = "参数不可为空";
                    return result;
                }

                var starRunTime = DateTime.Now;
                var endTime = DateTime.MaxValue;
                DateTimeOffset endRunTime = DateTimeOffset.MaxValue;

                JobKey jobKey = new JobKey(jobName, jobGroup);
                if (await Scheduler.CheckExists(jobKey))
                {
                    await Scheduler.PauseJob(jobKey);
                    await Scheduler.DeleteJob(jobKey);
                }

                var jobPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jobAssemblyName);
                Assembly assembly = Assembly.LoadFrom(jobPath); //loadfrom 可以加载依赖程序集，如nlog
                Type jobType = assembly.GetType(jobNameSpaceAndClassName);
                if (jobType == null)
                {
                    result.ResultCode = -1;
                    result.ResultMsg = "系统找不到对应的任务，请重新设置";
                    return result;
                }

                IJobDetail jobDetail = JobBuilder.Create(jobType)
                    .WithIdentity(jobKey)
                    .UsingJobData("ServerName", Scheduler.SchedulerName)
                    .Build();
                ICronTrigger trigger = (ICronTrigger)TriggerBuilder.Create()
                    .StartAt(starRunTime)
                    .EndAt(endRunTime)
                    .WithIdentity(jobName, jobGroup)
                    .WithCronSchedule(cropExpress)
                    .Build();

                await Scheduler.ScheduleJob(jobDetail, trigger);
                if (Scheduler.IsStarted != true)
                {
                    await Scheduler.Start();
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(nameof(AddJobAsync),ex,null);
                result.ResultCode = -4;
                result.ResultMsg = ex.ToString();
                return result;//出现异常
            }
        }

        public async Task<ScheduleResult> ResumeJobAsync(string jobName, string jobGroup)
        {
            ScheduleResult scheduleResult = new ScheduleResult();
            try
            {
                var jobKey = new JobKey(jobName, jobGroup);
                if (await Scheduler.CheckExists(jobKey))
                {
                    await Scheduler.PauseJob(jobKey);
                    await Scheduler.ResumeJob(jobKey);
                }
                else
                {

                    scheduleResult.ResultCode = -1;
                    scheduleResult.ResultMsg = "任务不存在";
                }
            }
            catch (Exception ex)
            {
                _logger.Error(nameof(AddJobAsync), ex,null);

                scheduleResult.ResultCode = -4;
                scheduleResult.ResultMsg = ex.ToString();
            }
            return scheduleResult;
        }


        /// <summary>
        /// 删除指定的任务
        /// </summary>
        /// <param name="jobName">任务名称</param>
        /// <param name="jobGroup">任务组</param>
        /// <returns></returns>
        public async Task<ScheduleResult> DeleteJobAsync(string jobName, string jobGroup)
        {
            ScheduleResult result = new ScheduleResult();
            try
            {
                JobKey jobKey = new JobKey(jobName, jobGroup);

                if (await Scheduler.CheckExists(jobKey))
                {
                    //先暂停，再移除
                    await Scheduler.PauseJob(jobKey);
                    await Scheduler.DeleteJob(jobKey);
                }
                else
                {
                    result.ResultCode = -1;
                    result.ResultMsg = "任务不存在";
                }

            }
            catch (Exception ex)
            {
                _logger.Error(nameof(AddJobAsync), ex,null);
                result.ResultCode = -4;
                result.ResultMsg = ex.ToString();
            }
            return result;
        }
    }
}
