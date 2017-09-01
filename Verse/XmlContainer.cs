using System;
using System.Xml;

namespace Verse
{
	public class XmlContainer
	{
		public XmlNode node;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			this.node = xmlRoot;
		}
	}
}
