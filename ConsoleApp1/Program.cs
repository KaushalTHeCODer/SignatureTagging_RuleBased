using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using iTextSharp;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using iTextSharp.text;
using System.IO;
using iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup;

namespace ConsoleApp1
{
    class Program
    {
       // public static int pgno = 0;
        
       // public static float height = 0;
        static void Main(string[] args)
        {
            string filename = "2018-09-05_03-27-41-PM_f032c166f449443780c9f55aa746f7db";
            XMLParserForNuance parser = new XMLParserForNuance();
            List<Dictionary<string, string>> coordinates =  parser.ReadXml(filename);

           //PdfReader pdfReader = new PdfReader("C:\\Users\\kprajapati\\Documents\\Projects\\SignatureTagging\\PdftoworkOn\\PdfToWorkOn2.pdf");
            int GlobalIteration = 0;
            

            

            foreach (Dictionary<string,string> cordinate in coordinates)
            {
                try
                {


                    float left_nu = float.Parse(cordinate["l"]);
                    float top_nu = float.Parse(cordinate["t"]);
                    float right_nu = float.Parse(cordinate["r"]);
                    float bottom_nu = float.Parse(cordinate["b"]);
                    int pgn = Convert.ToInt32(cordinate["PageNo"]);
                    
                    PdfReader pdfReader = new PdfReader("C:\\Users\\kprajapati\\Documents\\Projects\\SignatureTagging\\PdftoworkOn\\Output\\"+filename+".pdf");


                    float page_width_nu = float.Parse(cordinate["Width"]);
                    float page_height_nu = float.Parse(cordinate["Height"]);

                    float page_width_itext = pdfReader.GetPageSize(pgn).Width;
                    float page_height_itext = pdfReader.GetPageSize(pgn).Height;

                    float relative_left = left_nu / page_width_nu;
                    float relative_right = right_nu / page_width_nu + 0.01f;
                    float relative_top = top_nu / page_height_nu;
                    float relative_bottom = bottom_nu / page_height_nu - 0.02f;

                    float itext_pos_left = page_width_itext - (relative_left * page_width_itext);
                    float itext_pos_right = page_width_itext - (relative_right * page_width_itext);
                    float itext_pos_top = page_height_itext - (relative_top * page_height_itext);
                    float itext_pos_bottom = page_height_itext - (relative_bottom * page_height_itext);


                    
                    string file = "C:\\Users\\kprajapati\\Documents\\Projects\\SignatureTagging\\PdftoworkOn\\Output\\" + filename + ".pdf";
                    //   string oldchar = "orignals.pdf"; string repChar = "copy.pdf";
                    if (GlobalIteration == 0)
                    {
                        string repText = filename + GlobalIteration + ".pdf";
                        Mark(file, filename+".pdf", repText, itext_pos_left, itext_pos_bottom, itext_pos_right, itext_pos_top, pgn);
                        GlobalIteration++;
                    }
                    else
                    {
                        string orig = filename + (GlobalIteration - 1) + ".pdf";
                        string repText = filename + GlobalIteration + ".pdf";
                       
                        var input1 = "C:\\Users\\kprajapati\\Documents\\Projects\\SignatureTagging\\PdftoworkOn\\Output\\" + orig;
                        Mark(input1, orig, repText, itext_pos_left, itext_pos_bottom, itext_pos_right, itext_pos_top, pgn);
                        File.Delete(input1);
                        
                        GlobalIteration++;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                  
                }

            }
            
            Console.ReadLine();

        }
        public static void Mark(string file,string oldFile,string newFile, float itext_pos_left, float itext_pos_bottom,float itext_pos_right,float itext_pos_top,int pgn)
        {
            PdfReader pdfReader = new PdfReader(file);
            

            PdfStamper stamper = new PdfStamper(pdfReader, new FileStream(file.Replace(oldFile, newFile), FileMode.Create, FileAccess.Write));

            

            List<PdfCleanUpLocation> cleanUpLocations = new List<PdfCleanUpLocation>();
            cleanUpLocations.Add(new PdfCleanUpLocation(pgn, new iTextSharp.text.Rectangle(itext_pos_left, itext_pos_bottom, itext_pos_right, itext_pos_top), BaseColor.RED));
            PdfCleanUpProcessor cleaner = new PdfCleanUpProcessor(cleanUpLocations, stamper);
            cleaner.CleanUp();
            stamper.Close();
            pdfReader.Close();
            stamper.Dispose();
            pdfReader.Dispose();

            
        }
       
    }
}
