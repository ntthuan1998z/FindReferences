using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GetReferences
{
    public class MainProgram
    {
        List<RefInfo> lstRefs = new List<RefInfo>();
        StringBuilder outputTxt = new StringBuilder();
        List<Dictionary<int, RefInfo>> printList = new List<Dictionary<int, RefInfo>>();

        public void Execute()
        {
            string path = @"D:\eShopOnWeb-main";
            string[] targetDlls = new string[] { "Infrastructure", "ApplicationCore", "Web" };

            this.lstRefs = GetAllReferences(path);
            
            
            foreach (string dll in targetDlls)
            {
                List<RefInfo> refsDll = GetReferencesOfDll(dll, dll);
                List<Dictionary<int, RefInfo>> listResult = this.ConvertToPrintList(refsDll);
                printList.AddRange(listResult);
            }
            this.CreateOutputFile();

            File.WriteAllText(@"D:\References.csv", outputTxt.ToString());
            File.WriteAllText(@"D:\References.txt", outputTxt.ToString());
        }

        private List<RefInfo> GetReferencesOfDll(string baseDll, string targetDll)
        {
            var lstReferences = this.lstRefs.Where(r => r.DllName.Equals(targetDll))
                                            .Select(r => new RefInfo
                                            {
                                                ProjectPath = r.ProjectPath,
                                                DllPath = r.DllPath,
                                                CopyLocal = r.CopyLocal,
                                                RefsDll = GetReferencesOfDll(baseDll, r.ProjectName)
                                            }).ToList();
            return lstReferences;
        }

        private List<Dictionary<int, RefInfo>> ConvertToPrintList(List<RefInfo> refInfos)
        {
            List<Dictionary<int, RefInfo>> listResult = new List<Dictionary<int, RefInfo>>();
            int hierachy = 0;
            //var rootRefInfo = refInfos.Skip(1).First();
            Dictionary<int, RefInfo> tempDic = new Dictionary<int, RefInfo>();
            //tempDic.Add(hierachy, rootRefInfo);

            var tempInfos = refInfos;

            while (tempInfos.Any())
            {
                tempDic.Add(++hierachy, tempInfos.First());
                
                if (tempInfos.First().RefsDll.Any())
                {
                    tempInfos = tempInfos.First().RefsDll;
                } else
                {
                    tempInfos.RemoveAt(0);
                    listResult.Add(tempDic);
                    tempDic = new Dictionary<int, RefInfo>();

                    if (refInfos.Any())
                    {
                        hierachy = 0;
                        tempInfos = refInfos;
                    }
                }

            }

            return listResult;
        }

        private void CreateOutputFile()
        {
            //Create output text
            int maxHierachy = this.printList.Max(i => i.Count());
            foreach (Dictionary<int, RefInfo> infos in this.printList.OrderBy(i => i.First().Value.DllName).ThenBy(i => i.First().Value.ProjectName).ThenBy(i => i.Count()))
            {
                foreach (var item in infos.OrderByDescending(i => i.Key))
                {
                    this.outputTxt.AppendLine($"{item.Key},{item.Value.ConvertToText(maxHierachy, infos.Max(i => i.Key) - item.Key)}");
                }
                //this.outputTxt.AppendLine("________________________________________________");
                this.outputTxt.AppendLine();
            }
        }

        private List<RefInfo> GetAllReferences(string folderPath)
        {
            string[] files = System.IO.Directory.GetFiles(folderPath, "*.csproj", SearchOption.AllDirectories);

            List<RefInfo> lstRefs = new List<RefInfo>();
            XNamespace msbuild = string.Empty;
            //XNamespace msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";
            foreach (string file in files)
            {
                XDocument projDefinition = XDocument.Load(file);
                var references = projDefinition
                    .Element(msbuild + "Project")
                    .Elements(msbuild + "ItemGroup")
                    .Elements(msbuild + "ProjectReference")
                    //    .Elements(msbuild + "Reference")
                    .Select(refElem => new RefInfo()
                    {
                        ProjectPath = file,
                        //Path = (refElem.Attribute("Include") == null ? "" : refElem.Attribute("Include").Value) + "\n" + (refElem.Element(msbuild + "HintPath") == null ? "" : refElem.Element(msbuild + "HintPath").Value) + "\n",
                        DllPath = refElem.Element(msbuild + "HintPath") == null ? refElem.Attribute("Include").Value : refElem.Element(msbuild + "HintPath").Value,
                        CopyLocal = refElem.Element(msbuild + "Private") == null ? true : bool.Parse(refElem.Element(msbuild + "Private").Value)
                    }).ToList();

                Console.WriteLine(file);

                lstRefs.AddRange(references);
            }
            return lstRefs;
        }
    }
}
