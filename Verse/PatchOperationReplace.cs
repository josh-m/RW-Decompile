using System;
using System.Xml;

namespace Verse
{
	public class PatchOperationReplace : PatchOperationPathed
	{
		private XmlContainer value;

		protected override bool ApplyWorker(XmlDocument xml)
		{
			XmlNode node = this.value.node;
			bool result = false;
			foreach (object current in xml.SelectNodes(this.xpath))
			{
				result = true;
				XmlNode xmlNode = current as XmlNode;
				XmlNode parentNode = xmlNode.ParentNode;
				for (int i = 0; i < node.ChildNodes.Count; i++)
				{
					parentNode.InsertBefore(parentNode.OwnerDocument.ImportNode(node.ChildNodes[i], true), xmlNode);
				}
				parentNode.RemoveChild(xmlNode);
			}
			return result;
		}
	}
}
