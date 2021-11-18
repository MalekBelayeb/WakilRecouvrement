using MyFinance.Data.Infrastructure;
using System;
using System.Web.Mvc;
using WakilRecouvrement.Data;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Service;
using WakilRecouvrement.Web.Models;

namespace WakilRecouvrement.Web.Controllers
{
    public class AuthentificationController : Controller
    {


        public int id = 0;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Logger");

       
        public AuthentificationController()
        {
        }

        public ActionResult Login()
        {
            return View("Login");
        }

        public ActionResult InscriptionCompte()
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {


                    try
                    {

                        RoleService RoleService = new RoleService(UOW);

                        var Roles = RoleService.GetAll();
                        ViewBag.RoleList = new SelectList(Roles, "RoleId", "role");

                        RoleService.Dispose();

                        return View("InscriptionCompte");

                    }
                    catch(Exception e)
                    {

                        log.Error(e);
                        return View("~/Views/Shared/Error.cshtml", null);

                    }



                } 
            }
                 


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
            using(WakilRecouvContext WakilContext= new WakilRecouvContext() )
            {
                using(UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    try
                    {

                        EmployeService EmpService = new EmployeService(UOW);

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

                        EmpService.Dispose();

                        return View();


                    }
                    catch(Exception e)
                    {
                        log.Error(e);
                        return View("~/Views/Shared/Error.cshtml", null);
                    }

                }
            }


        }


        [HttpPost]
        public ActionResult InscriptionCompte(Employe emp)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    try
                    {


                        RoleService RoleService = new RoleService(UOW);
                        EmployeService EmpService = new EmployeService(UOW);

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

                        RoleService.Dispose();
                        EmpService.Dispose();

                        //Response.Write("<script>alert('" + emp.RoleId + "')</script>");
                        return View();

                    }
                    catch(Exception e)
                    {

                        log.Error(e);
                        return View("~/Views/Shared/Error.cshtml", null);
                    }


                }
            }



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

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {
                    try
                    {


                        RoleService RoleService = new RoleService(UOW);
                        EmployeService EmpService = new EmployeService(UOW);

                        Employe e = EmpService.GetEmployeByUername(Session["username"].ToString());

                        if (e.Password == CompteCVM.Password)
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

                        RoleService.Dispose();
                        EmpService.Dispose();

                        return View();

                    }
                    catch(Exception e)
                    {

                        log.Error(e);
                        return View("~/Views/Shared/Error.cshtml", null);

                    }


                }
            }

                    
            
        }


        public ActionResult Deconnect()
        {

            Session.Clear();

            return RedirectToAction("Login");
        }




    }
}
