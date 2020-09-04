using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Web;
using System.Web.Mvc;
using WakilRecouvrement.Service;
using WakilRecouvrement.Web.Models;

namespace WakilRecouvrement.Web.Controllers
{
    public class LotController : Controller
    {

        EmployeService EmpService;
        RoleService RoleService;
        LotService LotService;
        public LotController()
        {
            EmpService = new EmployeService();
            RoleService = new RoleService();
            LotService = new LotService();
        }



        public ActionResult ImportLot()
        {

            return View();

        }

        [HttpPost]
        public ActionResult UploadExcel(LotViewModel lotVM, HttpPostedFileBase PostedFile)
        {

            string filePath = string.Empty;

            if (PostedFile != null)
            {
                string path = Server.MapPath("~/Uploads/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                filePath = path + Path.GetFileName(PostedFile.FileName);
                string extension = Path.GetExtension(PostedFile.FileName);
                PostedFile.SaveAs(filePath);
                string conString = string.Empty;

                switch (extension)
                {
                    case ".xls":
                        conString = ConfigurationManager.ConnectionStrings["Excel03ConString"].ConnectionString;

                        break;

                    case ".xlsx":

                        conString = ConfigurationManager.ConnectionStrings["Excel07ConString"].ConnectionString;


                        break;

                }

                DataTable dt = new DataTable();
                conString = string.Format(conString, filePath);


                using (OleDbConnection connExcel = new OleDbConnection(conString))
                {
                    using (OleDbCommand cmdExcel = new OleDbCommand())
                    {
                        using (OleDbDataAdapter odaExcel = new OleDbDataAdapter())
                        {

                            cmdExcel.Connection = connExcel;
                            connExcel.Open();
                            DataTable dtExcelSchema;
                            dtExcelSchema = connExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                            string sheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
                            connExcel.Close();


                            connExcel.Open();
                            cmdExcel.CommandText = "SELECT * FROM [" + sheetName + "]";
                            odaExcel.SelectCommand = cmdExcel;
                            odaExcel.Fill(dt);
                            connExcel.Close();





                        }
                    }
                }



            }

            return View("ImportLot");

        }


        // GET: Lot
        public ActionResult Index()
        {
            return View();
        }

        // GET: Lot/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Lot/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Lot/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Lot/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Lot/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Lot/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Lot/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
