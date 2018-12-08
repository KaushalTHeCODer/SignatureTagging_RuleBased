using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ConsoleApp1
{
    class XMLParserForNuance
    {
        List<Dictionary<string, string>> cordinates = new List<Dictionary<string, string>>();
        public void AddToMarkList(XmlNode xmlNode, string pgNo, string height, string width)
        {
            var AttrList = xmlNode.Attributes;
            Dictionary<string, string> cordinate = new Dictionary<string, string>();
            foreach (XmlAttribute attr in AttrList)
            {
                cordinate.Add(attr.Name.ToString(), attr.Value.ToString());
            }
            cordinate.Add("PageNo", pgNo);
            cordinate.Add("Height", height);
            cordinate.Add("Width", width);
            cordinates.Add(cordinate);

        }
        public List<Dictionary<string, string>> ReadXml(string filename)
        {
            XmlDocument XDoc = new XmlDocument();
            
            XDoc.Load(@"C:\Users\kprajapati\Documents\Projects\SignatureTagging\XML files\" + filename+".xml");

            XmlNodeList XList = XDoc.SelectNodes("//*");
          //  List<Dictionary<string, string>> cordinates = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> Page = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> Metadata = new List<Dictionary<string, string>>();
            string pgNo = "NOT";
            string height = "NOT";
            string width = "NOT";

            for (int i = 0; i < XList.Count; i++)
            {
                
                if(XList[i].Name == "source")
                {
                    var AttrList = XList[i].Attributes;
                    Dictionary<string, string> metadata = new Dictionary<string, string>();
                    foreach (XmlAttribute attr in AttrList)
                    {
                        if(attr.Name == "pageNo")
                        {
                            pgNo = attr.Value;
                        }
                        
                        metadata.Add(attr.Name.ToString(), attr.Value.ToString());
                       
                    }
                    Page.Add(metadata);
                }
                if (XList[i].Name == "theoreticalPage")
                {
                    var AttrList = XList[i].Attributes;
                    Dictionary<string, string> metadata = new Dictionary<string, string>();
                    foreach (XmlAttribute attr in AttrList)
                    {
                        if(attr.Name == "height")
                        {
                            height = attr.Value;
                        }
                        if(attr.Name == "width")
                        {
                            width = attr.Value;
                        }
                        metadata.Add(attr.Name.ToString(), attr.Value.ToString());
                        
                    }
                    Metadata.Add(metadata);
                }
                
                //Signatures(s) of Borrower(s)
                //loan originator signature
                if (XList[i].Name =="para" & (XList[i].InnerText.ToLower().StartsWith("signature(s)ofborrower(s)") |
                    XList[i].InnerText.Contains("LoanOriginator'sSignature") | XList[i].InnerText.ToLower().StartsWith("signsignatureof")
                    | XList[i].InnerText.ToLower() == "signature(seal)"))
                {
                  
                    AddToMarkList(XList[i], pgNo, height, width);
                }

                //I ******* the undersigned 
                    if (XList[i].InnerText.StartsWith("I") && XList[i].InnerText.ToLower().Contains("undersigned") 
                    | XList[i].InnerText.StartsWith("InWitnessWhereof") )
                {
                   
                    int k = i + 1;
                    while (XList[k].Name != "rulerline")
                    {
                        k++;
                    }
                    if (XList[k].Name == "rulerline")
                    {
                        AddToMarkList(XList[k], pgNo, height, width);
                       
                    }
                   // Console.WriteLine(XList[k+1].InnerText);
                }
                //witness my hands ---- signature
                if (XList[i].InnerText.ToLower().StartsWith("signature"))
                {
                    int j = i-1;
                    while(XList[j].Name != "para")
                    {
                        j--;
                    }
                  
                    if (XList[j].Name == "para" & ( XList[j].InnerText.ToLower().StartsWith("witness")))
                    {

                        var AttrList = XList[i].Attributes;
                        Dictionary<string, string> cordinate = new Dictionary<string, string>();
                        foreach (XmlAttribute attr in AttrList)
                        {
                            cordinate.Add(attr.Name.ToString(), attr.Value.ToString());
                        }
                        cordinate.Add("PageNo", pgNo);
                        cordinate.Add("Height", height);
                        cordinate.Add("Width", width);
                        cordinates.Add(cordinate);
                    }
                }
                if(XList[i].Name == "table")
                {
                    foreach(XmlNode x in XList[i].ChildNodes)
                    {
                        //Console.WriteLine(x.Name);
                        
                        if(x.Name == "cell" && x.InnerText.ToLower().Contains("signature") )
                        {
                            //Console.WriteLine(x.InnerText);
                            //Console.WriteLine(pgNo);
                            if (x.HasChildNodes)
                            {
                               
                                AddToMarkList(x.LastChild, pgNo, height, width);
                            }
                        }
                        
                    }
                   
                }

                //Search for a line
                if (XList[i].Name == "rulerline")
                {
                    int k = i - 1;
                    while(XList[k].Name != "para")
                    {
                        k--;
                    }
                    
                    //to handle cases for two signatures in a single line
                    if (XList[i+1].Name == "rulerline")
                    { 

                        var para = XList[i+1].NextSibling;
                        
                        if (para != null && para.InnerText == "BorrowerSignatureDateBorrowerSignatureDate")
                        {
                            
                            AddToMarkList(XList[i], pgNo, height, width);
                            
                            AddToMarkList(XList[i+1], pgNo, height, width);
                        }
                        if (para != null && para.InnerText == "Name:[SettlementAgentName]")
                        {
                            AddToMarkList(XList[i], pgNo, height, width);
                        }
                        //Console.WriteLine(para.InnerText);
                    }
                   

                    //to handle cases where only single signature area is present
                    else if (XList[i+1].Name == "para" | XList[i+1].Name == "section" 
                        | XList[i + 1].Name == "dd")
                    {
                        var para = XList[i + 1];
                        if (para != null && para.HasChildNodes)
                        {
                            foreach (XmlNode childNode in para.ChildNodes)
                            {
                                if (childNode.InnerText.EndsWith("Date") | childNode.InnerText.EndsWith("(Date)")
                                    | childNode.InnerText.EndsWith("DATE")| childNode.InnerText.StartsWith("Veteran-")
                                    | childNode.InnerText.StartsWith("Borrower") | childNode.InnerText.Contains("Signature") |
                                    childNode.InnerText.StartsWith("Signature") | childNode.InnerText.StartsWith("Owner")|
                                     childNode.InnerText.StartsWith("Grantor") | childNode.InnerText.StartsWith("Applicant")
                                     | childNode.InnerText.ToLower().Contains("non-obligatedborrower")
                                   | childNode.InnerText.StartsWith("Consumer")| childNode.InnerText.ToLower().Contains("non-borrowingspouse")
                                    |childNode.InnerText == "OnbehalfoftheGrantees" | childNode.InnerText.Contains("Co-Borrower")
                                   )
                                {
                                    AddToMarkList(XList[i], pgNo, height, width);
                                    
                                }
                                //Console.WriteLine(childNode.Name);

                                //Console.WriteLine(childNode.InnerText);
                                //

                            }
                        }

                    }

                    if ( XList[k].InnerText.ToLower() == "affiant:" | XList[k].InnerText.ToLower() == "signature"
                       // |XList[k].InnerText.ToLower() == "(seal)"| XList[k].InnerText.EndsWith("NOTICEOFRIGHTTOCANCEL.")
                        )
                    {
                        AddToMarkList(XList[k], pgNo, height, width);
                    }

                }
                //to handle cases where signature is present inside a frame "I WISH TO CANCEL"
                else if (XList[i].Name == "frame")
                {
                    if (XList[i].HasChildNodes)
                    {
                        foreach(XmlNode xmlNodeChild in XList[i].ChildNodes)
                        {
                            if(xmlNodeChild.InnerText == "ConsumerSignatureDate" 
                                | xmlNodeChild.InnerText == "IWISHTOCANCEL")
                            {
                                AddToMarkList(XList[i].LastChild, pgNo, height, width);
                            }
                        }
                    }
                }
            }
            //foreach(Dictionary<string,string> item in cordinates)
            //{
            //    foreach(var x in item.Values)
            //    {
            //        //Console.WriteLine(x.Key);
            //        Console.WriteLine(x);
            //    }
            //    Console.WriteLine();
            //}
            //Console.ReadLine();
            return cordinates;
        }
    }
}
