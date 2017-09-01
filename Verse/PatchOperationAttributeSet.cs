using System;
using System.Xml;

namespace Verse
{
	public class PatchOperationAttributeSet : PatchOperationAttribute
	{
		protected string value;

		protected override bool ApplyWorker(XmlDocument xml)
		{
			bool result = false;
			foreach (object current in xml.SelectNodes(this.xpath))
			{
				XmlNode xmlNode = current as XmlNode;
				if (xmlNode.Attributes[this.attribute] != null)
				{
					xmlNode.Attributes[this.attribute].Value = this.value;
				}
				else
				{
					XmlAttribute xmlAttribute = xmlNode.OwnerDocument.CreateAttribute(this.attribute);
					xmlAttribute.Value = this.value;
					xmlNode.Attributes.Append(xmlAttribute);
				}
				result = true;
			}
			return result;
		}
	}
}
