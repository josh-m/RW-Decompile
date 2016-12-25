using System;

namespace Verse
{
	public static class HediffMaker
	{
		public static Hediff MakeHediff(HediffDef def, Pawn pawn, BodyPartRecord partRecord = null)
		{
			if (pawn == null)
			{
				Log.Error("Cannot make hediff " + def + " for null pawn.");
				return null;
			}
			Hediff hediff = (Hediff)Activator.CreateInstance(def.hediffClass);
			hediff.def = def;
			hediff.pawn = pawn;
			hediff.Part = partRecord;
			hediff.PostMake();
			return hediff;
		}
	}
}
