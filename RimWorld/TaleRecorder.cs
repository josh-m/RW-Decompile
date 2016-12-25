using System;
using Verse;

namespace RimWorld
{
	public static class TaleRecorder
	{
		public static void RecordTale(TaleDef def, params object[] args)
		{
			if (Rand.Value < def.ignoreChance)
			{
				return;
			}
			if (def.colonistOnly)
			{
				bool flag = false;
				bool flag2 = false;
				for (int i = 0; i < args.Length; i++)
				{
					Pawn pawn = args[i] as Pawn;
					if (pawn != null)
					{
						flag = true;
						if (pawn.Faction == Faction.OfPlayer)
						{
							flag2 = true;
						}
					}
				}
				if (flag && !flag2)
				{
					return;
				}
			}
			Find.TaleManager.Add(TaleFactory.MakeRawTale(def, args));
			for (int j = 0; j < args.Length; j++)
			{
				Pawn pawn2 = args[j] as Pawn;
				if (pawn2 != null && !pawn2.Dead && pawn2.needs.mood != null)
				{
					pawn2.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
				}
			}
		}
	}
}
