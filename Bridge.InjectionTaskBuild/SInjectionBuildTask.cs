using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using SInject;

namespace Bridge.InjectionBuildTask
{
    public class SInjectionBuildTask : ITask
    {
        public bool Execute()
        {
            var sInjection = new SInjection(Assembly, PatchMode.Debug);
            sInjection.Patch(SerializationTypes.BinarySerialization);

            return true;
        }

        [Required]
        public string Assembly { get; set; }

        [Required]
        public string References { get; set; }
        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }
    }
}
