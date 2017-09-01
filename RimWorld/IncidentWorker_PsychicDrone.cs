using System;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_PsychicDrone : IncidentWorker_PsychicEmanation
	{
		protected override void DoConditionAndLetter(Map map, int duration, Gender gender)
		{
			GameCondition_PsychicEmanation gameCondition_PsychicEmanation = (GameCondition_PsychicEmanation)GameConditionMaker.MakeCondition(GameConditionDefOf.PsychicDrone, duration, 0);
			gameCondition_PsychicEmanation.gender = gender;
			map.gameConditionManager.RegisterCondition(gameCondition_PsychicEmanation);
			string text = "LetterIncidentPsychicDrone".Translate(new object[]
			{
				gender.ToString().Translate().ToLower()
			});
			Find.LetterStack.ReceiveLetter("LetterLabelPsychicDrone".Translate(), text, LetterDefOf.BadNonUrgent, null);
		}
	}
}
