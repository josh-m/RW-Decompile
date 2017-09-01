using System;
using System.Xml;

namespace Verse
{
	public class PatchOperationAddModExtension : PatchOperationPathed
	{
		private XmlContainer value;

		protected override bool ApplyWorker(XmlDocument xml)
		{
			XmlNode node = this.value.node;
			bool result = false;
			foreach (object current in xml.SelectNodes(this.xpath))
			{
				XmlNode xmlNode = current as XmlNode;
				XmlNode xmlNode2 = xmlNode["modExtensions"];
				if (xmlNode2 == null)
				{
					xmlNode2 = xmlNode.OwnerDocument.CreateElement("modExtensions");
					xmlNode.AppendChild(xmlNode2);
				}
				for (int i = 0; i < node.ChildNodes.Count; i++)
				{
					xmlNode2.AppendChild(xmlNode.OwnerDocument.ImportNode(node.ChildNodes[i], true));
				}
				result = true;
			}
			return result;
		}
	}
}
