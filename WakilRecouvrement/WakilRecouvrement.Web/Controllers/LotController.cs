using System;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Mvc;
using WakilRecouvrement.Service;
using WakilRecouvrement.Web.Models;
using WakilRecouvrement.Domain.Entities;
using System.Net;
using System.Runtime.CompilerServices;

namespace WakilRecouvrement.Web.Controllers
{
    public class LotController : Controller
    {

        EmployeService EmpService;
        RoleService RoleService;
        LotService LotService;
       
        public int x = 0;
        public int dup = 0;
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
        public ActionResult UploadExcel(HttpPostedFileBase PostedFile)
        {

            if (PostedFile != null )
            {

                string filePath = string.Empty;
                string path = Server.MapPath("~/Uploads/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                filePath = path + Path.GetFileName(PostedFile.FileName);
                string extension = Path.GetExtension(PostedFile.FileName);
                string conString = string.Empty;


                if (PostedFile.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" )
                {
                    PostedFile.SaveAs(filePath);

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


                                //string argNumLot = dt.Columns[0].ColumnName;
                                string argId = "ID";
                                string argCompte = "Compte";
                                string argNomClient = "Nom Client";
                                string argCompteDebiteur = "Compte Debiteur";
                                string argTelPortable = "Tel Portable";
                                string argTelFixe = "Tel Fixe";
                                string argAgence = "Agence";
                                string argAdresse = "Adresse";
                                string argType = "Type";
                                string argNumero = "Numero";

                                Employe emp = EmpService.GetEmployeByUername(Session["username"].ToString());
                                
                                foreach (DataRow row in dt.Rows)
                                {
                                    string IdClient = "";
                                    string NomClient = "";
                                    string Compte = "";
                                    string CompteDebiteur = "";
                                    string TelPortable = "";
                                    string TelFixe = "";
                                    string Agence = "";
                                    string Adresse = "";
                                    string Type = "";
                                    string Numero = "";

                                    try
                                    {
                                         IdClient = row[argId].ToString();
                                    }
                                    catch (ArgumentException e)
                                    {
                                        ModelState.AddModelError("Importer", "La colonne "+ argId + " n'appartient pas à la table Excel, verifier les espaces cachés et les majuscules.");
                                        return View("ImportLot");

                                    }

                                    try
                                    {
                                        NomClient = row[argNomClient].ToString();
                                    }
                                    catch (ArgumentException e)
                                    {
                                        ModelState.AddModelError("Importer", "La colonne " + argNomClient + " n'appartient pas à la table Excel, verifier les espaces cachés et les majuscules.");
                                        return View("ImportLot");

                                    }

                                    try
                                    {
                                         Compte = row[argCompte].ToString();
                                    }
                                    catch (ArgumentException e)
                                    {
                                        ModelState.AddModelError("Importer", "La colonne " + argCompte + " n'appartient pas à la table Excel, verifier les espaces cachés et les majuscules.");
                                        return View("ImportLot");
                                    }

                                    try
                                    {
                                        CompteDebiteur = row[argCompteDebiteur].ToString();
                                    }
                                    catch (ArgumentException e)
                                    {
                                        ModelState.AddModelError("Importer", "La colonne " + argCompteDebiteur + " n'appartient pas à la table Excel, verifier les espaces cachés et les majuscules.");
                                        return View("ImportLot");

                                    }

                                    try
                                    {
                                        TelPortable = row[argTelPortable].ToString();
                                    }
                                    catch (ArgumentException e)
                                    {
                                        ModelState.AddModelError("Importer", "La colonne " + argTelPortable + " n'appartient pas à la table Excel, verifier les espaces cachés et les majuscules.");
                                        return View("ImportLot");
                                    }

                                    try
                                    {
                                        TelFixe = row[argTelFixe].ToString();
                                    }
                                    catch (ArgumentException e)
                                    {
                                        ModelState.AddModelError("Importer", "La colonne " + argTelFixe + " n'appartient pas à la table Excel, verifier les espaces cachés et les majuscules.");
                                        return View("ImportLot");

                                    }

                                    try
                                    {
                                        Agence = row[argAgence].ToString();
                                    }
                                    catch (ArgumentException e)
                                    {
                                        ModelState.AddModelError("Importer", "La colonne " + argAgence + " n'appartient pas à la table Excel, verifier les espaces cachés et les majuscules.");
                                        return View("ImportLot");
                                    }

                                    try
                                    {
                                        Adresse = row[argAdresse].ToString();
                                    }
                                    catch (ArgumentException e)
                                    {
                                        ModelState.AddModelError("Importer", "La colonne " + argAdresse + " n'appartient pas à la table Excel, verifier les espaces cachés et les majuscules.");
                                        return View("ImportLot");
                                    }

                                    try
                                    {
                                        Type = row[argType].ToString();
                                    }
                                    catch (ArgumentException e)
                                    {
                                        ModelState.AddModelError("Importer", "La colonne " + argType + " n'appartient pas à la table Excel, verifier les espaces cachés et les majuscules.");
                                        return View("ImportLot");

                                    }

                                    try
                                    {
                                        Numero = row[argNumero].ToString();
                                    }
                                    catch (ArgumentException e)
                                    {
                                        ModelState.AddModelError("Importer", "La colonne " + argNumero + " n'appartient pas à la table Excel, verifier les espaces cachés et les majuscules.");
                                        return View("ImportLot");

                                    }



                                    Lot Lot = new Lot()
                                    {

                                        IDClient = IdClient,
                                        Compte = Compte,
                                        NomClient = NomClient,
                                        SoldeDebiteur = CompteDebiteur,
                                        TelPortable = TelPortable,
                                        TelFixe = TelFixe,
                                        DescIndustry = Agence,
                                        Adresse = Adresse,
                                        Type = Type,
                                        Numero = Numero,
                                        EmployeId = emp.EmployeId

                                    };


                                    if(LotService.GetClientByIDClient(Lot.IDClient)!=null)
                                    {
                                        ViewData["nbDup"] = dup++;
                                        if(dup>0)
                                        {
                                            ViewData["dup"] = "1";
                                        }


                                    }
                                    else
                                    {
                                        ViewData["nb"] = x++;
                                        
                                        if(x>0)
                                        {
                                            ViewData["noDup"] = "1";
                                        }
                                        LotService.Add(Lot);

                                    }


                                }
                                ViewData["nbTotal"] = dt.Rows.Count-1;
                                ViewData["finished"] = "1";
                                LotService.Commit();

                            }
                        }
                    }

                }
                else
                {
                    ModelState.AddModelError("Importer", "Le fichier selectionné n'est pas un fichier Excel");
                }

            }
            else
            {
                ModelState.AddModelError("Importer", "Vous devez sélectionner un fichier");
            }

            return View("ImportLot");

        }


    }
}
