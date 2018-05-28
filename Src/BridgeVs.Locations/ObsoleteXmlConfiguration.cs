using BridgeVs.Shared.Common;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace BridgeVs.Locations
{
    public class ObsoleteXmlConfiguration
    {
        public static readonly bool IsFramework45Installed = Directory.Exists(CommonFolderPaths.DotNet45FrameworkPath);

        #region [ Obsolete ]
        private static XDocument _microsoftCommonTargetDocument;
        private static XDocument MicrosoftCommonTargetDocument
        {
            get
            {
                _microsoftCommonTargetDocument = _microsoftCommonTargetDocument ?? XDocument.Load(CommonFolderPaths.MicrosoftCommonTargetFileNamePath);
                return _microsoftCommonTargetDocument;
            }

        }

        private static XDocument _microsoftCommonTargetX64Document;
        private static XDocument MicrosoftCommonTargetX64Document
        {
            get
            {
                _microsoftCommonTargetX64Document = _microsoftCommonTargetX64Document ?? XDocument.Load(CommonFolderPaths.MicrosoftCommonTargetX64FileNamePath);
                return _microsoftCommonTargetX64Document;
            }

        }

        private static XDocument _microsoftCommonTarget45Document;
        private static XDocument MicrosoftCommonTarget45Document
        {
            get
            {
                _microsoftCommonTarget45Document = _microsoftCommonTarget45Document ?? XDocument.Load(CommonFolderPaths.MicrosoftCommonTarget45FileNamePath);
                return _microsoftCommonTarget45Document;
            }
        }
        #endregion

        #region [ Obsolete Methods ]

        [Obsolete("Keep them for Backward compatibility. Microsoft.Common.targets should not be modifie anymore")]
        private static void RemoveBridgeBuildTargetFromMicrosoftCommon(XDocument document, string location)
        {
            XElement linqBridgeTargetImportNode = GetTargetImportNode(document);

            if (linqBridgeTargetImportNode == null) return;

            linqBridgeTargetImportNode.Remove();

            document.Save(location);
        }

        [Obsolete("Keep them for Backward compatibility. Microsoft.Common.targets should not be modifie anymore")]
        private static XElement GetTargetImportNode(XDocument document)
        {
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("aw", "http://schemas.microsoft.com/developer/msbuild/2003");

            IEnumerable importProjectNode =
                (IEnumerable)
                document.XPathEvaluate("/aw:Project/aw:Import[@Project='BridgeBuildTask.targets']",
                    namespaceManager);


            XElement linqBridgeTargetImportNode = importProjectNode.Cast<XElement>().FirstOrDefault();

            return linqBridgeTargetImportNode;
        }

        #endregion
        /// <summary>
        /// This old method is used to remove the old msbuild configuration. In that configuration I used to inject Microsoft.Common.targets
        /// to trigger my build tasks for c# and/or vb on enabled projects
        /// </summary>
        public static void RemoveOldTargets()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            RemoveBridgeBuildTargetFromMicrosoftCommon(MicrosoftCommonTargetDocument, CommonFolderPaths.MicrosoftCommonTargetFileNamePath);
#pragma warning restore CS0618 // Type or member is obsolete

            if (Environment.Is64BitOperatingSystem)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                RemoveBridgeBuildTargetFromMicrosoftCommon(MicrosoftCommonTargetX64Document, CommonFolderPaths.MicrosoftCommonTargetX64FileNamePath);
#pragma warning restore CS0618 // Type or member is obsolete

            }
            if (IsFramework45Installed)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                RemoveBridgeBuildTargetFromMicrosoftCommon(MicrosoftCommonTarget45Document, CommonFolderPaths.MicrosoftCommonTarget45FileNamePath);
#pragma warning restore CS0618 // Type or member is obsolete

            }
        }
    }
}
