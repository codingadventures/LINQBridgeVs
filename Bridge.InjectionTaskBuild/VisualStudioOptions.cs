using System;
using System.Collections.Generic;
using Bridge.InjectionBuildTask.Properties;

namespace Bridge.InjectionBuildTask
{

    public enum VisualStudioVersion
    {
        VS2010,
        VS2012

    }

    public static class VisualStudioOptions
    {
        private static readonly string MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);


        public static readonly Dictionary<VisualStudioVersion, List<string>> VisualStudioPaths = new Dictionary<VisualStudioVersion, List<string>>(){
            {
                VisualStudioVersion.VS2010, new List<string>()
                                        {
                                            MyDocuments + Resources.VS2010Path1,
                                            MyDocuments + Resources.VS2010Path2
                                        }},
            
            {
                VisualStudioVersion.VS2012, new List<string>()
                                            {
                                                MyDocuments + Resources.VS2012Path1
                                        }}
            };
    }
}
