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
using System.Text.RegularExpressions;

namespace WakilRecouvrement.Web.Controllers
{
    public class LotController : Controller
    {

        public int x = 0;
        public int dup = 0;
        public int up = 0;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Logger");

        protected override void OnException(ExceptionContext filterContext)
        {
            
            filterContext.ExceptionHandled = true;
            log.Error(filterContext.Exception);

        }

        public LotController()
        {
         
        
        }
        
        //13363

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
                        string numLot = string.Join(string.Empty, Regex.Matches(PostedFile.FileName, @"\d+").OfType<Match>().Select(m => m.Value));
                        //string numLot = filename.Split('_')[1];

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
                                        string argEmploi = "Travail";
                                       
                                        

                                     /*   List<ClientCompte> ccList1 = new List<ClientCompte>();
                                        List<ClientCompte> ccList2 = new List<ClientCompte>();
                                     */


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
                                            string Emploi = "";

                                         


                                            //updateEmploi(row["Compte"].ToString(), row["Travail"].ToString());

                                        
/*
                                            ccList1.Add(c1);
                                            ccList2.Add(c2);
                                            */


                                            //Debug.WriteLine(row["Travail"].ToString());
                                           // updateEmploi(row["ID"].ToString(), row["Travail"].ToString());


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
                                                 Emploi = row[argEmploi].ToString();
                                             }
                                             catch (ArgumentException e)
                                             {
                                                 ModelState.AddModelError("Importer", "La colonne " + argEmploi + " n'appartient pas à la table Excel, verifier les espaces cachés et les majuscules.");
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
                                                                                            Emploi = Emploi

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

                                                    //lot.NumLot = Lot.NumLot;
                                                   
                                                    lot.NomClient = Lot.NomClient;
                                                    lot.TelPortable = Lot.TelPortable;
                                                    lot.TelFixe = Lot.TelFixe;
                                                    lot.Emploi = Lot.Emploi;
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

                                        /*
                                        ccList1 = ccList1.Where(c => c.Compte != "").ToList();
                                        ccList2 = ccList2.Where(c => c.Compte != "").ToList();

                                        */
                                        List<ClientCompte> newList = new List<ClientCompte>();
                                        
                                       /* foreach(ClientCompte c in ccList1)
                                        {

                                            if(!ccList2.Contains(c))
                                            {

                                                newList.Add(c);

                                            }
                                            else
                                            {

                                            }

                                        }*/

                                      
                                        /*
                                        
                                        string path1 = GetFolderName() + "/" + "Diff" + "_MAJ_" + DateTime.Now.ToString("dd.MM.yyyy") + "_" + ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds() + ".xlsx";
                                        */

                                        ViewData["nbTotal"] = dt.Rows.Count;
                                        ViewData["finished"] = "1";
                                        LotService.Commit();

                                        /*
                                        Debug.WriteLine(newList.Count);

                                        GenerateExcel(GenerateDatatableFromJoinedList(newList), path1);
                                        */
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



                    LotService.Dispose();
                    return View("ImportLot");



                }
            }
  
        }


        public string GetFolderName()
        {
            string folderName = Server.MapPath("~/Uploads/Updates");
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);

            }

            return folderName;
        }


        public DataTable GenerateDatatableFromJoinedList(List<ClientCompte> list)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);

                    List<ClientCompte> newList = new List<ClientCompte>();
                    DataTable dataTable = new DataTable();

                    newList = list.Select(j =>
                     new ClientCompte
                     {

                         Nom =j.Nom,
                         Compte = j.Compte
               
                     }).ToList();

                    dataTable.Columns.Add("Nom", typeof(string));
                    dataTable.Columns.Add("Compte", typeof(string));
           
                    foreach (ClientCompte c in newList)
                    {

                        DataRow row = dataTable.NewRow();
                        row["Nom"] = c.Nom;
                        row["Compte"] = c.Compte;
                        dataTable.Rows.Add(row);

                    }

                    Debug.WriteLine("---->"+dataTable.Rows.Count);

                    return dataTable;

                }
            }
        }



        public void Traitement()
        {




        }

        public static void GenerateExcel(DataTable dataTable, string path)
        {
            dataTable.TableName = "Table1";

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);
            
            // create a excel app along side with workbook and worksheet and give a name to it
            Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook excelWorkBook = excelApp.Workbooks.Add();
            Microsoft.Office.Interop.Excel._Worksheet xlWorksheet = excelWorkBook.Sheets[1];
            Microsoft.Office.Interop.Excel.Range xlRange = xlWorksheet.UsedRange;

            foreach (DataTable table in dataSet.Tables)
            {
                //Add a new worksheet to workbook with the Datatable name
                // Excel.Worksheet excelWorkSheet = excelWorkBook.Sheets.Add();
                Microsoft.Office.Interop.Excel.Worksheet excelWorkSheet = excelWorkBook.Sheets.Add();

                
                excelWorkSheet.Cells.EntireColumn.NumberFormat = "@";

                excelWorkSheet.Name = table.TableName;

                // add all the columns
                for (int i = 1; i < table.Columns.Count + 1; i++)
                {
                    excelWorkSheet.Cells[1, i] = table.Columns[i - 1].ColumnName;
                    excelWorkSheet.Cells[1, i].Font.Bold = true;
                    excelWorkSheet.Cells[1, i].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    excelWorkSheet.Cells[1, i].Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                    excelWorkSheet.Cells[1, i].Borders.Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin;
                    excelWorkSheet.Cells[1, i].Borders.Weight = 2;
                    excelWorkSheet.Cells[1, i].Font.Size = 14;
                    excelWorkSheet.Cells[1, i].ColumnWidth = 22;
                }

                // add all the rows
                for (int j = 0; j < table.Rows.Count; j++)
                {
                    for (int k = 0; k < table.Columns.Count; k++)
                    {

                        excelWorkSheet.Cells[j + 2, k + 1] = table.Rows[j].ItemArray[k].ToString();
                        excelWorkSheet.Cells[j + 2, k + 1].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        excelWorkSheet.Cells[j + 2, k + 1].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        excelWorkSheet.Cells[j + 2, k + 1].Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                        excelWorkSheet.Cells[j + 2, k + 1].Borders.Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin;
                        excelWorkSheet.Cells[j + 2, k + 1].Borders.Weight = 2;
                        excelWorkSheet.Cells[j + 2, k + 1].Font.Size = 12;
                        excelWorkSheet.Cells[j + 2, k + 1].ColumnWidth = 22;
                    }
                }
            }
            // excelWorkBook.Save(); -> this will save to its default location
            
            
            Debug.WriteLine("----->"+path);
             

            excelWorkBook.SaveAs(path); // -> this will do the custom
            excelWorkBook.Close();
            excelApp.Quit();
        }

        public void updateEmploi (string compte,string emploi)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);

                   
                    Lot lot = LotService.Get(l => l.IDClient == compte);

                    if(lot !=null)
                    {

                        lot.Emploi = emploi;
                        Debug.WriteLine(lot.Compte);
                        LotService.Commit();

                    }
                    else
                    {
                        Debug.WriteLine("no no ===>"+ compte);

                    }

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
                                      Emploi = a.Emploi
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

                    LotService.Dispose();
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

        public double castSolde(string solde)
        {
            double soldeDeb = 0;
            if(double.TryParse(solde, out soldeDeb))
            {
                return soldeDeb;
            }
            
            return soldeDeb;
        }

        public void updateFormulaireStatusIfNeeded(int affectationId,double newSoldeDeb, AffectationService AffectationService,FormulaireService FormulaireService)
        {

            var JoinedLot = from f in FormulaireService.GetAll()
                            join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                            where a.AffectationId == affectationId
                            select new ClientAffecteViewModel {  Formulaire = f };

            List<Formulaire> formulaireList = new List<Formulaire>();
            JoinedLot.ForEach(j =>
            {
                
                if (newSoldeDeb> j.Formulaire.MontantDebInitial)
                {

                    Formulaire formulaireTemp = j.Formulaire;
                    formulaireTemp.MontantDebInitial = newSoldeDeb;
                    
                    if(formulaireList.Count == 0)
                    {

                        Decimal newMontantDebMaj = Convert.ToDecimal(newSoldeDeb) - Convert.ToDecimal(formulaireTemp.MontantVerseDeclare);

                        if ((formulaireTemp.EtatClient == Note.SOLDE || formulaireTemp.EtatClient == Note.SOLDE_TRANCHE))
                        {

                            if (newMontantDebMaj > 0)
                            {
                                formulaireTemp.EtatClient = Note.SOLDE_TRANCHE;
                                formulaireTemp.MontantDebMAJ = Math.Abs((double)newMontantDebMaj);

                            }
                            else
                            {
                                formulaireTemp.EtatClient = Note.SOLDE;
                                formulaireTemp.MontantDebMAJ = 0;

                            }
                        }

                    }
                    else
                    {
                        double minSoldeDebMAj = newSoldeDeb;

                        try
                        {
                            minSoldeDebMAj = formulaireList.Where(minJ => minJ.Status == Status.VERIFIE).Min(minJ => minJ.MontantDebMAJ);
                        }
                        catch(Exception e)
                        {
                            minSoldeDebMAj = newSoldeDeb;
                        }

                        Decimal newMontantDebMaj = Convert.ToDecimal(minSoldeDebMAj) - Convert.ToDecimal(formulaireTemp.MontantVerseDeclare);
                        
                        if ((formulaireTemp.EtatClient == Note.SOLDE || formulaireTemp.EtatClient == Note.SOLDE_TRANCHE))
                        {
                            
                            if(newMontantDebMaj > 0)
                            {
                                formulaireTemp.EtatClient = Note.SOLDE_TRANCHE;
                                formulaireTemp.MontantDebMAJ = Math.Abs((double)newMontantDebMaj);

                            }
                            else
                            {
                                formulaireTemp.EtatClient = Note.SOLDE;
                                formulaireTemp.MontantDebMAJ = 0;
                            }
                        }

                    }

                    FormulaireService.Update(formulaireTemp);
                    FormulaireService.Commit();

                }
                //167097
                formulaireList.Add(j.Formulaire);

            });

        }

        public ActionResult updateLot(int id,string errorSolde)
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
                    ViewBag.errorSolde = errorSolde;

                    UpdateLotViewModel lotToUpdate = new UpdateLotViewModel();
                    
                    lotToUpdate = (from a in AffectationService.GetAll()
                            join l in LotService.GetAll() on a.LotId equals l.LotId
                            where l.LotId == id
                            select new UpdateLotViewModel
                            {
                                
                                LotId = l.LotId,
                                AffectationId = a.AffectationId,
                                EmployeId = a.EmployeId,
                                NumLot = l.NumLot,
                                NomClient = l.NomClient,
                                IDClient = l.IDClient,
                                Adresse = l.Adresse,
                                Compte = l.Compte,
                                DescIndustry = l.DescIndustry,
                                Numero =  l.Numero,
                                SoldeDebiteur = castSolde(l.SoldeDebiteur),
                                TelFixe = l.TelFixe,
                                TelPortable = l.TelPortable,
                                Emploi = l.Emploi,
                                PostCode = l.PostCode,

                            }).FirstOrDefault();

                    if (lotToUpdate == null)
                    {

                        lotToUpdate = new UpdateLotViewModel();
                        Lot lot = LotService.GetById(id);
                        lotToUpdate.LotId = lot.LotId;
                        lotToUpdate.TelFixe = lot.TelFixe;
                        lotToUpdate.NumLot = lot.NumLot;
                        lotToUpdate.Numero = lot.Numero;
                        lotToUpdate.SoldeDebiteur = castSolde(lot.SoldeDebiteur);
                        lotToUpdate.TelPortable = lot.TelPortable;
                        lotToUpdate.Adresse = lot.Adresse;
                        lotToUpdate.IDClient = lot.IDClient;
                        lotToUpdate.PostCode = lot.PostCode;
                        lotToUpdate.Emploi = lot.Emploi;
                        lotToUpdate.DescIndustry = lot.DescIndustry;
                        lotToUpdate.Adresse = lot.Adresse;
                        lotToUpdate.NomClient = lot.NomClient;
                        
                    }

                    LotService.Dispose();
                    AffectationService.Dispose();
                    EmpService.Dispose();
                    return View(lotToUpdate);
                
                }
            }

        }

        [HttpPost]
        public ActionResult updateLot(UpdateLotViewModel updateLotvm)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {
                    LotService LotService = new LotService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);

                    double newSolde = 0;
                    double oldSolde = 0;
                    Lot newlot = LotService.GetById(updateLotvm.LotId);

                    newlot.Adresse = updateLotvm.Adresse;
                    newlot.Compte = updateLotvm.Compte;
                    newlot.DescIndustry = updateLotvm.DescIndustry;
                    newlot.Emploi = updateLotvm.Emploi;
                    newlot.IDClient = updateLotvm.IDClient;
                    newlot.NomClient = updateLotvm.NomClient;
                    newlot.NumLot = updateLotvm.NumLot;
                    newlot.PostCode = updateLotvm.PostCode;

                    if (Double.TryParse(newlot.SoldeDebiteur, out oldSolde))
                    {
                        if (Double.TryParse(updateLotvm.SoldeDebiteur+"", out newSolde) )
                        {

                            if(newSolde == 0)
                            {

                                ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
                                ViewData["agent"] = new SelectList(AgentListForDropDown(), "Value", "Text");
                                ModelState.AddModelError("SoldeDebiteur", "Nouveau solde n'est pas valide");
                                return View(updateLotvm);

                            }
                            else
                            {

                                if (newSolde >= oldSolde)
                                {

                                    newlot.SoldeDebiteur = updateLotvm.SoldeDebiteur + "";
                                    
                                    if(newSolde !=oldSolde)
                                    {
                                        updateFormulaireStatusIfNeeded(updateLotvm.AffectationId, updateLotvm.SoldeDebiteur, AffectationService, FormulaireService);
                                    }

                                }
                                else
                                {
                                    ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
                                    ViewData["agent"] = new SelectList(AgentListForDropDown(), "Value", "Text");
                                    ModelState.AddModelError("SoldeDebiteur", "Nouveau solde doit etre superieur à l'ancien");
                                    return View(updateLotvm);
                                }
                            }

                        }
                       

                    }
                 
                    newlot.TelFixe = updateLotvm.TelFixe;
                    newlot.TelPortable = updateLotvm.TelPortable;
                    //newlot.Type = updateLotvm.Type;
                    newlot.Emploi = updateLotvm.Emploi;

                    
                    LotService.Update(newlot);
                    //LotService.Commit();

                    Affectation affectation = AffectationService.GetById(updateLotvm.AffectationId);

                    if (affectation == null)
                    {
                        Affectation aff = new Affectation
                        {

                            EmployeId = updateLotvm.EmployeId,
                            LotId = updateLotvm.LotId,
                            DateAffectation = DateTime.Now

                        };

                        AffectationService.Add(aff);
                        AffectationService.Commit();

                    }
                    else
                    {

                        affectation.EmployeId = updateLotvm.EmployeId;
                        affectation.LotId = updateLotvm.LotId;
                        affectation.DateAffectation = DateTime.Now;
                        AffectationService.Update(affectation);
                        AffectationService.Commit();

                    }

                    AffectationService.Dispose();
                    LotService.Dispose();
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

                    Debug.WriteLine(lot);

                    LotService LotService = new LotService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);

                    LotService.Add(lot);
                    LotService.Commit();

                    Affectation affectation = new Affectation
                    {
                        DateAffectation = DateTime.Now,
                        LotId = lot.LotId,
                        EmployeId = agent
                    };

                    AffectationService.Add(affectation);
                    AffectationService.Commit();

                    LotService.Dispose();
                    AffectationService.Dispose();
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

