using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Service;
using WakilRecouvrement.Web.Hubs;

namespace WakilRecouvrement.Web.Controllers
{
    public class NotificationController : Controller
    {

        //NotificationService NotificationService;

        /*
        public NotificationController()
        {
            NotificationService = new NotificationService();

        }
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
                using (SqlCommand command = new SqlCommand(@"SELECT [Status],[Type],[NotificationId],[From],[ToSingle],[ToRole],[Message],[FormulaireId] FROM [dbo].[Notifications]", connection))
                {
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
                                Message = (string)x["Message"],
                                FormulaireId = (int)x["FormulaireId"],
                                From = (string)x["From"],
                                Status = (string)x["Status"],
                                Type = (string)x["Type"],
                            }).ToList();

                    return Json(new { listCus = listCus }, JsonRequestBehavior.AllowGet);

                }
            }
        }

        [HttpPost]
        public JsonResult GetByRoleDestination(string role)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["WRConnectionStrings"].ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(@"SELECT [AddedIn],[Status],[Type],[NotificationId],[From],[ToSingle],[ToRole],[Message],[FormulaireId] FROM [dbo].[Notifications] WHERE ToRole = '" + role + "' ORDER BY AddedIn DESC ", connection))
                {
                    command.Notification = null;

                    SqlDependency dependency = new SqlDependency(command);

                    if(role.Equals("admin"))
                    {
                        dependency.OnChange += new OnChangeEventHandler(dependency_OnChangeAdminRole);
                    }
                    else if (role.Equals("agent"))
                    {
                        dependency.OnChange += new OnChangeEventHandler(dependency_OnChangeAgentRole);

                    }

                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    SqlDataReader reader = command.ExecuteReader();

                    var listCus = reader.Cast<IDataRecord>()
                            .Select(x => new
                            {
                                NotificationId = (int)x["NotificationId"],
                                Message = (string)x["Message"],
                                FormulaireId = (int)x["FormulaireId"],
                                From = (string)x["From"],
                                Type = (string)x["Type"],
                                Status = (string)x["Status"],
                                AddedIn = ((DateTime)x["AddedIn"]).ToString()
                            }).ToList();

                    return Json(new { listCus = listCus }, JsonRequestBehavior.AllowGet);

                }
            }
        }


        [HttpPost]
        public JsonResult GetBySingleDestination(string tosingle)
        {

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["WRConnectionStrings"].ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(@"SELECT [AddedIn],[Status],[Type],[NotificationId],[From],[ToSingle],[ToRole],[Message],[FormulaireId] FROM [dbo].[Notifications] WHERE ToSingle = '" + tosingle+ "' ORDER BY AddedIn DESC", connection))
                {
                    command.Notification = null;

                    SqlDependency dependency = new SqlDependency(command);
                    dependency.OnChange += new OnChangeEventHandler(dependency_OnChangeUser);

                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    SqlDataReader reader = command.ExecuteReader();

                    var listCus = reader.Cast<IDataRecord>()
                            .Select(x => new
                            {
                              
                                NotificationId = (int)x["NotificationId"],
                                Message = (string)x["Message"],
                                FormulaireId = (int)x["FormulaireId"],
                                From = (string)x["From"],
                                Status = (string)x["Status"],
                                Type = (string)x["Type"],
                                AddedIn = ((DateTime)x["AddedIn"]).ToString()

                            }).ToList();

                    return Json(new { listCus = listCus }, JsonRequestBehavior.AllowGet);

                }
            }
        }

        [HttpPost]
        public ActionResult MarkAsSeen(int id)
        {
            Notification notification = NotificationService.GetById(id);
            notification.Status = "SEEN";


            NotificationService.Update(notification);
            NotificationService.Commit();

            return Json(new { });
        }


        private void dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            UserHub.Show();
        }


        private void dependency_OnChangeAgentRole(object sender, SqlNotificationEventArgs e)
        {
            UserHub.ShowByAgentRole();
        }

        private void dependency_OnChangeAdminRole(object sender, SqlNotificationEventArgs e)
        {
            
            UserHub.ShowByAdminRole();

        }

        private void dependency_OnChangeUser(object sender, SqlNotificationEventArgs e)
        {
            UserHub.ShowByUser();
        }

        */
    }
}