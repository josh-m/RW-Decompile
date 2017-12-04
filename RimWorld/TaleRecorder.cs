using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class TaleRecorder
	{
		public static void RecordTale(TaleDef def, params object[] args)
		{
			bool flag = Rand.Value < def.ignoreChance;
			if (Rand.Value < def.ignoreChance && !DebugViewSettings.logTaleRecording)
			{
				return;
			}
			if (def.colonistOnly)
			{
				bool flag2 = false;
				bool flag3 = false;
				for (int i = 0; i < args.Length; i++)
				{
					Pawn pawn = args[i] as Pawn;
					if (pawn != null)
					{
						flag2 = true;
						if (pawn.Faction == Faction.OfPlayer)
						{
							flag3 = true;
						}
					}
				}
				if (flag2 && !flag3)
				{
					return;
				}
			}
			Tale tale = TaleFactory.MakeRawTale(def, args);
			if (tale == null)
			{
				return;
			}
			if (DebugViewSettings.logTaleRecording)
			{
				string arg_107_0 = "Tale {0} from {1}, targets {2}:\n{3}";
				object[] expr_A5 = new object[4];
				expr_A5[0] = ((!flag) ? "recorded" : "ignored");
				expr_A5[1] = def;
				expr_A5[2] = GenText.ToCommaList(args.Select(new Func<object, string>(Gen.ToStringSafe<object>)), true);
				expr_A5[3] = TaleTextGenerator.GenerateTextFromTale(TextGenerationPurpose.ArtDescription, tale, Rand.Int, RulePackDefOf.ArtDescription_Sculpture.RulesPlusIncludes);
				Log.Message(string.Format(arg_107_0, expr_A5));
			}
			if (flag)
			{
				return;
			}
			Find.TaleManager.Add(tale);
			for (int j = 0; j < args.Length; j++)
			{
				Pawn pawn2 = args[j] as Pawn;
				if (pawn2 != null)
				{
					if (!pawn2.Dead && pawn2.needs.mood != null)
					{
						pawn2.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
					}
					pawn2.records.AccumulateStoryEvent(StoryEventDefOf.TaleCreated);
				}
			}
		}
	}
}
