using System;
using System.Xml;

namespace Verse
{
	public class PatchOperationRemove : PatchOperationPathed
	{
		protected override bool ApplyWorker(XmlDocument xml)
		{
			bool result = false;
			foreach (object current in xml.SelectNodes(this.xpath))
			{
				result = true;
				XmlNode xmlNode = current as XmlNode;
				xmlNode.ParentNode.RemoveChild(xmlNode);
			}
			return result;
		}
	}
}
