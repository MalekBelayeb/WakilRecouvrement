using System.Web.Mvc;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Service;
using WakilRecouvrement.Web.Models;

namespace WakilRecouvrement.Web.Controllers
{
    public class AuthentificationController : Controller
    {

        EmployeService EmpService;
        RoleService RoleService;

        public AuthentificationController()
        {
            EmpService = new EmployeService();
            RoleService = new RoleService();
        }

        public ActionResult Login()
        {
            Response.Write("<script>alert('ffff')</script>");
            return View("Login");
        }

        public ActionResult InscriptionCompte()
        {
            var Roles = RoleService.GetAll();
            ViewBag.RoleList = new SelectList(Roles, "RoleId", "role");

            return View("InscriptionCompte");
        }

        [HttpPost]
        public ActionResult Login(Compte compte)
        {
            Employe emp = EmpService.GetEmployeByUername(compte.Username);

            if (emp == null)
            {

                ModelState.AddModelError("Username", "Nom d'utilisateur inexistant");

            }
            else if (emp != null)
            {

                if (emp.Password == compte.Password)
                {

                    if (emp.IsVerified == false)
                    {

                        ModelState.AddModelError("Connect", "Votre compte n'est pas encore validé par un administrateur");

                    }
                    else if (emp.IsVerified == true)
                    {

                        Session["username"] = emp.Username;
                        Session["role"] = emp.Role.role;
                        return RedirectToAction("Index", "Home");

                    }

                }
                else
                {
                    ModelState.AddModelError("Password", "Mot de passe incorrect");
                }
            }

            return View();
        }


        [HttpPost]
        public ActionResult InscriptionCompte(Employe emp)
        {
            var Roles = RoleService.GetAll();
            ViewBag.RoleList = new SelectList(Roles, "RoleId", "role");

            if (EmpService.GetEmployeByUername(emp.Username) != null)
            {
                ModelState.AddModelError("Username", "Nom d'utilisateur existe deja !");
            }
            else
            {

                EmpService.Add(emp);
                EmpService.Commit();
                return View("Login");

            }

            //Response.Write("<script>alert('" + emp.RoleId + "')</script>");
            return View();
        }



        public ActionResult Deconnect()
        {

            Session.Clear();

            return RedirectToAction("Login");
        }




    }
}
