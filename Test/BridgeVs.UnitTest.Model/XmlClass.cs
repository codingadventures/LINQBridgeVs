using System.Xml.Linq;

namespace BridgeVs.UnitTest.Model
{
    public class XmlClass
    {
        public int MyProperty => 12;

        public XElement Contacts
        {
            get;
        }

        public XmlClass()
        {
            Contacts =
                new XElement("Contacts",
                    new XElement("Contact",
                        new XElement("Name", "Patrick Hines"),
                        new XElement("Phone", "206-555-0144"),
                        new XElement("Address",
                            new XElement("Street1", "123 Main St"),
                            new XElement("City", "Mercer Island"),
                            new XElement("State", "WA"),
                            new XElement("Postal", "68042")
                        )
                    )
                );
        }
    }
}
