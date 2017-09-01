using System;
using System.Xml;

namespace Verse
{
	public class PatchOperationInsert : PatchOperationPathed
	{
		private enum Order
		{
			Append,
			Prepend
		}

		private XmlContainer value;

		private PatchOperationInsert.Order order = PatchOperationInsert.Order.Prepend;

		protected override bool ApplyWorker(XmlDocument xml)
		{
			XmlNode node = this.value.node;
			bool result = false;
			foreach (object current in xml.SelectNodes(this.xpath))
			{
				result = true;
				XmlNode xmlNode = current as XmlNode;
				XmlNode parentNode = xmlNode.ParentNode;
				if (this.order == PatchOperationInsert.Order.Append)
				{
					for (int i = 0; i < node.ChildNodes.Count; i++)
					{
						parentNode.InsertAfter(parentNode.OwnerDocument.ImportNode(node.ChildNodes[i], true), xmlNode);
					}
				}
				else if (this.order == PatchOperationInsert.Order.Prepend)
				{
					for (int j = node.ChildNodes.Count - 1; j >= 0; j--)
					{
						parentNode.InsertBefore(parentNode.OwnerDocument.ImportNode(node.ChildNodes[j], true), xmlNode);
					}
				}
			}
			return result;
		}
	}
}
