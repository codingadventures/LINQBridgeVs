namespace BridgeVs.VsPackage.Helper.Installer.VsRestart
{
    internal class OpenedItem
    {
        public OpenedItem(string name, bool isSolution)
        {
            Name = name;
            IsSolution = isSolution;
        }

        public string Name { get; private set; }

        public bool IsSolution { get; set; }

        public bool IsNone()
        {
            return string.IsNullOrEmpty(Name) && !IsSolution;
        }
    }
}
