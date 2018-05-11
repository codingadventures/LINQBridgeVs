using System;

namespace BridgeVs.VsPackage.Helper
{
    [Flags]
    public enum CommandStates : int
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
