using System;
using System.Xml;

namespace Verse
{
	public class PatchOperationAdd : PatchOperationPathed
	{
		private enum Order
		{
			Append,
			Prepend
		}

		private XmlContainer value;

		private PatchOperationAdd.Order order;

		protected override bool ApplyWorker(XmlDocument xml)
		{
			XmlNode node = this.value.node;
			bool result = false;
			foreach (object current in xml.SelectNodes(this.xpath))
			{
				result = true;
				XmlNode xmlNode = current as XmlNode;
				if (this.order == PatchOperationAdd.Order.Append)
				{
					for (int i = 0; i < node.ChildNodes.Count; i++)
					{
						xmlNode.AppendChild(xmlNode.OwnerDocument.ImportNode(node.ChildNodes[i], true));
					}
				}
				else if (this.order == PatchOperationAdd.Order.Prepend)
				{
					for (int j = node.ChildNodes.Count - 1; j >= 0; j--)
					{
						xmlNode.PrependChild(xmlNode.OwnerDocument.ImportNode(node.ChildNodes[j], true));
					}
				}
			}
			return result;
		}
	}
}
