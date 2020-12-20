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
            return View("Login");
        }

        public ActionResult InscriptionCompte()
        {
            var Roles = RoleService.GetAll();
            ViewBag.RoleList = new SelectList(Roles, "RoleId", "role");

            return View("InscriptionCompte");
        }


        public bool IsValid(Employe emp)
        {

         
                if (emp.Password.Equals(emp.ConfirmPassword) && emp.RoleId !=0 )
                {
                    return true;
                }

            return false;

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
                return RedirectToAction("Login");

            }

            //Response.Write("<script>alert('" + emp.RoleId + "')</script>");
            return View();
        }



        public ActionResult ChangePassword()
        {
            if (Session["username"] == null || Session["username"].ToString().Length < 1)
                return RedirectToAction("Login", "Authentification");

            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(CompteConfigViewModel CompteCVM)
        {

            Employe e = EmpService.GetEmployeByUername(Session["username"].ToString());

            if(e.Password == CompteCVM.Password)
            {
                e.Password = CompteCVM.NewPassword;
                e.ConfirmPassword = CompteCVM.NewPassword;

                EmpService.Update(e);
                EmpService.Commit();
            }
            else
            {
                ModelState.AddModelError("Password", "Votre mot de passe actuel est incorrect");
            }

            return View();

        }


        public ActionResult Deconnect()
        {

            Session.Clear();

            return RedirectToAction("Login");
        }




    }
}
