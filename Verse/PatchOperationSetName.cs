using System;
using System.Xml;

namespace Verse
{
	public class PatchOperationSetName : PatchOperationPathed
	{
		protected string name;

		protected override bool ApplyWorker(XmlDocument xml)
		{
			bool result = false;
			foreach (object current in xml.SelectNodes(this.xpath))
			{
				XmlNode xmlNode = current as XmlNode;
				XmlNode xmlNode2 = xmlNode.OwnerDocument.CreateElement(this.name);
				xmlNode2.InnerXml = xmlNode.InnerXml;
				xmlNode.ParentNode.InsertBefore(xmlNode2, xmlNode);
				xmlNode.ParentNode.RemoveChild(xmlNode);
			}
			return result;
		}
	}
}
