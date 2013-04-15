// Guids.cs
// MUST match guids.h
using System;

namespace Company.VSPackage1
{
    static class GuidList
    {
        public const string guidVSPackage1PkgString = "834d8308-970a-4741-be60-6da97d6d3f97";
        public const string guidVSPackage1CmdSetString = "f4730d00-d761-4521-b9e2-cf7b87785a39";

        public static readonly Guid guidVSPackage1CmdSet = new Guid(guidVSPackage1CmdSetString);
    };
}