using RimWorld;
using System;
using System.Xml;

namespace Verse
{
	public class SkillGain
	{
		public SkillDef skill;

		public int xp;

		public SkillGain()
		{
		}

		public SkillGain(SkillDef skill, int xp)
		{
			this.skill = skill;
			this.xp = xp;
		}

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			if (xmlRoot.ChildNodes.Count != 1)
			{
				Log.Error("Misconfigured SkillGain: " + xmlRoot.OuterXml);
				return;
			}
			CrossRefLoader.RegisterObjectWantsCrossRef(this, "skill", xmlRoot.Name);
			this.xp = (int)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(int));
		}
	}
}
