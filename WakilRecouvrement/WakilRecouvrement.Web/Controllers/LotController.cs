using System;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Mvc;
using WakilRecouvrement.Service;
using WakilRecouvrement.Domain.Entities;
using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Linq.Dynamic;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;
using Microsoft.Ajax.Utilities;

namespace WakilRecouvrement.Web.Controllers
{
    public class LotController : Controller
    {

        EmployeService EmpService;
        LotService LotService;
        public int x = 0;
        public int dup = 0;
        public int up = 0;


        public LotController()
        {
            EmpService = new EmployeService();
            LotService = new LotService();
        }

        public ActionResult ImportLot()
        {
   
            return View();

        }

        [HttpPost]
        public ActionResult UploadExcel(HttpPostedFileBase PostedFile)
        {
            //Nthabtou li fichier mahouch feragh makenesh nabaathou erreur lel client
            if (PostedFile != null )
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
                
                //nekhdhou num mtaa lot men esm l fichier ex: Lot 11 => 11 
                string numLot = string.Join(string.Empty, Regex.Matches(PostedFile.FileName, @"\d+").OfType<Match>().Select(m => m.Value));

                //Nthaabtou eli l client selectiona fichier excel moush haja okhra makanesh nabaathoulou erreur
                if (PostedFile.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" )
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

                                    //On verifier aala koll colonne eli l fichier excel il respecte bien le syntaxe mtaa koll colonne bel try/catch 
                                    //O nsobou l information/donné fi variable
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
                                        NumLot= numLot,
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

                                    };

                                    //On verifie est ce que l client hedha aana menou fel base ouala le
                                    if(LotService.GetClientByIDClient(Lot.IDClient)!=null)
                                    {
                                        //Ken aana menou nshoufou ken identique ouala le
                                        //ken identique manaamlou chay 
                                        //ken aaana menou ama fama difference => donc nhotouh a jour
                                        //sinon ken maanesh menou on l'ajouter fel base
                                        //Benesba l ViewData rahom des flag lel affichage 
                                        Lot lot = LotService.GetClientByIDClient(Lot.IDClient);
                                        
                                        if(lot.Equals(Lot))
                                        {
                                            dup++;
                                            ViewData["nbDup"] = dup;
                                            if (dup > 0)
                                            {
                                                ViewData["dup"] = "1";
                                            }
                                        }
                                        else
                                        {
                                            up++;
                                            ViewData["nbUp"] = up;
                                            if (up > 0)
                                            {
                                                ViewData["up"] = "1";
                                            }
                                            lot.NumLot = Lot.NumLot;
                                            lot.NomClient = Lot.NomClient;
                                            lot.TelPortable = Lot.TelPortable;
                                            lot.TelFixe = Lot.TelFixe;
                                            lot.Fax = Lot.Fax;
                                            lot.SoldeDebiteur = Lot.SoldeDebiteur;
                                            lot.DescIndustry = Lot.DescIndustry;
                                            lot.Adresse = Lot.Adresse;
                                            lot.Type = Lot.Type;
                                            lot.Numero = Lot.Numero;
                                            lot.PostCode = Lot.PostCode;

                                            LotService.Update(lot);
                                        }

                                    }
                                    else
                                    {
                                        x++;
                                        ViewData["nb"] = x;
                                        
                                        if(x>0)
                                        {
                                            ViewData["noDup"] = "1";
                                        }
                                        LotService.Add(Lot);

                                    }

                                }

                                ViewData["nbTotal"] = dt.Rows.Count;
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


        
        public ActionResult ConsulterClients()
        {

            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            return View();
        }


        public IEnumerable<SelectListItem> NumLotListForDropDown()
        {
           
            List<Lot> Lots = LotService.GetAll().ToList();
            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Touts les lots", Value = "0" });

            Lots.DistinctBy(l => l.NumLot).ForEach(l => {
                listItems.Add(new SelectListItem { Text = "Lot " + l.NumLot, Value = l.NumLot });
            });

            return listItems;
        }

        [HttpPost]
        public ActionResult LoadData(string numLot)
        {

            List<Lot> Lots = new List<Lot>();

            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");

            if(numLot == "0")
            {
                Lots = LotService.GetAll().ToList();
            }
            else
            {
                Lots = LotService.GetAll().ToList().Where(l => l.NumLot.Equals(numLot)).ToList();
            } 

            JsonResult result = new JsonResult();

            try
            {

                string search = Request.Form.GetValues("search[value]")[0];
                string draw = Request.Form.GetValues("draw")[0];
                string order = Request.Form.GetValues("order[0][column]")[0];
                string orderDir = Request.Form.GetValues("order[0][dir]")[0];
                int startRec = Convert.ToInt32(Request.Form.GetValues("start")[0]);
                int pageSize = Convert.ToInt32(Request.Form.GetValues("length")[0]);
               


                int totalRecords = Lots.Count;
                
                if (!string.IsNullOrEmpty(search) &&
                    !string.IsNullOrWhiteSpace(search))
                {
                    Lots = Lots.Where(l =>

                        l.Numero.ToString().Contains(search)
                    ||  l.Adresse.ToString().ToLower().Contains(search.ToLower())
                    ||  l.TelFixe.ToString().Contains(search)
                    ||  l.TelPortable.ToString().Contains(search)
                    ||  l.IDClient.ToString().Contains(search)
                    ||  l.Compte.ToString().Contains(search)
                    ||  l.LotId.ToString().Contains(search)  
                    ||  l.NomClient.ToString().ToLower().Contains(search.ToLower()) 
                    ||  l.DescIndustry.ToString().ToLower().Contains(search.ToLower()) 
                  
                        ).ToList();
                }


                Lots = SortTableData(order, orderDir, Lots);
               
                int recFilter = Lots.Count;

                Lots = Lots.Skip(startRec).Take(pageSize).ToList();
                var modifiedData = Lots.Select(l =>
                   new
                   {
                       l.LotId,
                       l.NumLot,
                       l.Compte,
                       l.IDClient,
                       l.NomClient,
                       l.TelPortable,
                       l.TelFixe,
                       l.SoldeDebiteur,
                       l.DescIndustry,
                       l.Adresse,
                       l.Type,
                       l.Numero
                   }
                   );
                result = this.Json(new
                {
                    draw = Convert.ToInt32(draw),
                    recordsTotal = totalRecords,
                    recordsFiltered = recFilter,
                    data = modifiedData
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }

            return result;

        }

        private List<Lot> SortTableData(string order, string orderDir, List<Lot> data)
        {
            List<Lot> lst = new List<Lot>();
            try
            {
                switch (order)
                {
                    case "0":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => l.LotId).ToList()
                                                                                                 : data.OrderBy(l => l.LotId).ToList();
                        break;

                    case "1":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l =>long.Parse(l.NumLot)).ToList()
                                                                                                 : data.OrderBy(l => long.Parse(l.NumLot)).ToList();
                        break;
                    case "2":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => l.Compte).ToList()
                                                                                                 : data.OrderBy(l => l.Compte).ToList();
                        break;
                    case "3":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => long.Parse(l.IDClient)).ToList()
                                                                                                 : data.OrderBy(l => long.Parse(l.IDClient)).ToList();
                        break;
                    case "4":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => l.NomClient).ToList()
                                                                                                 : data.OrderBy(l => l.NomClient).ToList();
                        break;
                    case "5":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => l.TelPortable).ToList()
                                                                                                   : data.OrderBy(l => l.TelPortable).ToList();
                        break;
                    case "6":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => l.TelFixe).ToList()
                                                                                                   : data.OrderBy(l => l.TelFixe).ToList();
                        break;
                    case "7":
                       
                            lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => Double.Parse(l.SoldeDebiteur)).ToList()
                                                                                                  : data.OrderBy(l => Double.Parse(l.SoldeDebiteur)).ToList();

                        break;
                    case "8":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => l.DescIndustry).ToList()
                                                                                                   : data.OrderBy(l => l.DescIndustry).ToList();
                        break;
                    case "9":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => l.Adresse).ToList()
                                                                                                   : data.OrderBy(l => l.Adresse).ToList();
                        break;
                    case "10":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => l.Type).ToList()
                                                                                                   : data.OrderBy(l => l.Type).ToList();
                        break;

                    case "11":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => l.Numero).ToList()
                                                                                                   : data.OrderBy(l => l.Numero).ToList();
                        break;

                    default:
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => l.LotId).ToList()
                                                                                                 : data.OrderBy(l => l.LotId).ToList();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return lst;
        }
    }


}

