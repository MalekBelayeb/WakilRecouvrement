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
using System.Linq;
using System.Linq.Dynamic;
using System.Collections.Generic;
using Microsoft.Ajax.Utilities;
using WakilRecouvrement.Web.Models.ViewModel;
using PagedList;
using WakilRecouvrement.Web.Models;
using WakilRecouvrement.Data;
using MyFinance.Data.Infrastructure;

namespace WakilRecouvrement.Web.Controllers
{
    public class LotController : Controller
    {

        public int x = 0;
        public int dup = 0;
        public int up = 0;


        public LotController()
        {
         
        
        }

        public ActionResult ImportLot()
        {
   
            return View();

        }

        [HttpPost]
        public ActionResult UploadExcel(HttpPostedFileBase PostedFile)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                  

                    string filename = "";
                    //Nthabtou li fichier mahouch feragh makenesh nabaathou erreur lel client
                    if (PostedFile != null)
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

                                                ModelState.AddModelError("Importer", "La colonne " + argId + " n'appartient pas à la table Excel, verifier les espaces cachés et les majuscules.");
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
                                                NumLot = numLot,
                                                IDClient = IdClient,
                                                Compte = Compte,
                                                NomClient = NomClient,
                                                SoldeDebiteur = CompteDebiteur.Replace("-", ""),
                                                TelPortable = TelPortable,
                                                TelFixe = TelFixe,
                                                DescIndustry = Agence,
                                                Adresse = Adresse,
                                                Type = Type,
                                                Numero = Numero,

                                            };

                                            //On verifie est ce que l client hedha aana menou fel base ouala le
                                            if (LotService.Get(l => l.Compte.Equals(Compte)) != null)
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

                                                if (x > 0)
                                                {
                                                    ViewData["noDup"] = "1";
                                                }
                                                LotService.Add(Lot);
                                                //AffecterClient(Lot, filename);

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
            }
          
        }

        public ActionResult deleteLot(int id, string currentFilter, string currentNumLot, string currentPage)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {
                 
                    LotService LotService = new LotService(UOW);

                    LotService.Delete(LotService.GetById(id));
                    LotService.Commit();

                    return RedirectToAction("ConsulterClients", "Lot", new { currentFilter, currentNumLot, currentPage });

                }
            }
        }

        public ActionResult ConsulterClients(string SearchString,string numLot, string currentFilter, string currentNumLot, string currentPage,string CurrentSort, string sortOrder,int? page)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {
                   LotService LotService = new LotService(UOW);
                   

                    ViewData["list"] = new SelectList(NumLotAllListForDropDown(), "Value", "Text");
                    ViewData["sortOrder"] = new SelectList(SortOrdrForDropDown(), "Value", "Text");

                    List<SuiviAffectationViewModel> JoinedList = new List<SuiviAffectationViewModel>();


                    if (sortOrder != null)
                    {
                        //page = 1;
                    }
                    else
                    {
                        sortOrder = CurrentSort;
                    }


                    ViewBag.CurrentSort = sortOrder;


                    if (page == null)
                    {
                        if (currentPage != null)
                            page = int.Parse(currentPage);
                    }

                    ViewBag.page = page;

                    if (SearchString != null)
                    {
                        // page = 1;
                    }
                    else
                    {
                        SearchString = currentFilter;
                    }

                    ViewBag.currentFilter = SearchString;

                    if (numLot != null)
                    {
                        //page = 1;
                    }
                    else
                    {
                        numLot = currentNumLot;
                    }

                    ViewBag.currentNumLot = numLot;


                    JoinedList = (from a in LotService.GetAll()

                                  select new SuiviAffectationViewModel
                                  {
                                      LotId = a.LotId
                                      ,
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
                        if (numLot.Equals("0") == false)
                            JoinedList = JoinedList.Where(j => j.NumLot.Equals(numLot)).ToList();

                    }

                    if (!String.IsNullOrEmpty(SearchString))
                    {
                        JoinedList = JoinedList.Where(s => s.Adresse.IfNullOrWhiteSpace("").ToLower().Contains(SearchString.ToLower())
                                               || s.Compte.IfNullOrWhiteSpace("").ToLower().Contains(SearchString.ToLower())
                                               || s.DescIndustry.IfNullOrWhiteSpace("").ToLower().Contains(SearchString.ToLower())
                                               || s.IDClient.IfNullOrWhiteSpace("").ToLower().Contains(SearchString.ToLower())
                                               || s.NomClient.IfNullOrWhiteSpace("").ToLower().Contains(SearchString.ToLower())
                                               || s.Numero.IfNullOrWhiteSpace("").ToLower().Contains(SearchString.ToLower())
                                               || s.SoldeDebiteur.IfNullOrWhiteSpace("").ToLower().Contains(SearchString.ToLower())
                                               || s.TelFixe.IfNullOrWhiteSpace("").ToLower().Contains(SearchString.ToLower())
                                               || s.TelPortable.IfNullOrWhiteSpace("").ToLower().Contains(SearchString.ToLower())

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
                    return View(JoinedList.ToPagedList(pageNumber, pageSize));

                }
            }





        }


        public IEnumerable<SelectListItem> NumLotListForDropDown()
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {
                    LotService LotService = new LotService(UOW);

                    List<Lot> Lots = LotService.GetAll().ToList();
                    List<SelectListItem> listItems = new List<SelectListItem>();


                    Lots.DistinctBy(l => l.NumLot).ForEach(l => {
                        listItems.Add(new SelectListItem { Text = "Lot " + l.NumLot, Value = l.NumLot });
                    });

                    return listItems;
                }
            }
                   
        }


        public IEnumerable<SelectListItem> SortOrdrForDropDown()
        {

            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Nom (A-Z)", Value = "0" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde debiteur (o. decroissant)", Value = "1" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde debiteur (o. croissant)", Value = "2" });


            return listItems;
        }

        public ActionResult updateLot(int id)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {
                    LotService LotService = new LotService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);


                    ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
                    ViewData["agent"] = new SelectList(AgentListForDropDown(), "Value", "Text");

                    //Lot lot = LotService.GetById(id);
                    ClientAffecteViewModel cavm = new ClientAffecteViewModel();
                    cavm = (from a in AffectationService.GetAll()
                            join l in LotService.GetAll() on a.LotId equals l.LotId
                            where l.LotId == id
                            select new ClientAffecteViewModel
                            {
                                Affectation = a,
                                Lot = l
                            }).FirstOrDefault();

                    if (cavm == null)
                    {
                        cavm = new ClientAffecteViewModel();
                        Lot lot = LotService.GetById(id);
                        cavm.Lot = lot;
                    }

                    return View(cavm);
                }
            }



        }


        [HttpPost]
        public ActionResult updateLot(ClientAffecteViewModel cavm)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {
                    LotService LotService = new LotService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                 
                    Lot newlot = LotService.GetById(cavm.Lot.LotId);
                    newlot.Adresse = cavm.Lot.Adresse;
                    newlot.Compte = cavm.Lot.Compte;
                    newlot.DescIndustry = cavm.Lot.DescIndustry;
                    newlot.Fax = cavm.Lot.Fax;
                    newlot.IDClient = cavm.Lot.IDClient;
                    newlot.NomClient = cavm.Lot.NomClient;
                    newlot.NumLot = cavm.Lot.NumLot;
                    newlot.PostCode = cavm.Lot.PostCode;
                    newlot.SoldeDebiteur = cavm.Lot.SoldeDebiteur;
                    newlot.TelFixe = cavm.Lot.TelFixe;
                    newlot.TelPortable = cavm.Lot.TelPortable;
                    newlot.Type = cavm.Lot.Type;

                    LotService.Update(newlot);
                    LotService.Commit();


                    Affectation affectation = AffectationService.GetById(cavm.Affectation.AffectationId);
                    Debug.WriteLine(cavm.Affectation.EmployeId);

                    if (affectation == null)
                    {
                        Affectation aff = new Affectation
                        {
                            EmployeId = cavm.Affectation.EmployeId,
                            LotId = cavm.Lot.LotId,
                            DateAffectation = DateTime.Now

                        };

                        AffectationService.Add(aff);
                        AffectationService.Commit();


                    }
                    else
                    {

                        affectation.EmployeId = cavm.Affectation.EmployeId;
                        affectation.LotId = cavm.Lot.LotId;
                        affectation.DateAffectation = DateTime.Now;
                        AffectationService.Update(affectation);
                        AffectationService.Commit();

                    }


                    return RedirectToAction("ConsulterClients", new { numLot = 0, sortOrder = 0 });


                }
            }



          
        }


        public ActionResult addLot()
        {
            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            ViewBag.AgentList = new SelectList(AgentListForDropDown(), "Value", "Text");

            return View();
        }

        [HttpPost]
        public ActionResult addLot(Lot lot,int agent)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {
                    LotService LotService = new LotService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    Affectation affectation = new Affectation
                    {
                        DateAffectation = DateTime.Now,
                        LotId = lot.LotId,
                        EmployeId = agent
                    };

                    AffectationService.Add(affectation);
                    AffectationService.Commit();

                    return RedirectToAction("ConsulterClients", new { numLot = 0, sortOrder = 0 });
                }
            }

          
        }

        public IEnumerable<SelectListItem> AgentListForDropDown()
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    EmployeService EmpService = new EmployeService(UOW);

                    List<Employe> agents = EmpService.GetMany(emp => emp.Role.role.Equals("agent") && emp.IsVerified == true).ToList();
                    List<SelectListItem> listItems = new List<SelectListItem>();

                    agents.ForEach(l => {
                        listItems.Add(new SelectListItem { Text = l.Username, Value = l.EmployeId + "" });
                    });

                    return listItems;

                }
            }
                    
        }
        public IEnumerable<SelectListItem> NumLotAllListForDropDown()
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    List<Lot> Lots = LotService.GetAll().ToList();
                    List<SelectListItem> listItems = new List<SelectListItem>();

                    listItems.Add(new SelectListItem { Selected = true, Text = "Tous les lots", Value = "0" });

                    Lots.DistinctBy(l => l.NumLot).ForEach(l => {
                        listItems.Add(new SelectListItem { Text = "Lot " + l.NumLot, Value = l.NumLot });
                    });

                    return listItems;
                }
            }
            
        }
    }



}

