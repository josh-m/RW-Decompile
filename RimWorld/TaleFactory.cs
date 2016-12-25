using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class TaleFactory
	{
		public static Tale MakeRawTale(TaleDef def, params object[] args)
		{
			Tale tale = (Tale)Activator.CreateInstance(def.taleClass, args);
			tale.def = def;
			tale.id = Find.World.uniqueIDsManager.GetNextTaleID();
			tale.date = Find.TickManager.TicksAbs;
			return tale;
		}

		public static Tale MakeRandomTestTale(TaleDef def = null)
		{
			if (def == null)
			{
				def = (from d in DefDatabase<TaleDef>.AllDefs
				where d.usableForArt
				select d).RandomElement<TaleDef>();
			}
			Tale tale = TaleFactory.MakeRawTale(def, new object[0]);
			tale.GenerateTestData();
			return tale;
		}
	}
}
