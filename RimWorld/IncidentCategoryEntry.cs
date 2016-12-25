using System;
using System.Xml;
using Verse;

namespace RimWorld
{
	public class IncidentCategoryEntry
	{
		public IncidentCategory category;

		public float weight;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			this.category = (IncidentCategory)((byte)ParseHelper.FromString(xmlRoot.Name, typeof(IncidentCategory)));
			this.weight = (float)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float));
		}
	}
}
