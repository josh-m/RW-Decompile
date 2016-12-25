using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class TaleData_Surroundings : TaleData
	{
		public IntVec2 worldCoords;

		public float temperature;

		public float snowDepth;

		public WeatherDef weather;

		public RoomRoleDef roomRole;

		public float roomImpressiveness;

		public float roomBeauty;

		public float roomCleanliness;

		public bool Outdoors
		{
			get
			{
				return this.weather != null;
			}
		}

		public override void ExposeData()
		{
			Scribe_Values.LookValue<IntVec2>(ref this.worldCoords, "worldCoords", default(IntVec2), false);
			Scribe_Values.LookValue<float>(ref this.temperature, "temperature", 0f, false);
			Scribe_Values.LookValue<float>(ref this.snowDepth, "snowDepth", 0f, false);
			Scribe_Defs.LookDef<WeatherDef>(ref this.weather, "weather");
			Scribe_Defs.LookDef<RoomRoleDef>(ref this.roomRole, "roomRole");
			Scribe_Values.LookValue<float>(ref this.roomImpressiveness, "roomImpressiveness", 0f, false);
			Scribe_Values.LookValue<float>(ref this.roomBeauty, "roomBeauty", 0f, false);
			Scribe_Values.LookValue<float>(ref this.roomCleanliness, "roomCleanliness", 0f, false);
		}

		[DebuggerHidden]
		public override IEnumerable<Rule> GetRules()
		{
			yield return new Rule_String("biome", Find.World.grid.Get(this.worldCoords).biome.label);
			if (this.roomRole != null && this.roomRole != RoomRoleDefOf.None)
			{
				yield return new Rule_String("room_role", this.roomRole.label);
				yield return new Rule_String("room_roleDefinite", Find.ActiveLanguageWorker.WithDefiniteArticle(this.roomRole.label));
				yield return new Rule_String("room_roleIndefinite", Find.ActiveLanguageWorker.WithIndefiniteArticle(this.roomRole.label));
				RoomStatScoreStage impressiveness = RoomStatDefOf.Impressiveness.GetScoreStage(this.roomImpressiveness);
				RoomStatScoreStage beauty = RoomStatDefOf.Beauty.GetScoreStage(this.roomBeauty);
				RoomStatScoreStage cleanliness = RoomStatDefOf.Cleanliness.GetScoreStage(this.roomCleanliness);
				yield return new Rule_String("room_impressiveness", impressiveness.label);
				yield return new Rule_String("room_impressivenessIndefinite", Find.ActiveLanguageWorker.WithIndefiniteArticle(impressiveness.label));
				yield return new Rule_String("room_beauty", beauty.label);
				yield return new Rule_String("room_beautyIndefinite", Find.ActiveLanguageWorker.WithIndefiniteArticle(beauty.label));
				yield return new Rule_String("room_cleanliness", cleanliness.label);
				yield return new Rule_String("room_cleanlinessIndefinite", Find.ActiveLanguageWorker.WithIndefiniteArticle(cleanliness.label));
			}
		}

		public static TaleData_Surroundings GenerateFrom(IntVec3 c)
		{
			TaleData_Surroundings taleData_Surroundings = new TaleData_Surroundings();
			taleData_Surroundings.worldCoords = Find.Map.WorldCoords;
			Room roomOrAdjacent = c.GetRoomOrAdjacent();
			if (roomOrAdjacent != null)
			{
				if (roomOrAdjacent.PsychologicallyOutdoors)
				{
					taleData_Surroundings.weather = Find.WeatherManager.curWeather;
				}
				taleData_Surroundings.roomRole = roomOrAdjacent.Role;
				taleData_Surroundings.roomImpressiveness = roomOrAdjacent.GetStat(RoomStatDefOf.Impressiveness);
				taleData_Surroundings.roomBeauty = roomOrAdjacent.GetStat(RoomStatDefOf.Beauty);
				taleData_Surroundings.roomCleanliness = roomOrAdjacent.GetStat(RoomStatDefOf.Cleanliness);
			}
			if (!GenTemperature.TryGetTemperatureForCell(c, out taleData_Surroundings.temperature))
			{
				taleData_Surroundings.temperature = 21f;
			}
			taleData_Surroundings.snowDepth = Find.SnowGrid.GetDepth(c);
			return taleData_Surroundings;
		}

		public static TaleData_Surroundings GenerateRandom()
		{
			return TaleData_Surroundings.GenerateFrom(CellFinder.RandomCell());
		}
	}
}
