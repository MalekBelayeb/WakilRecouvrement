using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace WakilRecouvrement.Web.Job
{
    public class JobScheduler
    {

        private static readonly string ScheduleCronExpression = ConfigurationManager.AppSettings["ExecuteTaskScheduleCronExpression"];

        public static async Task StartAsync()
        {

            try
            {
                var scheduler = await StdSchedulerFactory.GetDefaultScheduler();

                if(!scheduler.IsStarted)
                {
                    await scheduler.Start();
                }

                var job = JobBuilder.Create<MyJob>()
                    .WithIdentity("MyJob1", "group1").Build();


                var trigger = TriggerBuilder.Create()
                    .WithIdentity("MyTrigger1", "group1")
                    .WithCronSchedule(ScheduleCronExpression)
                    .Build();

                await scheduler.ScheduleJob(job, trigger);

            }catch(Exception ex)
            {
                    


            }

        }

    }
}