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
using WakilRecouvrement.Web.Models.ViewModel;
using PagedList;

namespace WakilRecouvrement.Web.Controllers
{
    public class LotController : Controller
    {

        EmployeService EmpService;
        LotService LotService;
        AffectationService AffectationService;
        public int x = 0;
        public int dup = 0;
        public int up = 0;


        public LotController()
        {
            EmpService = new EmployeService();
            LotService = new LotService();
            AffectationService = new AffectationService();
        }

        public ActionResult ImportLot()
        {
   
            return View();

        }

        [HttpPost]
        public ActionResult UploadExcel(HttpPostedFileBase PostedFile)
        {


            string filename="";
            //Nthabtou li fichier mahouch feragh makenesh nabaathou erreur lel client
            if (PostedFile != null )
            {

                filename = PostedFile.FileName;
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
                //string numLot = string.Join(string.Empty, Regex.Matches(PostedFile.FileName, @"\d+").OfType<Match>().Select(m => m.Value));
                string numLot = filename.Split('_')[1];

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
                                        SoldeDebiteur = CompteDebiteur.Replace("-",""),
                                        TelPortable = TelPortable,
                                        TelFixe = TelFixe,
                                        DescIndustry = Agence,
                                        Adresse = Adresse,
                                        Type = Type,
                                        Numero = Numero,

                                    };

                                    //On verifie est ce que l client hedha aana menou fel base ouala le
                                    if(LotService.Get(l=>l.Compte.Equals(Compte))!=null)
                                    {
                                        //Ken aana menou nshoufou ken identique ouala le
                                        //ken identique manaamlou chay 
                                        //ken aaana menou ama fama difference => donc nhotouh a jour
                                        //sinon ken maanesh menou on l'ajouter fel base
                                        //Benesba l ViewData rahom des flag lel affichage 
                                        Lot lot = LotService.Get(l => l.Compte.Equals(Compte));

                                        Debug.WriteLine(Compte);

                                        if (lot.Equals(Lot))
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
                                        LotService.Commit();
                                        AffecterClient(Lot, filename);


                                    }


                                }

                                ViewData["nbTotal"] = dt.Rows.Count;
                                ViewData["finished"] = "1";

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


        
        public ActionResult ConsulterClients(string SearchString,string numLot,string currentFilter, string sortOrder,int? page)
        {


            ViewBag.CurrentSort = sortOrder;
 
            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            ViewData["sortOrder"] = new SelectList(SortOrdrForDropDown(), "Value", "Text");

            List<SuiviAffectationViewModel> JoinedList = new List<SuiviAffectationViewModel>();
            if (SearchString != null)
            {
                page = 1;
            }
            else
            {
                SearchString = currentFilter;
            }

            ViewBag.CurrentFilter = SearchString;


            JoinedList = (from a in LotService.GetAll()

                          select new SuiviAffectationViewModel
                          {
                              Adresse = a.Adresse
                             ,
                              Compte = a.Compte
                             ,
                              DescIndustry = a.DescIndustry
                             ,
                              Fax = a.Fax
                             ,
                              IDClient = a.IDClient

                             ,
                              NomClient = a.NomClient
                             ,
                              Numero = a.Numero
                             ,
                              NumLot = a.NumLot
                             ,
                              PostCode = a.PostCode
                             ,
                              SoldeDebiteur = a.SoldeDebiteur
                             ,
                              TelFixe = a.TelFixe
                             ,
                              TelPortable = a.TelPortable
                             ,
                              Type = a.Type

                          }).ToList();

            if (!String.IsNullOrEmpty(numLot))
            {
                if(numLot.Equals("0")== false)
                JoinedList = JoinedList.Where(j => j.NumLot.Equals(numLot)).ToList();

            }

            if (!String.IsNullOrEmpty(SearchString))
            {
                JoinedList = JoinedList.Where(s => s.Adresse.ToLower().Contains(SearchString.ToLower())
                                       || s.Compte.ToLower().Contains(SearchString.ToLower())
                                       || s.DescIndustry.ToLower().Contains(SearchString.ToLower())
                                       || s.IDClient.ToLower().Contains(SearchString.ToLower())
                                       || s.NomClient.ToLower().Contains(SearchString.ToLower())
                                       || s.Numero.ToLower().Contains(SearchString.ToLower())
                                       || s.SoldeDebiteur.ToLower().Contains(SearchString.ToLower())
                                       || s.TelFixe.ToLower().Contains(SearchString.ToLower())
                                       || s.TelPortable.ToLower().Contains(SearchString.ToLower())
                                                               
                                       ).ToList();
            }


            switch (sortOrder)
            {
                case "0":
                    JoinedList = JoinedList.OrderBy(s => s.NomClient).ToList();
                    break;
               case "1":
                    JoinedList = JoinedList.OrderByDescending(s => s.SoldeDebiteur).ToList();
                    break;
             
                case "2":
                    JoinedList = JoinedList.OrderBy(s => s.SoldeDebiteur).ToList();
                    break;
             
                default:
              
                    
                    break;
            }



            ViewBag.total = JoinedList.Count();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(JoinedList.ToPagedList(pageNumber,pageSize));
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


        public IEnumerable<SelectListItem> SortOrdrForDropDown()
        {

            List<Lot> Lots = LotService.GetAll().ToList();
            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Nom (A-Z)", Value = "0" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde debiteur (o. decroissant)", Value = "1" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde debiteur (o. croissant)", Value = "2" });


            return listItems;
        }

        public void AffecterClient(Lot lot, string filename)
        {
            string agent = filename.Split('_')[2].Split('.')[0];
            string numlot = filename.Split('_')[1];

            Employe emp = EmpService.GetEmployeByUername(agent);

            Affectation affectation = new Affectation
            {
                AffectePar = "",
                EmployeId = emp.EmployeId,
                DateAffectation = DateTime.Now,
                LotId = lot.LotId
            };

            AffectationService.Add(affectation);
            AffectationService.Commit();
            

        }





    }




}

