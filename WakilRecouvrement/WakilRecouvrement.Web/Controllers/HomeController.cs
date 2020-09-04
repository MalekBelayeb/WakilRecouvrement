using System.Web.Mvc;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Service;

namespace WakilRecouvrement.Web.Controllers
{
    public class HomeController : Controller
    {

        EmployeService EmpService;
        RoleService RoleService;

        public HomeController()
        {
            EmpService = new EmployeService();
            RoleService = new RoleService();
        }


        public ActionResult AccountList()
        {
            return View(EmpService.GetAll());
        }



        [HttpPost]
        public ActionResult AccountList(string type)
        {
            if (type == "0")
            {
                return View(EmpService.GetAll());
            }
            else if (type == "1")
            {
                return View(EmpService.GetEmployeByVerified(true));
            }
            else if (type == "2")
            {
                return View(EmpService.GetEmployeByVerified(false));
            }
            else
            {
                return View(EmpService.GetAll());
            }

        }


        public ActionResult UpdateAccount(int id)
        {
            var Roles = RoleService.GetAll();
            ViewBag.RoleList = new SelectList(Roles, "RoleId", "role");

            return View(EmpService.GetById(id));

        }

        [HttpPost]
        public ActionResult UpdateAccount(int id, Employe e)
        {
            var Roles = RoleService.GetAll();
            ViewBag.RoleList = new SelectList(Roles, "RoleId", "role");

            Employe emp = EmpService.GetById(id);
            emp.RoleId = e.RoleId;
            emp.IsVerified = e.IsVerified;
            emp.ConfirmPassword = emp.Password;

            EmpService.Update(emp);
            EmpService.Commit();

            return View("AccountList", EmpService.GetAll());
        }



        [HttpPost]
        public ActionResult GestionDesComptes()
        {
            return View();
        }

        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        // GET: Home/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Home/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Home/Create
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


        // GET: Home/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Home/Delete/5
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
