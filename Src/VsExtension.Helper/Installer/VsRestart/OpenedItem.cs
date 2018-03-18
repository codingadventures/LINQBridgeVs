namespace BridgeVs.Helper.Installer.VsRestart
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
