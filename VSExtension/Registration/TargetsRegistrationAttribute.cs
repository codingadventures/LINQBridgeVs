using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace LINQBridge.VSExtension.Registration
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    class TargetsRegistrationAttribute : RegistrationAttribute
    {
        public override void Register(RegistrationContext context)
        {
            
        }

        public override void Unregister(RegistrationContext context)
        {
            
        }
    }
}
