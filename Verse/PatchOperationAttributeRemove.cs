using System;
using System.Xml;

namespace Verse
{
	public class PatchOperationAttributeRemove : PatchOperationAttribute
	{
		protected override bool ApplyWorker(XmlDocument xml)
		{
			bool result = false;
			foreach (object current in xml.SelectNodes(this.xpath))
			{
				XmlNode xmlNode = current as XmlNode;
				if (xmlNode.Attributes[this.attribute] != null)
				{
					xmlNode.Attributes.Remove(xmlNode.Attributes[this.attribute]);
					result = true;
				}
			}
			return result;
		}
	}
}
