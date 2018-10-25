using System;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_PsychicSoothe : IncidentWorker_PsychicEmanation
	{
		protected override void DoConditionAndLetter(Map map, int duration, Gender gender, float points)
		{
			GameCondition_PsychicEmanation gameCondition_PsychicEmanation = (GameCondition_PsychicEmanation)GameConditionMaker.MakeCondition(GameConditionDefOf.PsychicSoothe, duration, 0);
			gameCondition_PsychicEmanation.gender = gender;
			map.gameConditionManager.RegisterCondition(gameCondition_PsychicEmanation);
			string text = "LetterIncidentPsychicSoothe".Translate(gender.ToString().Translate().ToLower());
			Find.LetterStack.ReceiveLetter("LetterLabelPsychicSoothe".Translate(), text, LetterDefOf.PositiveEvent, null);
		}
	}
}
