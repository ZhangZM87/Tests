using Microsoft.Win32;
using PdfSharp.Pdf.IO;
using System;
using System.Drawing.Printing;
using System.IO;

namespace PdfPrintingNetLastTest
{
    internal class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            var fileDialog = new OpenFileDialog();
            if (!(fileDialog.ShowDialog() ?? false)) return;

            var filePath = fileDialog.FileName;
            //using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            //var pdf = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

            //Console.WriteLine(pdf.PageCount);
            //Console.ReadKey();
            //filePath = @"C:\Users\MC\Downloads\762676632499539968\762668094125146112.pdf";
            //filePath = @"C:\Users\MC\Documents\WXWork\1688854702617892\Cache\File\2025-03\756089615662219264.pdf";
            //filePath = @"C:\Users\MC\Documents\WXWork\1688854702617892\Cache\File\2025-02\产险-机构证照-负责人身份证-山东枣庄_盖章(2)_合并(1)(1).pdf";

            var printerName = "RICOH MP C2504 PCL 6";
            //printerName = "Brother DCP-T820DW Printer";
            //printerName = Properties.Settings.Default.PrinterName;
            //printerName = "iR-ADV C3530 III (副本 1)";
            //printerName = "Canon G5080 series";
            var pdfPrint = new PdfPrintingNet.PdfPrint("93701560", "g/4JFMjn6Kv2jmBSKcuys7sS36TlrC4cL9ns1Nr3ATRC5sf7uKioYeCwdq/YtQSPSaPx2LVZrnnhpgtFPMk7c0i9pUuAjMYYuC7PQ5p2Mc0=");
            pdfPrint.PrinterName = printerName;
            pdfPrint.PrintInColor = false;
            pdfPrint.DuplexType  = Duplex.Simplex;
            //pdfPrint.Copies = 1;
            //pdfPrint.Pages = "1-1";
            //pdfPrint.Collate = true;
            //var re = new PrinterResolution();
            //re.X = 600;
            //re.Y = 600;
            //re.Kind = PrinterResolutionKind.High;
            //pdfPrint.PrinterResolution = re;
            var resultMessage = "";

            //这个打印文件使用默认的打印方法打印，缺少部分文字， 使用PrintWithAdobe方法能够打印完整


            // Print complete
            var result = pdfPrint.PrintWithAdobe(filePath, ref resultMessage);

            // Print missing text
            //var result = pdfPrint.Print(filePath);


            Console.WriteLine(resultMessage);
            Console.ReadKey();
        }
    }
}
