using System;
using System.Xml;
using Verse;

namespace RimWorld
{
	public class IncidentCategoryEntry
	{
		public IncidentCategoryDef category;

		public float weight;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "category", xmlRoot.Name);
			this.weight = (float)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float));
		}
	}
}
