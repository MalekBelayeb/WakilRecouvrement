using iText.Kernel.Pdf;
using iText.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WakilRecouvrement.Service;
using iText.Layout.Element;
using iText.Layout.Properties;
using WakilRecouvrement.Web.Models.ViewModel;
using iText.Kernel.Colors;
using System.Diagnostics;
using WakilRecouvrement.Web.Models;
using Microsoft.Ajax.Utilities;
using System.Data;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;
using WakilRecouvrement.Domain.Entities;
using PagedList;

namespace WakilRecouvrement.Web.Controllers
{
    public class FactureController : Controller
    {


        AffectationService AffectationService;
        LotService LotService;
        EmployeService EmpService;
        FormulaireService FormulaireService;
        NotificationService NotificationService;
        FactureService factureService;


        public FactureController ()
        {
            AffectationService = new AffectationService();
            LotService = new LotService();
            EmpService = new EmployeService();
            FormulaireService = new FormulaireService();
            NotificationService = new NotificationService();
            factureService = new FactureService();
        }


        public ActionResult genererFacture(int? page)
        {

            List<Facture> factureList = factureService.GetAll().OrderByDescending(f=>f.DateExtrait).ToList();


            ViewBag.total = factureList.Count();

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            return View(factureList.ToPagedList(pageNumber, pageSize));

        }


        public List<ClientAffecteViewModel> getTraitHist(int idAff)
        {

            List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

            JoinedList = (from f in FormulaireService.GetAll()
                          join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                          join l in LotService.GetAll() on a.LotId equals l.LotId
                          where f.Status == Domain.Entities.Status.VERIFIE && f.EtatClient == Domain.Entities.Note.SOLDE_TRANCHE && f.AffectationId == idAff

                          select new ClientAffecteViewModel
                          {

                              Formulaire = f,
                              Affectation = a,
                              Lot = l,

                          }).OrderByDescending(f=>f.Formulaire.TraiteLe).ToList();

            return JoinedList;
        }




        public ClientAffecteViewModel getVersementDor(int idAff)
        {

            List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

            JoinedList = (from f in FormulaireService.GetAll()
                          join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                          join l in LotService.GetAll() on a.LotId equals l.LotId
                          where f.Status == Domain.Entities.Status.VERIFIE && f.EtatClient == Domain.Entities.Note.SOLDE_TRANCHE && f.AffectationId == idAff

                          select new ClientAffecteViewModel
                          {

                              Formulaire = f,
                              Affectation = a,
                              Lot = l,

                          }).OrderByDescending(f => f.Formulaire.TraiteLe).ToList();

            return JoinedList.FirstOrDefault();
        }

        public ActionResult extraireFacture(string debutDate, string finDate)
        {


            DateTime startDate = DateTime.Parse(debutDate);
            DateTime endDate = DateTime.Parse(finDate);
            double trancheTot = 0;
            double soldeTot = 0;
            double tot = 0;
            List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();
            List<ClientAffecteViewModel> TempJoinedList = new List<ClientAffecteViewModel>();
            List<ClientAffecteViewModel> AnnexeJoinedList = new List<ClientAffecteViewModel>();

            JoinedList = (from f in FormulaireService.GetAll()
                          join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                          join l in LotService.GetAll() on a.LotId equals l.LotId
                          where f.Status == Domain.Entities.Status.VERIFIE
                          select new ClientAffecteViewModel
                          {

                              Formulaire = f,
                              Affectation = a,
                              Lot = l,

                          }).Where(j=>j.Formulaire.TraiteLe.Date>=startDate.Date && j.Formulaire.TraiteLe.Date<=endDate.Date).ToList();


            foreach(ClientAffecteViewModel cvm in JoinedList)
            {

                if(cvm.Formulaire.EtatClient == Domain.Entities.Note.SOLDE_TRANCHE)
                {
                    trancheTot += (cvm.Formulaire.MontantVerseDeclare * 15) / 100;
                    cvm.vers = cvm.Formulaire.MontantVerseDeclare;
                    AnnexeJoinedList.Add(cvm);

                }

                if (cvm.Formulaire.EtatClient == Domain.Entities.Note.SOLDE)
                {
                    TempJoinedList = getTraitHist(cvm.Affectation.AffectationId);
                    if(TempJoinedList.Count() == 0)
                    {

                        soldeTot += (cvm.Formulaire.MontantDebInitial * 15) / 100;
                        cvm.vers = cvm.Formulaire.MontantDebInitial;
                        AnnexeJoinedList.Add(cvm);
                    }
                    else
                    {

                        soldeTot += (TempJoinedList.FirstOrDefault().Formulaire.MontantDebMAJ * 15) / 100;
                        cvm.vers = TempJoinedList.FirstOrDefault().Formulaire.MontantDebMAJ;
                        AnnexeJoinedList.Add(cvm);

                    }


                }


            }

            string lotsNames = "";
            List<string> listNameLot = JoinedList.DistinctBy(j => j.Lot.NumLot).Select(l => l.Lot.NumLot).ToList();
       
            if(listNameLot.Count()==1)
            {
                lotsNames = listNameLot.FirstOrDefault();
            }else if(listNameLot.Count()>1)
            {
                string lastlots = JoinedList.DistinctBy(j => j.Lot.NumLot).Select(l => l.Lot.NumLot).LastOrDefault();
                listNameLot.RemoveAt(listNameLot.Count - 1);
                string lots = String.Join(", ", listNameLot);
                lotsNames = lots + " et " + lastlots;
            }


            tot = soldeTot + trancheTot;
            FactureContent factureContent = new FactureContent();
            factureContent.Date = DateTime.Today;
            factureContent.Beneficiere = "Zaitouna Bank";
            factureContent.PrixHT = tot;
            factureContent.PrixTVA = (tot*19)/100;
            factureContent.PrixTTC = tot + factureContent.PrixTVA;
            factureContent.TimbreFiscal = 0.600;

            string annexeFileName = "Annexe_" + DateTime.Now.ToString("dd.MM.yyyy") + "_" + ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds() + ".xlsx";
            string factureFileName = "Facture_" + DateTime.Now.ToString("dd.MM.yyyy") + "_" + ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds() + ".pdf";
            string pathAnnexe = GetFolderName() + "/"+annexeFileName;
            string pathFacture = GetFolderName() + "/"+factureFileName;
            Facture facture = new Facture() { 
            
                AnnexePathName = annexeFileName,
                FacturePathName = factureFileName,
                DateDeb = startDate,
                DateFin = endDate,
                DateExtrait = DateTime.Now

            };
            factureService.Add(facture);
            factureService.Commit();
            GenerateExcel(GenerateDatatableFromJoinedList(AnnexeJoinedList), pathAnnexe);
            GeneratePDF(pathFacture,lotsNames, factureContent);

            return RedirectToAction("genererFacture", new { page=1 });
        }

        public void GeneratePDF(string path,string lotsNames,FactureContent factureContent)
        {
            string date = factureContent.Date.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

            PdfWriter writer = new PdfWriter(path);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf);
            Paragraph title = new Paragraph("Facture "+ date).SetTextAlignment(TextAlignment.CENTER).SetBold().SetFontSize(20);
            
            Paragraph dateLabel = new Paragraph("Date: ").SetTextAlignment(TextAlignment.LEFT).SetFontSize(15);
            Paragraph dateValue = new Paragraph(date).SetTextAlignment(TextAlignment.LEFT).SetFontSize(15);
            
            Paragraph beneficiereLabel = new Paragraph("Bénéficiaire: ").SetTextAlignment(TextAlignment.LEFT).SetFontSize(15);
            Paragraph beneficiereValue = new Paragraph(factureContent.Beneficiere).SetTextAlignment(TextAlignment.LEFT).SetFontSize(15);
            Paragraph signature = new Paragraph("Signature").SetFontSize(18).SetTextAlignment(TextAlignment.RIGHT).SetPaddingRight(25);


            Table table = new Table(UnitValue.CreatePercentArray(6)).UseAllAvailableWidth();

            Cell cell11 = new Cell(1, 3)
                  .SetBold()
                  .SetTextAlignment(TextAlignment.LEFT)
                  .Add(new Paragraph("Designation"));
            
            Cell cell12 = new Cell(1, 1)
                  .SetBold()
                  .SetTextAlignment(TextAlignment.LEFT)
                  .Add(new Paragraph("PRIX (HT)"));
            
            Cell cell13 = new Cell(1,1)
                 .SetBold()
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph("TVA (19%)"));
            
            Cell cell14 = new Cell(1, 1)
                 .SetBold()
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph("PRIX (TTC)"));
    
            table.AddCell(cell11);
            table.AddCell(cell12);
            table.AddCell(cell13);
            table.AddCell(cell14);


            Cell cell21 = new Cell(2, 3)
                .SetBold()
               .SetTextAlignment(TextAlignment.LEFT)
               .Add(new Paragraph("Assistance pour recouvrement des comptes débiteurs: Lot "+ lotsNames));

            Cell cell22 = new Cell(2, 1)
               .SetTextAlignment(TextAlignment.LEFT)
               .Add(new Paragraph(String.Format("{0:0.000}", factureContent.PrixHT)));
            Cell cell23 = new Cell(2, 1)
                         .SetTextAlignment(TextAlignment.LEFT)
                         .Add(new Paragraph(String.Format("{0:0.000}", factureContent.PrixTVA) ));
            Cell cell24 = new Cell(2, 1)
                      .SetTextAlignment(TextAlignment.LEFT)
                      .Add(new Paragraph(String.Format("{0:0.000}", factureContent.PrixTTC)));

            table.AddCell(cell21);
            table.AddCell(cell22);
            table.AddCell(cell23);
            table.AddCell(cell24);





            Table table2 = new Table(UnitValue.CreatePercentArray(6)).UseAllAvailableWidth();


            Cell cell_2_11 = new Cell(1, 3)
                 .SetBold()
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph("PRIX TOTAL (HT)"));
            Cell cell_2_12 = new Cell(1, 3)
                 
                 .SetTextAlignment(TextAlignment.RIGHT)
               .Add(new Paragraph(String.Format("{0:0.000}", factureContent.PrixHT)));


            Cell cell_2_21 = new Cell(2, 3)
                 .SetBold()
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph("MONTANT TVA"));
            Cell cell_2_22 = new Cell(2, 3)
                 
                 .SetTextAlignment(TextAlignment.RIGHT)
                         .Add(new Paragraph(String.Format("{0:0.000}", factureContent.PrixTVA)));

            Cell cell_2_31 = new Cell(3, 3)
                 .SetBold()
                 .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph("TIMBRE FISCAL"));
            Cell cell_2_32 = new Cell(3, 3)
                 
                 .SetTextAlignment(TextAlignment.RIGHT)
                      .Add(new Paragraph(String.Format("{0:0.000}", factureContent.TimbreFiscal)));

            Cell cell_2_41 = new Cell(4, 3)
               .SetBold()
               .SetBackgroundColor(ColorConstants.GRAY)
               .SetTextAlignment(TextAlignment.LEFT)
               .Add(new Paragraph("MONTANT A PAYER EN DT"));
            Cell cell_2_42 = new Cell(4, 3)
                 .SetTextAlignment(TextAlignment.CENTER)
                 .SetBackgroundColor(ColorConstants.GRAY)
                      .Add(new Paragraph(String.Format("{0:0.000}", factureContent.PrixTTC)));


            table2.AddCell(cell_2_11);
            table2.AddCell(cell_2_12);

            table2.AddCell(cell_2_21);
            table2.AddCell(cell_2_22);

            table2.AddCell(cell_2_31);
            table2.AddCell(cell_2_32);

            table2.AddCell(cell_2_41);
            table2.AddCell(cell_2_42);

            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(title);
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(dateLabel.Add(dateValue));
            document.Add(beneficiereLabel.Add(beneficiereValue));
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());

            document.Add(table);
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            
            document.Add(table2);

            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());
            document.Add(new Paragraph());

            document.Add(signature);
            document.Close();

        }
        public DataTable GenerateDatatableFromJoinedList(List<ClientAffecteViewModel> list)
        {
            List<FormulaireExportable> newList = new List<FormulaireExportable>();
            DataTable dataTable = new DataTable();

            newList = list.Select(j =>
               new FormulaireExportable
               {
                   IDClient = j.Lot.IDClient,
                   Compte = j.Lot.Compte,
                   NomClient = j.Lot.NomClient,
                   Versement = String.Format("{0:0.000}", j.vers)
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

            return dataTable;
        }

        public static void GenerateExcel(DataTable dataTable, string path)
        {

            dataTable.TableName = "Table1";

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);
            // create a excel app along side with workbook and worksheet and give a name to it
            Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
            Excel.Workbook excelWorkBook = excelApp.Workbooks.Add();

            Excel._Worksheet xlWorksheet = excelWorkBook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;
            
            foreach (DataTable table in dataSet.Tables)
            {
                //Add a new worksheet to workbook with the Datatable name
                // Excel.Worksheet excelWorkSheet = excelWorkBook.Sheets.Add();
                Excel.Worksheet excelWorkSheet = excelWorkBook.Sheets.Add();
                

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

        public string GetFolderName()
        {
            string folderName = Server.MapPath("~/Uploads/Facture");
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);

            }

            return folderName;
        }


        public FileResult downloadFactureAnnexe(string FileName)
        {
            string originPath = "~/Uploads/Facture/";
            string fullPath = originPath+ FileName;
            string rootpath = Server.MapPath("~/")+ "/Uploads/Facture/";

            byte[] fileBytes = System.IO.File.ReadAllBytes(rootpath + FileName);
            string fileName = FileName;
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }


    }
}
