using System;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_PsychicDrone : IncidentWorker_PsychicEmanation
	{
		private const float MaxPointsDroneLow = 800f;

		private const float MaxPointsDroneMedium = 2000f;

		protected override void DoConditionAndLetter(Map map, int duration, Gender gender, float points)
		{
			if (points < 0f)
			{
				points = StorytellerUtility.DefaultThreatPointsNow(map);
			}
			PsychicDroneLevel level;
			if (points < 800f)
			{
				level = PsychicDroneLevel.BadLow;
			}
			else if (points < 2000f)
			{
				level = PsychicDroneLevel.BadMedium;
			}
			else
			{
				level = PsychicDroneLevel.BadHigh;
			}
			GameCondition_PsychicEmanation gameCondition_PsychicEmanation = (GameCondition_PsychicEmanation)GameConditionMaker.MakeCondition(GameConditionDefOf.PsychicDrone, duration, 0);
			gameCondition_PsychicEmanation.gender = gender;
			gameCondition_PsychicEmanation.level = level;
			map.gameConditionManager.RegisterCondition(gameCondition_PsychicEmanation);
			string label = string.Concat(new string[]
			{
				"LetterLabelPsychicDrone".Translate(),
				" (",
				level.GetLabel(),
				", ",
				gender.GetLabel(false),
				")"
			});
			string text = "LetterIncidentPsychicDrone".Translate(gender.ToString().Translate().ToLower(), level.GetLabel());
			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NegativeEvent, null);
		}
	}
}
