// Guids.cs
// MUST match guids.h

using System;

namespace Bridge.VSExtension
{
    static class GuidList
    {
        public const string GuidBridgeVsExtensionPkgString = "fa136bfd-0b1d-4721-9159-1dbefcb5c4fc";
        private const string GuidBridgeVsExtensionCmdSetString = "d30046ec-11cc-48e5-bcfa-ca67686f0a45";

        public static readonly Guid GuidBridgeVsExtensionCmdSet = new Guid(GuidBridgeVsExtensionCmdSetString);
    };
}