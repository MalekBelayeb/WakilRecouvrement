using MyFinance.Data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WakilRecouvrement.Data;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Web.Models;
using WakilRecouvrement.Service;
using System.IO;
using System.Data;
using Excel = Microsoft.Office.Interop.Excel;
using WakilRecouvrement.Web.Models.ViewModel;
using PagedList;

namespace WakilRecouvrement.Web.Controllers
{
    public class ContactBanqueController : Controller
    {

        public ActionResult GenererResultat(string traite, string currentTraite, string numLot, string currentNumLot, string type, string currentType, string debutDate, string currentDebutDate,string currentFinDate,string currentJourDate, string finDate, string jourdate, string export, string currentPage, int? page)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);

                    ViewData["list"] = new SelectList(DropdownListController.NumLotListForDropDown(LotService), "Value", "Text");
                    ViewBag.TraiteList = new SelectList(DropdownListController.EnvoyerTraiteListForDropDown(), "Value", "Text");
                    ViewBag.typeTrait = new SelectList(DropdownListController.TraiteTypeForValiderListForDropDown(), "Value", "Text");

                    List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

                    if (page == null)
                    {
                        if (currentPage != null)
                            page = int.Parse(currentPage);
                    }

                    ViewBag.page = page;



                    if (numLot != null)
                    {
                        //page = 1;
                    }
                    else
                    {
                        numLot = currentNumLot;
                    }

                    ViewBag.currentNumLot = numLot;

                    if (type != null)
                    {
                        //page = 1;
                    }
                    else
                    {
                        type = currentType;
                    }

                    ViewBag.currentType = type;

                    if (traite != null)
                    {
                        //page = 1;
                    }
                    else
                    {
                        traite = currentTraite;
                    }

                    ViewBag.currentTraite = traite;

                    if (jourdate != null)
                    {
                        //page = 1;
                    }
                    else
                    {
                        jourdate = currentJourDate;
                    }

                    ViewBag.jourdate = jourdate;

                    if (debutDate != null)
                    {
                        //page = 1;
                    }
                    else
                    {
                        debutDate = currentDebutDate;
                    }

                    ViewBag.debutdate = debutDate;

                    if (finDate != null)
                    {
                        //page = 1;
                    }
                    else
                    {
                        finDate = currentFinDate;
                    }

                    ViewBag.findate = finDate;

                    string name = "";

                    if (traite == "RDV")
                    {

                        JoinedList = (from f in FormulaireService.GetAll()
                                      join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                      join l in LotService.GetAll() on a.LotId equals l.LotId

                                      select new ClientAffecteViewModel
                                      {

                                          Formulaire = f,
                                          Lot = l,
                                          Affectation = a

                                      }).Where(j => j.Formulaire.Status == Status.VERIFIE && j.Formulaire.EtatClient == Note.RDV).ToList();
                        
                        name = "RDV";

                    }
                    else if (traite == "SOLDE")
                    {

                        JoinedList = (from f in FormulaireService.GetAll()
                                      join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                      join l in LotService.GetAll() on a.LotId equals l.LotId

                                      select new ClientAffecteViewModel
                                      {

                                          Formulaire = f,
                                          Lot = l,
                                          Affectation = a

                                      }).Where(j => j.Formulaire.Status == Status.VERIFIE && (j.Formulaire.EtatClient == Note.SOLDE || j.Formulaire.EtatClient == Note.SOLDE_TRANCHE)).ToList();

                        name = "SOLDE";

                    }
                    else if (traite == "A_VERIFIE")
                    {

                        JoinedList = (from f in FormulaireService.GetAll()
                                      join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                      join l in LotService.GetAll() on a.LotId equals l.LotId

                                      select new ClientAffecteViewModel
                                      {

                                          Formulaire = f,
                                          Lot = l,
                                          Affectation = a

                                      }).Where(j => j.Formulaire.EtatClient == Note.A_VERIFIE && j.Formulaire.Status == Status.EN_COURS).ToList();

                        name = "A_VERIFIE";

                    }
                    else if (traite == "Autre")
                    {
                        
                        JoinedList = (from f in FormulaireService.GetAll()
                                      join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                      join l in LotService.GetAll() on a.LotId equals l.LotId

                                      select new ClientAffecteViewModel
                                      {

                                          Formulaire = f,
                                          Lot = l,
                                          Affectation = a

                                      }).Where(j => j.Formulaire.EtatClient == Note.FAUX_NUM || j.Formulaire.EtatClient == Note.NRP || j.Formulaire.EtatClient == Note.RACCROCHE || j.Formulaire.EtatClient == Note.INJOIGNABLE || j.Formulaire.EtatClient == Note.RAPPEL || j.Formulaire.EtatClient == Note.REFUS_PAIEMENT).ToList();

                        name = "AUTRE";

                    }
                    else
                    {
                        JoinedList = (from f in FormulaireService.GetAll()
                                      join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                      join l in LotService.GetAll() on a.LotId equals l.LotId

                                      select new ClientAffecteViewModel
                                      {

                                          Formulaire = f,
                                          Lot = l,
                                          Affectation = a

                                      }).Where(j => j.Formulaire.Status == Status.VERIFIE && (j.Formulaire.EtatClient == Note.SOLDE || j.Formulaire.EtatClient == Note.SOLDE_TRANCHE)).ToList();

                    }

                    if (numLot != "0")
                    {
                        JoinedList = JoinedList.Where(j => j.Lot.NumLot.Equals(numLot)).ToList();
                    }

                    if (type == "P_INTERVAL")
                    {

                        if (String.IsNullOrEmpty(debutDate) == false && String.IsNullOrEmpty(finDate) == false)
                        {

                            if (traite == "SOLDE")
                            {

                                JoinedList = JoinedList.Where(j => j.Formulaire.VerifieLe.Date >= DateTime.Parse(debutDate).Date && j.Formulaire.VerifieLe.Date <= DateTime.Parse(finDate).Date).ToList();
                            }
                            else
                            {
                                JoinedList = JoinedList.Where(j => j.Formulaire.TraiteLe.Date >= DateTime.Parse(debutDate).Date && j.Formulaire.TraiteLe.Date <= DateTime.Parse(finDate).Date).ToList();

                            }

                        }
                       
                    }
                    else if (type == "P_DATE")
                    {

                        if ( String.IsNullOrEmpty(jourdate) == false)
                        {
                            if (traite =="SOLDE")
                            {

                                JoinedList = JoinedList.Where(j => j.Formulaire.VerifieLe.Date == DateTime.Parse(jourdate).Date).ToList();

                            }
                            else
                            {
                                JoinedList = JoinedList.Where(j => j.Formulaire.TraiteLe.Date == DateTime.Parse(jourdate).Date).ToList();
                            }

                        }

                    }

                    if(string.IsNullOrEmpty(export)==false)
                    {
                        if (export == "2")
                        {
                            string filename = name + "_MAJ_" + DateTime.Now.ToString("dd.MM.yyyy") + "_" + ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds() + ".xlsx";
                            string path = GetFolderName() + "/" + filename;

                            GenerateExcel(GenerateDatatableFromJoinedList(JoinedList, traite), path);
                            
                            return downloadFile(path, filename);
                        }
                    }
                
                  
                    var modifiedData = JoinedList.Select(j =>
                         new ContactBanqueViewModel
                         {

                             NumLot = j.Lot.NumLot,
                             Compte = j.Lot.Compte,
                             IDClient = j.Lot.IDClient,
                             NomClient = j.Lot.NomClient,
                             Etat = j.Formulaire.EtatClient.ToString(),

                         });

                    ViewBag.total = JoinedList.Count();

                    int pageSize = 10;
                    int pageNumber = (page ?? 1);

                    return View(modifiedData.ToPagedList(pageNumber, pageSize));
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

        public void GenerateExcel(DataTable dataTable, string path)
        {


            dataTable.TableName = "Table1";

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);
            // create a excel app along side with workbook and worksheet and give a name to it
            Excel.Application excelApp = new Excel.Application();
            Excel.Workbook excelWorkBook = excelApp.Workbooks.Add();

            Excel._Worksheet xlWorksheet = excelWorkBook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;
            foreach (DataTable table in dataSet.Tables)
            {
                //Add a new worksheet to workbook with the Datatable name
                // Excel.Worksheet excelWorkSheet = excelWorkBook.Sheets.Add();
                Excel.Worksheet excelWorkSheet = excelWorkBook.Sheets.Add();

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

            excelWorkBook.SaveAs(path); // -> this will do the custom

            excelWorkBook.Close();
            excelApp.Quit();
        
        }


        public DataTable GenerateDatatableFromJoinedList(List<ClientAffecteViewModel> list, string traite)
        {

            List<FormulaireExportable> newList = new List<FormulaireExportable>();
            DataTable dataTable = new DataTable();

            if (traite.Equals("RDV"))
            {
                newList = list.Select(j =>
                 new FormulaireExportable
                 {
                     NomClient = j.Lot.NomClient,
                     Compte = j.Lot.Compte,
                     IDClient = j.Lot.IDClient,
                     RDV = j.Formulaire.DateRDV.ToString()
                 }).ToList();

                dataTable.Columns.Add("IDClient", typeof(string));
                dataTable.Columns.Add("Compte", typeof(string));
                dataTable.Columns.Add("NomClient", typeof(string));
                dataTable.Columns.Add("RDV", typeof(string));

                foreach (FormulaireExportable c in newList)
                {

                    DataRow row = dataTable.NewRow();
                    row["IDClient"] = c.IDClient;
                    row["Compte"] = c.Compte;
                    row["NomClient"] = c.NomClient;
                    row["RDV"] = c.RDV;
                    dataTable.Rows.Add(row);

                }
            }
            else if (traite.Equals("SOLDE"))
            {
                newList = list.Select(j =>
                new FormulaireExportable
                {
                    NomClient = j.Lot.NomClient,
                    Compte = j.Lot.Compte,
                    IDClient = j.Lot.IDClient,
                    Versement = j.Formulaire.MontantVerseDeclare + ""
                }

                ).ToList();

                dataTable.Columns.Add("IDClient", typeof(string));
                dataTable.Columns.Add("Compte", typeof(string));
                dataTable.Columns.Add("NomClient", typeof(string));
                dataTable.Columns.Add("Versement", typeof(string));

                foreach (FormulaireExportable c in newList)
                {

                    DataRow row = dataTable.NewRow();
                    row["IDClient"] = c.IDClient;
                    row["Compte"] = c.Compte;
                    row["NomClient"] = c.NomClient;
                    row["Versement"] = c.Versement;
                    dataTable.Rows.Add(row);

                }

            }
            else if (traite.Equals("A_VERIFIE"))
            {
                newList = list.Select(j =>
                new FormulaireExportable
                {
                    NomClient = j.Lot.NomClient,
                    Compte = j.Lot.Compte,
                    IDClient = j.Lot.IDClient,
                }

                ).ToList();

                dataTable.Columns.Add("IDClient", typeof(string));
                dataTable.Columns.Add("Compte", typeof(string));
                dataTable.Columns.Add("NomClient", typeof(string));
                dataTable.Columns.Add("Montant", typeof(string));

                foreach (FormulaireExportable c in newList)
                {

                    DataRow row = dataTable.NewRow();

                    row["IDClient"] = c.IDClient;
                    row["Compte"] = c.Compte;
                    row["NomClient"] = c.NomClient;

                    dataTable.Rows.Add(row);

                }

            }
            else
            {
                newList = list.Select(j =>
                new FormulaireExportable
                {
                    NomClient = j.Lot.NomClient,
                    Compte = j.Lot.Compte,
                    IDClient = j.Lot.IDClient,
                }

                ).ToList();

                dataTable.Columns.Add("IDClient", typeof(string));
                dataTable.Columns.Add("Compte", typeof(string));
                dataTable.Columns.Add("NomClient", typeof(string));

                foreach (FormulaireExportable c in newList)
                {


                    DataRow row = dataTable.NewRow();
                    row["IDClient"] = c.IDClient;
                    row["Compte"] = c.Compte;
                    row["NomClient"] = c.NomClient;
                    dataTable.Rows.Add(row);

                }

            }

            return dataTable;


        }

        public FileResult downloadFile(string path,string filename)
        {

            try
            {

                byte[] fileBytes = System.IO.File.ReadAllBytes(path);

                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, filename);

            }
            catch (FileNotFoundException e)
            {
                return null;
            }

        }

    }
}