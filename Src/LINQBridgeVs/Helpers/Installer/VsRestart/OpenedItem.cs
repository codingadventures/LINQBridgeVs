using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstTimeConfigurator.VsRestart
{
    internal class OpenedItem
    {
        public static readonly OpenedItem None = new OpenedItem(null, false);

        public OpenedItem(string name, bool isSolution)
        {
            Name = name;
            IsSolution = isSolution;
        }

        public string Name { get; private set; }

        public bool IsSolution { get; set; }
    }

}
