using System;
using System.Collections.Generic;
using LINQBridge.VisualStudio.Properties;

namespace LINQBridge.VisualStudio
{

    public enum VisualStudioVersion
    {
        VS2010,
        VS2012

    }

    public static class VisualStudioOptions
    {
        private static readonly string MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static readonly Dictionary<string, List<string>> VisualStudioPaths = new Dictionary<string, List<string>>(){
            {
                "10.0", new List<string>()
                                        {
                                            MyDocuments + Resources.VS2010Path1,
                                            MyDocuments + Resources.VS2010Path2
                                        }},
            
            {
                "11.0", new List<string>()
                                            {
                                                MyDocuments + Resources.VS2012Path1
                                        }}
            };
    }
}
