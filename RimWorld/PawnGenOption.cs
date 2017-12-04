using System;
using System.Xml;
using Verse;

namespace RimWorld
{
	public class PawnGenOption
	{
		public PawnKindDef kind;

		public float selectionWeight;

		public float Cost
		{
			get
			{
				return this.kind.combatPower;
			}
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"(",
				(this.kind == null) ? "null" : this.kind.ToString(),
				" w=",
				this.selectionWeight.ToString("F2"),
				" c=",
				(this.kind == null) ? "null" : this.Cost.ToString("F2"),
				")"
			});
		}

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "kind", xmlRoot.Name);
			this.selectionWeight = (float)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float));
		}
	}
}
