using System;
using System.Xml;
using Verse;

namespace RimWorld
{
	public class StatModifier
	{
		public StatDef stat;

		public float value;

		public string ToStringAsOffset
		{
			get
			{
				return this.stat.ValueToString(this.value, ToStringNumberSense.Offset);
			}
		}

		public string ToStringAsFactor
		{
			get
			{
				return this.stat.ValueToString(this.value, ToStringNumberSense.Factor);
			}
		}

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			CrossRefLoader.RegisterObjectWantsCrossRef(this, "stat", xmlRoot.Name);
			this.value = (float)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float));
		}
	}
}
