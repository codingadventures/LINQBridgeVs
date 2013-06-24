using System;
using Microsoft.Build.Framework;
using SInject;

namespace LINQBridge.BuildTasks
{
    public class SInjectionBuildTask : ITask
    {
        [Required]
        public string Assembly { get; set; }

        public bool Execute()
        {
            try
            {
                var sInjection = new SInjection(Assembly, PatchMode.Debug);
                sInjection.Patch(SerializationTypes.BinarySerialization);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }


        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }
    }
}