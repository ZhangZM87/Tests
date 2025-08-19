using Microsoft.Win32;
using System;

namespace PdfPrintingNetTest
{
    internal class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            //var fileDialog1 = new SaveFileDialog();

            //// 设置文件过滤器以仅显示 .pdf 文件
            //fileDialog1.Filter = "PDF Files (*.pdf)|*.pdf";  // 只显示 PDF 文件

            //if (!(fileDialog1.ShowDialog() ?? false)) return;


            
            var fileDialog = new OpenFileDialog();
            if (!(fileDialog.ShowDialog() ?? false)) return;
            var filePath = fileDialog.FileName;
            var printerName = "RICOH MP C2504 PCL 6";
            //printerName = "Brother DCP-T820DW Printer";
            //printerName = "EPSON WF-C5390 Series";
            //printerName = "Canon G5080 series";
            var pdfPrint = new PdfPrintingNet.PdfPrint("", "");
            pdfPrint.PrinterName = printerName;
            pdfPrint.PrintInColor = false;
            //pdfPrint.Copies = 1;

            var resultMessage = "";
            //var result = pdfPrint.PrintWithAdobe(filePath, ref resultMessage);
            var result = pdfPrint.Print(filePath);
            Console.WriteLine(resultMessage);
            Console.ReadKey();
        }
    }
}
