using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WakilRecouvrement.Web.Hubs;

namespace WakilRecouvrement.Web.Controllers
{
    public class NotificationController : Controller
    {
        // GET: Notification
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Get()
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["WRConnectionStrings"].ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(@"SELECT [NotificationId],[ToSingle],[ToRole],[Message],[AffectationId] FROM [dbo].[Notifications]", connection))
                {
                    // Make sure the command object does not already have
                    // a notification object associated with it.
                    command.Notification = null;

                    SqlDependency dependency = new SqlDependency(command);
                    dependency.OnChange += new OnChangeEventHandler(dependency_OnChange);

                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    SqlDataReader reader = command.ExecuteReader();

                    var listCus = reader.Cast<IDataRecord>()
                            .Select(x => new
                            {
                                NotificationId = (int)x["NotificationId"],
                                ToSingle = (string)x["ToSingle"],
                                Message = (string)x["Message"],
                                AffectationId = (int)x["AffectationId"],
                            }).ToList();

                    return Json(new { listCus = listCus }, JsonRequestBehavior.AllowGet);

                }
            }
        }

        private void dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            UserHub.Show();
        }
    }
}