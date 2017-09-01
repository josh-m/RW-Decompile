using System;
using System.Xml;

namespace Verse
{
	public class ShaderParameter
	{
		public string name;

		public float value;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			if (xmlRoot.ChildNodes.Count != 1)
			{
				Log.Error("Misconfigured ThingCount: " + xmlRoot.OuterXml);
				return;
			}
			this.name = xmlRoot.Name;
			this.value = (float)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float));
		}
	}
}
