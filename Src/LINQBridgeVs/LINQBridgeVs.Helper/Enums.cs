using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LINQBridgeVs.Helper
{
    [Flags]
    public enum CommandStates
    {
        None = 0,
        Visible = 0x01,
        Enabled = 0x02
    }

    public enum CommandAction
    {
        Enable,
        Disable
    }

}
