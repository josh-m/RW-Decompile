using System;
using System.Xml;

namespace Verse
{
	public sealed class ThingCountClass
	{
		public ThingDef thingDef;

		public int count;

		public ThingCountClass()
		{
		}

		public ThingCountClass(ThingDef thingDef, int count)
		{
			this.thingDef = thingDef;
			this.count = count;
		}

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			if (xmlRoot.ChildNodes.Count != 1)
			{
				Log.Error("Misconfigured ThingCount: " + xmlRoot.OuterXml);
				return;
			}
			CrossRefLoader.RegisterObjectWantsCrossRef(this, "thingDef", xmlRoot.Name);
			this.count = (int)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(int));
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"(",
				this.count,
				"x ",
				(this.thingDef == null) ? "null" : this.thingDef.defName,
				")"
			});
		}

		public override int GetHashCode()
		{
			return (int)this.thingDef.shortHash + this.count << 16;
		}
	}
}
