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
using Word = Microsoft.Office.Interop.Word;
using System.IO;
using WakilRecouvrement.Domain.Entities;
using PagedList;
using WakilRecouvrement.Data;
using MyFinance.Data.Infrastructure;
using System.Globalization;
using System.Text;
using iText.Layout.Font;
using System.Drawing;
using iText.Kernel.Font;
using iText.IO.Font;
using System.Xml;
using System.Threading;
using System.Windows.Forms;
using System.IO.Compression;

namespace WakilRecouvrement.Web.Controllers
{
    public class FactureController : Controller
    {


     

        public FactureController ()
        {
            
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

                    listItems.Add(new SelectListItem { Selected = true, Text = "Tous les lots", Value = "0" });

                    Lots.DistinctBy(l => l.NumLot).ForEach(l => {
                        listItems.Add(new SelectListItem { Text = "Lot " + l.NumLot, Value = l.NumLot });
                    });

                    return listItems;

                }
            }
                    
        }

        public ActionResult genererFacture(int? page)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    FactureService factureService = new FactureService(UOW);
                    List<Facture> factureList = factureService.GetAll().OrderByDescending(f => f.DateExtrait).ToList();
                    ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");


                    ViewBag.total = factureList.Count();

                    int pageSize = 10;
                    int pageNumber = (page ?? 1);

                    return View(factureList.ToPagedList(pageNumber, pageSize));

                }
            }
        }


        public List<ClientAffecteViewModel> getTraitHist(int idAff, FormulaireService FormulaireService, AffectationService AffectationService, LotService LotService)
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

                    return JoinedList;

                    
        }

        public ActionResult extraireFacture(string numLot,string factureNum,string pourcentage, string debutDate, string finDate)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    LotService LotService = new LotService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    FactureService factureService = new FactureService(UOW);

                    ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");

                    DateTime startDate = DateTime.Parse(debutDate);
                    DateTime endDate = DateTime.Parse(finDate);
                    double trancheTot = 0;
                    double soldeTot = 0;
                    double tot = 0;
                    float revenuParOp = float.Parse(pourcentage.Replace(".", ","));
                    List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();
                    List<ClientAffecteViewModel> TempJoinedList = new List<ClientAffecteViewModel>();
                    List<ClientAffecteViewModel> AnnexeJoinedList = new List<ClientAffecteViewModel>();

                    if (numLot == "0")
                    {

                        JoinedList = (from f in FormulaireService.GetAll()
                                      join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                      join l in LotService.GetAll() on a.LotId equals l.LotId
                                      where f.Status == Domain.Entities.Status.VERIFIE
                                      select new ClientAffecteViewModel
                                      {

                                          Formulaire = f,
                                          Affectation = a,
                                          Lot = l,

                                      }).Where(j => j.Formulaire.TraiteLe.Date >= startDate.Date && j.Formulaire.TraiteLe.Date <= endDate.Date).ToList();

                    }
                    else
                    {

                        JoinedList = (from f in FormulaireService.GetAll()
                                      join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                      join l in LotService.GetAll() on a.LotId equals l.LotId
                                      where f.Status == Domain.Entities.Status.VERIFIE && l.NumLot.Equals(numLot)
                                      select new ClientAffecteViewModel
                                      {

                                          Formulaire = f,
                                          Affectation = a,
                                          Lot = l,

                                      }).Where(j => j.Formulaire.TraiteLe.Date >= startDate.Date && j.Formulaire.TraiteLe.Date <= endDate.Date).ToList();
                        
                    }

                    foreach (ClientAffecteViewModel cvm in JoinedList)
                    {

                        if (cvm.Formulaire.EtatClient == Domain.Entities.Note.SOLDE_TRANCHE)
                        {

                            trancheTot += (cvm.Formulaire.MontantVerseDeclare * revenuParOp) / 100;
                            cvm.vers = cvm.Formulaire.MontantVerseDeclare;
                            cvm.recouvre = cvm.Formulaire.MontantVerseDeclare;
                            AnnexeJoinedList.Add(cvm);

                        }

                        if (cvm.Formulaire.EtatClient == Domain.Entities.Note.SOLDE)
                        {
                            
                            TempJoinedList = getTraitHist(cvm.Affectation.AffectationId,FormulaireService,AffectationService,LotService);
                            if (TempJoinedList.Count() == 0)
                            {

                                soldeTot += (cvm.Formulaire.MontantDebInitial * revenuParOp) / 100;
                                cvm.recouvre = cvm.Formulaire.MontantDebInitial;
                                cvm.vers = cvm.Formulaire.MontantVerseDeclare;

                                AnnexeJoinedList.Add(cvm);
                            }
                            else
                            {

                                soldeTot += (TempJoinedList.FirstOrDefault().Formulaire.MontantDebMAJ * revenuParOp) / 100;
                                cvm.vers = cvm.Formulaire.MontantVerseDeclare;
                                cvm.recouvre = TempJoinedList.FirstOrDefault().Formulaire.MontantDebMAJ;
                                AnnexeJoinedList.Add(cvm);

                            }


                        }


                    }

                    string lotsNames = "";
                    List<string> listNameLot = JoinedList.DistinctBy(j => j.Lot.NumLot).Select(l => l.Lot.NumLot).ToList();

                    if (listNameLot.Count() == 1)
                    {
                        lotsNames = listNameLot.FirstOrDefault();
                    }
                    else if (listNameLot.Count() > 1)
                    {
                        string lastlots = JoinedList.DistinctBy(j => j.Lot.NumLot).Select(l => l.Lot.NumLot).LastOrDefault();
                        listNameLot.RemoveAt(listNameLot.Count - 1);
                        string lots = String.Join(", ", listNameLot);
                        lotsNames = lots + " et " + lastlots;
                    }


                    tot = soldeTot + trancheTot;
                    FactureContent factureContent = new FactureContent();
                    factureContent.FacNum = factureNum;
                    factureContent.Date = DateTime.Today;
                    factureContent.Beneficiere = "Zitouna Bank";
                    factureContent.PrixHT = tot;
                    factureContent.PrixTVA = (tot * 19) / 100;
                    factureContent.TimbreFiscal = 0.600;

                    factureContent.PrixTTC = tot + factureContent.PrixTVA;

                    string annexeFileName = "Annexe_" + DateTime.Now.ToString("dd.MM.yyyy") + "_" + ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds() + ".xlsx";
                    string factureFileName = "Facture_" + DateTime.Now.ToString("dd.MM.yyyy") + "_" + ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds() + ".pdf";
                    string pathAnnexe = GetFolderName() + "/" + annexeFileName;
                    string pathFacture = GetFolderName() + "/" + factureFileName;
                    Facture facture = new Facture()
                    {

                        AnnexePathName = annexeFileName,
                        FacturePathName = factureFileName,
                        DateDeb = startDate,
                        DateFin = endDate,
                        DateExtrait = DateTime.Now

                    };
                    factureService.Add(facture);
                    factureService.Commit();
                    GenerateExcel(GenerateDatatableFromJoinedList(AnnexeJoinedList), pathAnnexe, String.Format("{0:0.000}", AnnexeJoinedList.Sum(j => j.recouvre)));
                    GeneratePDF(pathFacture, lotsNames, factureContent);

                    return RedirectToAction("genererFacture", new { page = 1 });

                }
            }


        }

        public void GeneratePDF(string path,string lotsNames,FactureContent factureContent)
        {
            string date = factureContent.Date.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

            PdfWriter writer = new PdfWriter(path);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf);
            Paragraph title = new Paragraph("Facture N° : "+ factureContent.FacNum).SetTextAlignment(TextAlignment.CENTER).SetBold().SetFontSize(20);
            
            Paragraph dateLabel = new Paragraph("Date : ").SetBold().SetTextAlignment(TextAlignment.LEFT).SetFontSize(15);
            Paragraph dateValue = new Paragraph(date).SetTextAlignment(TextAlignment.LEFT).SetFontSize(15);
            
            Paragraph beneficiereLabel = new Paragraph("Bénéficiaire : ").SetBold().SetTextAlignment(TextAlignment.LEFT).SetFontSize(15);
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
                      .Add(new Paragraph(String.Format("{0:0.000}", factureContent.PrixTTC+factureContent.TimbreFiscal)));


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

        public void GenerateWordForLettre(string path,LettreContent lettreContent)
        {

            Microsoft.Office.Interop.Word.Application winword = new Microsoft.Office.Interop.Word.Application();
            winword.ShowAnimation = false;
            winword.Visible = false;
            object missing = System.Reflection.Missing.Value;
            Microsoft.Office.Interop.Word.Document document = winword.Documents.Add(ref missing, ref missing, ref missing, ref missing);

            Word.Paragraph paragraphEmpty = document.Content.Paragraphs.Add(missing);
            paragraphEmpty.SpaceBefore = 55;
            paragraphEmpty.Range.InsertParagraphAfter();

            Word.Paragraph paragraphTitle = document.Content.Paragraphs.Add(missing);
            paragraphTitle.Range.Text = "من شركة الوكيل لاستخلاص الديون";
            paragraphTitle.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            paragraphTitle.Range.Font.NameBi = "Arial";
            paragraphTitle.Range.Font.SizeBi = 28;
            paragraphTitle.SpaceAfter = 50;
            paragraphTitle.Range.InsertParagraphAfter();

            object oEndOfDoc = "\\endofdoc";
            Word.Range tblRange = document.Bookmarks[oEndOfDoc].Range;
            Word.Table table = document.Tables.Add(tblRange, 2,3);

            table.Borders.Enable = 0;
            table.Cell(1, 1).Range.ParagraphFormat.BaseLineAlignment = Word.WdBaselineAlignment.wdBaselineAlignFarEast50;
            table.Cell(1, 1).Width = 70;
            table.Cell(1, 1).Range.Text = "Nom et prénom:";
            table.Cell(1, 1).Range.Font.Name = "Arial";
            table.Cell(1, 1).Range.Font.Size = 14;
            table.Cell(1, 1).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;

            table.Cell(1, 2).Range.ParagraphFormat.BaseLineAlignment = Word.WdBaselineAlignment.wdBaselineAlignFarEast50;
            table.Cell(1, 2).Range.Text = lettreContent.NomClient;
            table.Cell(1, 2).Width = 320;
            table.Cell(1, 2).Range.Font.Name = "Arial";
            table.Cell(1, 2).Range.Font.Size = 18;
            table.Cell(1, 2).Range.Font.Bold = 1;
            table.Cell(1, 2).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            //table.Cell(1, 2).VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;
            
            table.Cell(1, 3).Range.ParagraphFormat.BaseLineAlignment = Word.WdBaselineAlignment.wdBaselineAlignFarEast50;
            table.Cell(1, 3).Width = 70;
            table.Cell(1, 3).Range.Text = ":الاسم واللقب";
            table.Cell(1, 3).Range.Font.NameBi = "Arial";
            table.Cell(1, 3).Range.Font.SizeBi = 16;
            table.Cell(1, 3).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;

            table.Cell(2, 1).Range.ParagraphFormat.BaseLineAlignment = Word.WdBaselineAlignment.wdBaselineAlignFarEast50;
            table.Cell(2, 1).Width = 70;
            table.Cell(2, 1).Range.Text = "Agence:";
            table.Cell(2, 1).Range.Font.Name = "Arial";
            table.Cell(2, 1).Range.Font.Size = 14;
            table.Cell(2, 1).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            table.Cell(2, 2).Range.ParagraphFormat.BaseLineAlignment = Word.WdBaselineAlignment.wdBaselineAlignFarEast50;

            table.Cell(2, 2).Range.Text = lettreContent.Agence;
            table.Cell(2, 2).Width = 320;
            table.Cell(2, 2).Range.Font.Name = "Arial";
            table.Cell(2, 2).Range.Font.Size = 18;
            table.Cell(2, 2).Range.Font.Bold = 1;
            table.Cell(2, 2).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

            table.Cell(2, 3).Range.ParagraphFormat.BaseLineAlignment = Word.WdBaselineAlignment.wdBaselineAlignFarEast50;
            table.Cell(2, 3).Width = 70;
            table.Cell(2, 3).Range.Text = ":الفرع";
            table.Cell(2, 3).Range.Font.NameBi = "Arial";
            table.Cell(2, 3).Range.Font.SizeBi = 16;
            table.Cell(2, 3).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;

            Word.Paragraph paragraphBody1 = document.Content.Paragraphs.Add(missing);
            paragraphBody1.Range.Text = "تحية طيبة وبعد";
            paragraphBody1.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
            paragraphBody1.Range.Font.NameBi = "Arial";
            paragraphBody1.Range.Font.SizeBi = 20;
            paragraphBody1.Range.InsertParagraphAfter();

            Word.Paragraph paragraphBody2 = document.Content.Paragraphs.Add(missing);
            paragraphBody2.Range.Text = "في إطار تكليفنا لمساعدتكم على التواصل لتسوية وضعية دينكم تجاه مصرف الزيتونة فرع مركزي والمتعلق بحسابكم البنكي الخاص، نرجو منكم التوجه إلى الفرع المذكور أعلاه أو الاتصال بالشركة لإيجاد أفضل الحلول المناسبة التي تضمن لكم أحسن طريقة لتطهير دينكم";
            paragraphBody2.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
            paragraphBody2.Range.Font.NameBi = "Arial";
            paragraphBody2.Range.Font.SizeBi = 20;
            paragraphBody2.Range.InsertParagraphAfter();
            
            Word.Paragraph footer = document.Content.Paragraphs.Add(missing);
            footer.Range.Text = "رهاننا وعيكم وحسن تفهمكم";
            footer.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            footer.Range.Font.NameBi = "Arial";
            footer.Range.Font.SizeBi = 20;
            footer.SpaceAfter = 30;
            footer.Range.InsertParagraphAfter();

           
            object filename = path;
            document.SaveAs2(ref filename);
            document.Close(ref missing, ref missing, ref missing);
            document = null;
            winword.Quit(ref missing, ref missing, ref missing);
            winword = null;
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
                   Versement = String.Format("{0:0.000}", j.vers),
                   MontantDebInitial = String.Format("{0:0.000}", j.Lot.SoldeDebiteur),
                   MontantRecouvre = String.Format("{0:0.000}", j.recouvre)

               }

               ).ToList();
            
            dataTable.Columns.Add("IDClient", typeof(string));
            dataTable.Columns.Add("Compte", typeof(string));
            dataTable.Columns.Add("NomClient", typeof(string));
            dataTable.Columns.Add("Solde débiteur", typeof(string));
            dataTable.Columns.Add("Montant versé", typeof(string));
            dataTable.Columns.Add("Montant recouvré", typeof(string));

            foreach (FormulaireExportable c in newList)
            {

                DataRow row = dataTable.NewRow();
                row["IDClient"] = c.IDClient;
                row["Compte"] = c.Compte;
                row["NomClient"] = c.NomClient;
                row["Solde débiteur"] = c.MontantDebInitial;
                row["Montant versé"] = c.Versement;
                row["Montant recouvré"] = c.MontantRecouvre;
                
                dataTable.Rows.Add(row);

            }

            return dataTable;
        }

        public static void GenerateExcel(DataTable dataTable, string path,string tot)
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
                excelWorkSheet.Cells.EntireColumn.NumberFormat = "@";
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
                excelWorkSheet.Cells[table.Rows.Count+2, 5] = "Total";
                excelWorkSheet.Cells[table.Rows.Count+2, 5].Font.Size = 14;
                excelWorkSheet.Cells[table.Rows.Count+2, 5].Font.Bold = true;
                excelWorkSheet.Cells[table.Rows.Count+2, 5].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                excelWorkSheet.Cells[table.Rows.Count+2, 5].Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                excelWorkSheet.Cells[table.Rows.Count+2, 5].Borders.Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin;
                excelWorkSheet.Cells[table.Rows.Count+2, 5].Borders.Weight = 2;
               
                excelWorkSheet.Cells[table.Rows.Count+2, 6] = tot;
                excelWorkSheet.Cells[table.Rows.Count+2, 6].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                excelWorkSheet.Cells[table.Rows.Count+2, 6].Font.Size = 12;
                excelWorkSheet.Cells[table.Rows.Count+2, 6].Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                excelWorkSheet.Cells[table.Rows.Count+2, 6].Borders.Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin;
                excelWorkSheet.Cells[table.Rows.Count+2, 6].Borders.Weight = 2;

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
        public string GetFolderNameForLettre()
        {
            string folderName = Server.MapPath("~/Uploads/Lettre");
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);

            }

            return folderName;
        }

       
        public bool lettreIsTrue(FormulaireService formulaireService,int idAff)
        {

            int nb = formulaireService
                .GetMany(f=>f.AffectationId == idAff)
                .Where(f=>f.EtatClient == Note.AUTRE|| f.EtatClient == Note.A_VERIFIE || f.EtatClient == Note.RACCROCHE || f.EtatClient == Note.RAPPEL || f.EtatClient == Note.RDV || f.EtatClient == Note.REFUS_PAIEMENT || f.EtatClient == Note.SOLDE || f.EtatClient == Note.SOLDE_TRANCHE)
                .Count();
                                                      
            if(nb==0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public ActionResult Renseigner(int? page)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LettreService lettreService = new LettreService(UOW);
                    List<Lettre> lettreList = lettreService.GetAll().OrderByDescending(f => f.DateExtrait).ToList();
                    ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");


                    ViewBag.total = lettreList.Count();

                    int pageSize = 10;
                    int pageNumber = (page ?? 1);
                    return View(lettreList.ToPagedList(pageNumber, pageSize));

                }
            }


        }


        public DataTable GenerateDatatableFromJoinedListForLettre(List<LettreContent> list)
        {
            List<FormulaireExportable> newList = new List<FormulaireExportable>();
            DataTable dataTable = new DataTable();

            newList = list.Select(j =>
               new FormulaireExportable
               {
                   NumLot = j.NumLot,
                   Compte = j.Compte,
                   NomClient = j.NomClient,
                   Agence = j.Agence,
                   Adresse = j.Adresse

               }

               ).ToList();

            dataTable.Columns.Add("NumLot", typeof(string));
            dataTable.Columns.Add("Compte", typeof(string));
            dataTable.Columns.Add("NomClient", typeof(string));
            dataTable.Columns.Add("Agence", typeof(string));
            dataTable.Columns.Add("Adresse", typeof(string));

            foreach (FormulaireExportable c in newList)
            {

                DataRow row = dataTable.NewRow();
                row["NumLot"] = c.NumLot;
                row["Compte"] = c.Compte;
                row["NomClient"] = c.NomClient;
                row["Agence"] = c.Agence;
                row["Adresse"] = c.Adresse;

                dataTable.Rows.Add(row);

            }

            return dataTable;
        }

        public static void GenerateExcelForLettre(DataTable dataTable, string path)
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
                excelWorkSheet.Cells.EntireColumn.NumberFormat = "@";
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
                    if(i==1)
                    {
                        excelWorkSheet.Cells[1, i].ColumnWidth = 10;

                    }
                    else
                    {
                        excelWorkSheet.Cells[1, i].ColumnWidth = 40;
                    }

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
                        if(k==0)
                        {
                            excelWorkSheet.Cells[j + 2, k + 1].ColumnWidth = 10;

                        }
                        else
                        {
                            excelWorkSheet.Cells[j + 2, k + 1].ColumnWidth = 40;

                        }

                    }
                }

            }

            // excelWorkBook.Save(); -> this will save to its default location

            excelWorkBook.SaveAs(path); // -> this will do the custom

            excelWorkBook.Close();
            excelApp.Quit();
        }


        public ActionResult ExtraireLettreAction(string numLot,string debutDate,string finDate)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    LotService LotService = new LotService(UOW);
                    LettreService lettreService = new LettreService(UOW);
                    ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");

                    string lettreDir = GetFolderNameForLettre();
                    DateTime startDate = DateTime.Parse(debutDate);
                    DateTime endDate = DateTime.Parse(finDate);

                    if (!Directory.Exists(lettreDir))
                    {
                        Directory.CreateDirectory(lettreDir);
                    }

                    string lettreDirList = Directory.GetDirectories(lettreDir).Length + 1 + "_" + ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds() + "";

                    string dirLettre = "";

                    if (!Directory.Exists(lettreDir + "/" + lettreDirList))
                    {
                        Directory.CreateDirectory(lettreDir + "/" + lettreDirList);
                        dirLettre = lettreDir + "/" + lettreDirList;
                    }

                    List<LettreContent> lettreJoinedList = new List<LettreContent>();
                    if (numLot == "0")
                    {
                         lettreJoinedList = (from f in FormulaireService.GetAll()
                                                                join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                                                join l in LotService.GetAll() on a.LotId equals l.LotId
                                                                where (f.TraiteLe.Date >= startDate.Date && f.TraiteLe.Date <= endDate.Date) && (f.EtatClient == Note.FAUX_NUM || f.EtatClient == Note.NRP || f.EtatClient == Note.INJOIGNABLE) && lettreIsTrue(FormulaireService, a.AffectationId)
                                                                select new LettreContent
                                                                {
                                                                    NumLot = l.NumLot,
                                                                    Adresse = l.Adresse,
                                                                    NomClient = l.NomClient,
                                                                    Agence = l.DescIndustry,
                                                                    Compte = l.Compte

                                                                }).ToList();
                    }
                    else
                    {
                        lettreJoinedList = (from f in FormulaireService.GetAll()
                                            join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                            join l in LotService.GetAll() on a.LotId equals l.LotId
                                            where (f.TraiteLe.Date >= startDate.Date && f.TraiteLe.Date <= endDate.Date) && (f.EtatClient == Note.FAUX_NUM || f.EtatClient == Note.NRP || f.EtatClient == Note.INJOIGNABLE) && lettreIsTrue(FormulaireService, a.AffectationId) && (l.NumLot.Equals(numLot))
                                            select new LettreContent
                                            {
                                                NumLot = l.NumLot,
                                                Adresse = l.Adresse,
                                                NomClient = l.NomClient,
                                                Agence = l.DescIndustry,
                                                Compte = l.Compte

                                            }).ToList();
                    }

                    int x = 0;
                    ViewBag.total = lettreJoinedList.Count();
                    foreach (LettreContent lc in lettreJoinedList)
                    {

                        string path = dirLettre + "/" + x + "_" + "lettre" + "_" + lc.Compte + ".docx";

                        if (Directory.Exists(dirLettre))
                        {
                            GenerateWordForLettre(path, lc);
                        }
                        ViewData["currClient"] = x;
                        x++;
                    }

                    string zipPath = zipFolderResult(dirLettre);

                    string adressExcelPath = Server.MapPath("~/Uploads/Lettre/0_Result/")+ DateTime.Now.ToString("dd.MM.yyyy") + "_" + Path.GetFileName(dirLettre)+ ".xlsx";

                    GenerateExcelForLettre(GenerateDatatableFromJoinedListForLettre(lettreJoinedList), adressExcelPath);
                    Lettre lettre = new Lettre
                    {
                        DateDeb = startDate.Date,
                        DateFin = endDate.Date,
                        DateExtrait = DateTime.Now,
                        LettrePathName = zipPath,
                        LettreAdressPathName = Path.GetFileName(adressExcelPath)
                    };

                    lettreService.Add(lettre);
                    lettreService.Commit();

                    return RedirectToAction("Renseigner", new { page = 1 });
                }
            }
        }

        public string zipFolderResult(string path)
        {

            string destPath = Server.MapPath("~/Uploads/Lettre/0_Result");
            if(!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            string zipName = DateTime.Now.ToString("dd.MM.yyyy") + "_" + Path.GetFileName(path) + ".zip";
            ZipFile.CreateFromDirectory(path, destPath +"/" + zipName);

            return zipName;
        
        }


        public FileResult downloadFactureAnnexe(string FileName)
        {
            string originPath = "~/Uploads/Facture/";
            string fullPath = originPath+ FileName;
            string rootpath = Server.MapPath("~/")+ "/Uploads/Facture/";

            try
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(rootpath + FileName);
                string fileName = FileName;
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch (FileNotFoundException e)
            {
                return null;
            }
           
        }


        public FileResult downloadLettreZip(string FileName)
        {
           
            string rootpath = Server.MapPath("~/") + "/Uploads/Lettre/0_Result/";

            try
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(rootpath + FileName);
                string fileName = FileName;
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch (FileNotFoundException e)
            {
                return null;
            }

        }

    }
}
