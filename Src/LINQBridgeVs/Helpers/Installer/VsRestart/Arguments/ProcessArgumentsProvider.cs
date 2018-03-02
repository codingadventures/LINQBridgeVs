using System.Management;

namespace MidnightDevelopers.VisualStudio.VsRestart.Arguments
{
    internal static class ProcessArgumentsProvider
    {
        public static ArgumentTokenCollection Lookup(int processId)
        {
            string wmiQuery = string.Format("SELECT CommandLine FROM Win32_Process WHERE ProcessId = {0}", processId);
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery);
            ManagementObjectCollection retObjectCollection = searcher.Get();

            try
            {
                string commandLine = string.Empty;

                if (retObjectCollection.Count == 0 || retObjectCollection.Count > 1)
                {
                    return ArgumentTokenCollection.Empty;
                }

                using (var enumerator = retObjectCollection.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        commandLine = enumerator.Current["CommandLine"].ToString();
                    }
                }

                var parser = new ArgumentParser(commandLine);
                return parser.GetArguments();
            }
            finally
            {
                if (searcher != null)
                {
                    searcher.Dispose();
                }

                if (retObjectCollection != null)
                {
                    retObjectCollection.Dispose();
                }
            }

            //foreach (ManagementObject retObject in retObjectCollection)
            //{
            //    return retObject["CommandLine"].ToString();
            //}
        }
    }
}