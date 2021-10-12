using MyFinance.Data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WakilRecouvrement.Data;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Service;
using WakilRecouvrement.Web.Models;
namespace WakilRecouvrement.Web.Controllers
{
    public class ValiderTraitementVerifieFromExcelController : Controller
    {
        [HttpPost]
        public ActionResult UploadVerifier(HttpPostedFileBase PostedFile)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);

                    if (Session["username"] == null || Session["username"].ToString().Length < 1)
                        return RedirectToAction("Login", "Authentification");

                    ViewData["list"] = new SelectList(DropdownListController.NumLotListForDropDown(LotService), "Value", "Text");
                    ViewBag.TraiteList = new SelectList(DropdownListController.TraiteValidationListForDropDown(), "Value", "Text");
                    ViewBag.AgentList = new SelectList(DropdownListController.AgentListForDropDown(EmpService), "Value", "Text");

                    //Nthabtou li fichier mahouch feragh makenesh nabaathou erreur lel client
                    if (PostedFile != null)
                    {
                        //nsobou l fichier aana fel serveur
                        string filePath = string.Empty;
                        string path = Server.MapPath("~/Uploads/");
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        filePath = path + Path.GetFileName(PostedFile.FileName);
                        string extension = Path.GetExtension(PostedFile.FileName);
                        string conString = string.Empty;

                        if (PostedFile.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                        {
                            PostedFile.SaveAs(filePath);

                            //besh nakhtarou connectionString selon l version mtaa excel (xls = 2003 o xlsx mel 2013 o ahna tal3in)
                            //L connectionString predefini fel web.config mteena
                            switch (extension)
                            {
                                case ".xls":
                                    conString = ConfigurationManager.ConnectionStrings["Excel03ConString"].ConnectionString;
                                    break;

                                case ".xlsx":
                                    conString = ConfigurationManager.ConnectionStrings["Excel07ConString"].ConnectionString;
                                    break;

                            }

                            //Taoua besh nebdew nakraw l fichier Excel bel library OleDd
                            DataTable dt = new DataTable();
                            conString = string.Format(conString, filePath);
                            string status = "";

                            using (OleDbConnection connExcel = new OleDbConnection(conString))
                            {
                                using (OleDbCommand cmdExcel = new OleDbCommand())
                                {
                                    using (OleDbDataAdapter odaExcel = new OleDbDataAdapter())
                                    {
                                        // Houni nebdew naakraw awel sheet name mtaa l document excel mteena (eli ken jina fi table SQL rahou le nom de la table) 
                                        cmdExcel.Connection = connExcel;
                                        connExcel.Open();
                                        DataTable dtExcelSchema;
                                        dtExcelSchema = connExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                                        string sheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
                                        connExcel.Close();

                                        //Houni recuperation mtaa les données 
                                        connExcel.Open();
                                        cmdExcel.CommandText = "SELECT * FROM [Table1$]";
                                        odaExcel.SelectCommand = cmdExcel;
                                        odaExcel.Fill(dt);
                                        connExcel.Close();

                                        string argIDClient = "IDClient";
                                        string argCompte = "Compte";
                                        string argNomClient = "NomClient";
                                        string argMontant = "Montant";
                                        int rejete = 0;
                                        int verifier = 0;
                                        foreach (DataRow row in dt.Rows)
                                        {
                                            string IDClient = "";
                                            string Compte = "";
                                            string NomClient = "";
                                            string Montant = "";


                                            IDClient = row[argIDClient].ToString();
                                            Compte = row[argCompte].ToString();
                                            NomClient = row[argNomClient].ToString();
                                            Montant = row[argMontant].ToString();

                                           int result = VerifyClient(IDClient, Montant, FormulaireService, AffectationService, LotService);
                                        
                                            if(result == 1 )
                                            {
                                                verifier++;

                                            }else if (result == 2)
                                            {
                                                rejete++;
                                            }
                                            else
                                            {

                                            }

                                        }

                                        FormulaireService.Commit();
                                        status = "Opération terminée avec " + verifier + " client(s) vérifiés et " + rejete + " client(s) rejetés";

                                    }
                                }


                                return RedirectToAction("Valider", "ValiderTraitement", new { messageFromExcelVerifier = status });

                            }

                        }
                        else
                        {

                            return RedirectToAction("Valider", "ValiderTraitement",new { messageFromExcelVerifier = "Le fichier selectionné n'est pas un fichier Excel" });

                        }

                    }
                    else
                    {
                       
                        return RedirectToAction("Valider", "ValiderTraitement", new { messageFromExcelVerifier = "Vous devez sélectionner un fichier" });
                    }

                }
            }
        }


        public int VerifyClient(string idclient, string montant, FormulaireService FormulaireService, AffectationService AffectationService, LotService LotService)
        {
            // 1 for verifier 2 for rejeter else 0 
            int result = 0;

            var formulaireAtraite = (from f in FormulaireService.GetAll()
                                     join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                     join l in LotService.GetAll() on a.LotId equals l.LotId
                                     where l.IDClient == idclient && f.EtatClient == Note.A_VERIFIE && f.Status == Status.EN_COURS
                                     select new ClientAffecteViewModel
                                     {
                                         Formulaire = f,
                                         Affectation = a,
                                         Lot = l
                                     }).ToList().FirstOrDefault();

            if(formulaireAtraite != null)
            {
                if(String.IsNullOrEmpty(montant))
                {
                    formulaireAtraite.Formulaire.Status = Status.NON_VERIFIE;
                    FormulaireService.Update(formulaireAtraite.Formulaire);
                    result = 2;
                }
                else
                {

                    if(double.TryParse(montant.Replace('.', ','), out double mont))
                    {
                        formulaireAtraite.Formulaire.MontantVerseDeclare = mont;

                        Decimal NewSolde = 0;
                        NewSolde = Decimal.Subtract(decimal.Parse(formulaireAtraite.Formulaire.MontantDebMAJ.ToString()), decimal.Parse(formulaireAtraite.Formulaire.MontantVerseDeclare.ToString()));
                        double DebMaJ = GetFormulaire(formulaireAtraite.Formulaire.AffectationId, FormulaireService, AffectationService).MontantDebMAJ;
                        NewSolde = Decimal.Subtract(decimal.Parse(DebMaJ.ToString()), decimal.Parse(formulaireAtraite.Formulaire.MontantVerseDeclare.ToString()));

                        if (NewSolde <= 0)
                        {

                            formulaireAtraite.Formulaire.MontantDebMAJ = 0;

                            formulaireAtraite.Formulaire.Status = Status.VERIFIE;
                            formulaireAtraite.Formulaire.VerifieLe = DateTime.Now;
                            formulaireAtraite.Formulaire.EtatClient = Note.SOLDE;

                        }
                        else if (NewSolde > 0)
                        {

                            formulaireAtraite.Formulaire.MontantDebMAJ = double.Parse(NewSolde.ToString());

                            formulaireAtraite.Formulaire.Status = Status.VERIFIE;
                            formulaireAtraite.Formulaire.VerifieLe = DateTime.Now;
                            formulaireAtraite.Formulaire.EtatClient = Note.SOLDE_TRANCHE;

                        }
                        FormulaireService.Update(formulaireAtraite.Formulaire);
                        result = 1;
                    }
                    else
                    {
                        formulaireAtraite.Formulaire.Status = Status.NON_VERIFIE;
                        FormulaireService.Update(formulaireAtraite.Formulaire);
                        result = 2;
                    }
                   
                }

            }
            else
            {
                result = 0;
            }


            return result;

        }
        public Formulaire GetFormulaire(int affId, FormulaireService FormulaireService, AffectationService AffectationService)
        {
            
            
            var forms = (from f in FormulaireService.GetAll()
                         join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                         where a.AffectationId == affId
                         orderby f.MontantDebMAJ ascending
                         select new ClientAffecteViewModel
                         {
                             Formulaire = f,

                         }).FirstOrDefault();


            return forms.Formulaire;


        }


    }
}