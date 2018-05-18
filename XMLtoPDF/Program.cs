using RazorEngine;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace XMLtoPDF
{
    class Program
    {
        static void Main(string[] args)
        {
            Comprobante oComprobante;
            string pathXML = @"E:\ASP NET\XMLtoPDF\XMLtoPDF\xml\archivoXML3.xml";
            XmlSerializer oSerializer = new XmlSerializer(typeof(Comprobante));
            using (StreamReader reader = new StreamReader(pathXML))
            {
                oComprobante = (Comprobante)oSerializer.Deserialize(reader);
                //var tmp = oComprobante.Impuestos.TotalImpuestosTrasladados
                foreach(var oComplemento in oComprobante.Complemento)
                {
                    foreach(var oComplementoInterior in oComplemento.Any)
                    {
                        if (oComplementoInterior.Name.Contains("TimbreFiscalDigital"))
                        { 
                            XmlSerializer oSerializerComplemento = new XmlSerializer(typeof(TimbreFiscalDigital));
                            using (var readerComplemento = new StringReader(oComplementoInterior.OuterXml))
                            {
                                oComprobante.TimbreFiscalDigital = (TimbreFiscalDigital)oSerializerComplemento.Deserialize(readerComplemento);
                            }
                        }
                    }
                }
            }

            //Paso 2 Aplicando Razor y haciendo HTML a PDF

            string path = AppDomain.CurrentDomain.BaseDirectory + "/";
            string pathHTMLTemp = path + "miHTML.html";//temporal
            string pathHTPlantilla = path + "plantilla.html";
            string sHtml = GetStringOfFile(pathHTPlantilla);
            string resultHtml = "";
            resultHtml = Razor.Parse(sHtml, oComprobante);

            Console.WriteLine(resultHtml);
            //Creamos el archivo temporal
            File.WriteAllText(pathHTMLTemp, resultHtml);
            string pathWKHTMLTOPDF = @"E:\ASP NET\XMLtoPDF\XMLtoPDF\wkhtmltopdf\wkhtmltopdf.exe";
            ProcessStartInfo oProcessStartInfo = new ProcessStartInfo();
            oProcessStartInfo.UseShellExecute = false;
            oProcessStartInfo.FileName = pathWKHTMLTOPDF;
            oProcessStartInfo.Arguments = "miHTML.html miPDF.pdf";
            using(Process oProcess = Process.Start(oProcessStartInfo))
            {
                oProcess.WaitForExit();
            }
            //eliminamos el archivo temporal
            File.Delete(pathHTMLTemp);
            Console.Read();
        }

        private static string GetStringOfFile(string pathFile)
        {
            return File.ReadAllText(pathFile); 
        }

    }
}
