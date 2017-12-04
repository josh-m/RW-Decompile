using System;
using System.Xml;
using Verse;

namespace RimWorld
{
	public class StatModifier
	{
		public StatDef stat;

		public float value;

		public string ValueToStringAsOffset
		{
			get
			{
				return this.stat.Worker.ValueToString(this.value, false, ToStringNumberSense.Offset);
			}
		}

		public string ToStringAsFactor
		{
			get
			{
				return this.stat.Worker.ValueToString(this.value, false, ToStringNumberSense.Factor);
			}
		}

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "stat", xmlRoot.Name);
			this.value = (float)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float));
		}

		public override string ToString()
		{
			if (this.stat == null)
			{
				return "(null stat)";
			}
			return this.stat.defName + "-" + this.value.ToString();
		}
	}
}
