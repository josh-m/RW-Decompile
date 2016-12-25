using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;

namespace Verse
{
	public static class StartingPawnUtility
	{
		private static List<Pawn> StartingPawns
		{
			get
			{
				return Find.GameInitData.startingPawns;
			}
		}

		public static void ClearAllStartingPawns()
		{
			for (int i = StartingPawnUtility.StartingPawns.Count - 1; i >= 0; i--)
			{
				StartingPawnUtility.StartingPawns[i].relations.ClearAllRelations();
				if (Find.World != null)
				{
					PawnUtility.DestroyStartingColonistFamily(StartingPawnUtility.StartingPawns[i]);
					Find.WorldPawns.PassToWorld(StartingPawnUtility.StartingPawns[i], PawnDiscardDecideMode.Discard);
				}
				StartingPawnUtility.StartingPawns.RemoveAt(i);
			}
		}

		public static Pawn RandomizeInPlace(Pawn p)
		{
			int index = StartingPawnUtility.StartingPawns.IndexOf(p);
			Pawn pawn = StartingPawnUtility.RegenerateStartingPawnInPlace(index);
			if (pawn.story.WorkTagIsDisabled(WorkTags.ManualDumb) || pawn.story.WorkTagIsDisabled(WorkTags.Violent))
			{
				pawn = StartingPawnUtility.RegenerateStartingPawnInPlace(index);
			}
			return pawn;
		}

		private static Pawn RegenerateStartingPawnInPlace(int index)
		{
			Pawn pawn = StartingPawnUtility.StartingPawns[index];
			PawnUtility.TryDestroyStartingColonistFamily(pawn);
			pawn.relations.ClearAllRelations();
			Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
			StartingPawnUtility.StartingPawns[index] = null;
			for (int i = 0; i < Find.GameInitData.startingPawns.Count; i++)
			{
				if (StartingPawnUtility.StartingPawns[i] != null)
				{
					PawnUtility.TryDestroyStartingColonistFamily(Find.GameInitData.startingPawns[i]);
				}
			}
			Pawn pawn2 = StartingPawnUtility.NewGeneratedStartingPawn();
			Find.GameInitData.startingPawns[index] = pawn2;
			return pawn2;
		}

		public static Pawn NewGeneratedStartingPawn()
		{
			PawnGenerationRequest request = new PawnGenerationRequest(Faction.OfPlayer.def.basicMemberKind, Faction.OfPlayer, PawnGenerationContext.PlayerStarter, null, true, false, false, false, true, false, 26f, false, true, true, null, null, null, null, null, null);
			Pawn pawn = null;
			try
			{
				pawn = PawnGenerator.GeneratePawn(request);
			}
			catch (Exception arg)
			{
				Log.Error("There was an exception thrown by the PawnGenerator during generating a starting pawn. Trying one more time...\nException: " + arg);
				pawn = PawnGenerator.GeneratePawn(request);
			}
			pawn.relations.everSeenByPlayer = true;
			PawnComponentsUtility.AddComponentsForSpawn(pawn);
			return pawn;
		}

		public static bool WorkTypeRequirementsSatisfied()
		{
			if (StartingPawnUtility.StartingPawns.Count == 0)
			{
				return false;
			}
			List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				WorkTypeDef workTypeDef = allDefsListForReading[i];
				if (workTypeDef.requireCapableColonist)
				{
					bool flag = false;
					for (int j = 0; j < StartingPawnUtility.StartingPawns.Count; j++)
					{
						if (!StartingPawnUtility.StartingPawns[j].story.WorkTypeIsDisabled(workTypeDef))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						return false;
					}
				}
			}
			if (TutorSystem.TutorialMode)
			{
				if (StartingPawnUtility.StartingPawns.Any((Pawn p) => p.story.WorkTagIsDisabled(WorkTags.Violent)))
				{
					return false;
				}
			}
			return true;
		}
	}
}
