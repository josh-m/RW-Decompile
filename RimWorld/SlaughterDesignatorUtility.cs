using System;
using Verse;

namespace RimWorld
{
	public static class SlaughterDesignatorUtility
	{
		public static void CheckWarnAboutBondedAnimal(Pawn designated)
		{
			if (!designated.RaceProps.IsFlesh)
			{
				return;
			}
			Pawn firstDirectRelationPawn = designated.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond, null);
			if (firstDirectRelationPawn != null)
			{
				Messages.Message("MessageSlaughteringBondedAnimal".Translate(new object[]
				{
					designated.LabelShort,
					firstDirectRelationPawn.LabelShort
				}), designated, MessageSound.Standard);
			}
		}
	}
}
