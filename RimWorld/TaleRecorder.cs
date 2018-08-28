using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class TaleRecorder
	{
		public static Tale RecordTale(TaleDef def, params object[] args)
		{
			bool flag = Rand.Value < def.ignoreChance;
			if (Rand.Value < def.ignoreChance && !DebugViewSettings.logTaleRecording)
			{
				return null;
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
					return null;
				}
			}
			Tale tale = TaleFactory.MakeRawTale(def, args);
			if (tale == null)
			{
				return null;
			}
			if (DebugViewSettings.logTaleRecording)
			{
				string arg_105_0 = "Tale {0} from {1}, targets {2}:\n{3}";
				object[] expr_A8 = new object[4];
				expr_A8[0] = ((!flag) ? "recorded" : "ignored");
				expr_A8[1] = def;
				expr_A8[2] = args.Select(new Func<object, string>(Gen.ToStringSafe<object>)).ToCommaList(false);
				expr_A8[3] = TaleTextGenerator.GenerateTextFromTale(TextGenerationPurpose.ArtDescription, tale, Rand.Int, RulePackDefOf.ArtDescription_Sculpture);
				Log.Message(string.Format(arg_105_0, expr_A8), false);
			}
			if (flag)
			{
				return null;
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
			return tale;
		}
	}
}
