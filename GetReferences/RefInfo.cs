using System;
using System.Collections.Generic;

namespace GetReferences
{
    public class RefInfo
    {
        private string seperator = ",";
        //private string seperator = "\n";
        public string ProjectPath { get; set; }
        public string DllPath { get; set; }
        public bool CopyLocal { get; set; }

        List<Dictionary<int, RefInfo>> listDicReferencesInfo { get; set; }

        public List<RefInfo> RefsDll { get; set; }

        public string DllName
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(DllPath);
            }
        }

        public string ProjectName
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(this.ProjectPath);
            }
        }

        public string ConvertToText(int maxHierachy, int hierachyIndex)
        {
            string[] resultProjectPath = new string[maxHierachy];
            resultProjectPath[hierachyIndex] = this.ProjectPath;
            return $"{string.Join(seperator, resultProjectPath)} {this.seperator} {this.DllPath} {this.seperator} {CopyLocal.ToString()}";
        }

    }
}
