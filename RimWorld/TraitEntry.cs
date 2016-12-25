using System;
using System.Xml;
using Verse;

namespace RimWorld
{
	public class TraitEntry
	{
		public TraitDef def;

		public int degree;

		public TraitEntry()
		{
		}

		public TraitEntry(TraitDef def, int degree)
		{
			this.def = def;
			this.degree = degree;
		}

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			this.def = DefDatabase<TraitDef>.GetNamed(xmlRoot.Name, true);
			if (xmlRoot.HasChildNodes)
			{
				this.degree = (int)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(int));
			}
			else
			{
				this.degree = 0;
			}
		}
	}
}
