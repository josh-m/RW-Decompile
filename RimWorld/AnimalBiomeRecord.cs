using System;
using System.Xml;
using Verse;

namespace RimWorld
{
	public class AnimalBiomeRecord
	{
		public BiomeDef biome;

		public float commonality;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			CrossRefLoader.RegisterObjectWantsCrossRef(this, "biome", xmlRoot.Name);
			this.commonality = (float)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float));
		}
	}
}
