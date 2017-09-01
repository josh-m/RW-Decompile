using System;
using Verse;

namespace RimWorld.Planet
{
	public class WorldSettings : IExposable
	{
		public MedicalCareCategory defaultCareForColonyHumanlike = MedicalCareCategory.Best;

		public MedicalCareCategory defaultCareForColonyAnimal = MedicalCareCategory.HerbalOrWorse;

		public MedicalCareCategory defaultCareForColonyPrisoner = MedicalCareCategory.HerbalOrWorse;

		public MedicalCareCategory defaultCareForNeutralFaction = MedicalCareCategory.HerbalOrWorse;

		public MedicalCareCategory defaultCareForNeutralAnimal = MedicalCareCategory.HerbalOrWorse;

		public MedicalCareCategory defaultCareForHostileFaction = MedicalCareCategory.HerbalOrWorse;

		public void ExposeData()
		{
			Scribe_Values.Look<MedicalCareCategory>(ref this.defaultCareForColonyHumanlike, "defaultCareForHumanlikeColonists", MedicalCareCategory.NoCare, false);
			Scribe_Values.Look<MedicalCareCategory>(ref this.defaultCareForColonyAnimal, "defaultCareForAnimalColonists", MedicalCareCategory.NoCare, false);
			Scribe_Values.Look<MedicalCareCategory>(ref this.defaultCareForColonyPrisoner, "defaultCareForHumanlikeColonistPrisoners", MedicalCareCategory.NoCare, false);
			Scribe_Values.Look<MedicalCareCategory>(ref this.defaultCareForNeutralFaction, "defaultCareForHumanlikeNeutrals", MedicalCareCategory.NoCare, false);
			Scribe_Values.Look<MedicalCareCategory>(ref this.defaultCareForNeutralAnimal, "defaultCareForAnimalNeutrals", MedicalCareCategory.NoCare, false);
			Scribe_Values.Look<MedicalCareCategory>(ref this.defaultCareForHostileFaction, "defaultCareForHumanlikeEnemies", MedicalCareCategory.NoCare, false);
		}
	}
}
