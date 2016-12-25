using System;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_PsychicDrone : IncidentWorker_PsychicEmanation
	{
		protected override void DoConditionAndLetter(Map map, int duration, Gender gender)
		{
			MapCondition_PsychicEmanation mapCondition_PsychicEmanation = (MapCondition_PsychicEmanation)MapConditionMaker.MakeCondition(MapConditionDefOf.PsychicDrone, duration, 0);
			mapCondition_PsychicEmanation.gender = gender;
			map.mapConditionManager.RegisterCondition(mapCondition_PsychicEmanation);
			string text = "LetterIncidentPsychicDrone".Translate(new object[]
			{
				gender.ToString().Translate().ToLower()
			});
			Find.LetterStack.ReceiveLetter("LetterLabelPsychicDrone".Translate(), text, LetterType.BadNonUrgent, null);
		}
	}
}
