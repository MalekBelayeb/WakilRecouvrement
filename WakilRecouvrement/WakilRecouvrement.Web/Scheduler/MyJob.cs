using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace WakilRecouvrement.Web.Job
{
    public class MyJob : IJob
    {

        public static readonly string SchedulingStatus = ConfigurationManager.AppSettings["ExecuteTaskServiceCallSchedulingStatus"];
        public Task Execute(IJobExecutionContext context)
        {
            var task = Task.Run(() =>
            {
                if(SchedulingStatus.Equals("ON"))
                {
                    try
                    {

                        Debug.WriteLine("hiiiiiiiii");
                        
                    }
                    catch (Exception ex)
                    {

                    }


                }

            });


            return task;
        }
    }
}