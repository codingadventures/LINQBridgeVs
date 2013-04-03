// Guids.cs
// MUST match guids.h
using System;

namespace GiovanniCampo.Bridge_VSExtension
{
    static class GuidList
    {
        public const string guidBridge_VSExtensionPkgString = "fa136bfd-0b1d-4721-9159-1dbefcb5c4fc";
        public const string guidBridge_VSExtensionCmdSetString = "d30046ec-11cc-48e5-bcfa-ca67686f0a45";

        public static readonly Guid guidBridge_VSExtensionCmdSet = new Guid(guidBridge_VSExtensionCmdSetString);
    };
}