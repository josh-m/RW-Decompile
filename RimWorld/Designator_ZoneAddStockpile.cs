using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class Designator_ZoneAddStockpile : Designator_ZoneAdd
	{
		protected StorageSettingsPreset preset;

		protected override string NewZoneLabel
		{
			get
			{
				return this.preset.PresetName();
			}
		}

		public Designator_ZoneAddStockpile()
		{
			this.zoneTypeToPlace = typeof(Zone_Stockpile);
		}

		protected override Zone MakeNewZone()
		{
			return new Zone_Stockpile(this.preset, Find.VisibleMap.zoneManager);
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			AcceptanceReport result = base.CanDesignateCell(c);
			if (!result.Accepted)
			{
				return result;
			}
			TerrainDef terrain = c.GetTerrain(base.Map);
			if (terrain.passability == Traversability.Impassable)
			{
				return false;
			}
			List<Thing> list = base.Map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				if (!list[i].def.CanOverlapZones)
				{
					return false;
				}
			}
			return true;
		}

		protected override void FinalizeDesignationSucceeded()
		{
			base.FinalizeDesignationSucceeded();
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Stockpiles, KnowledgeAmount.Total);
		}
	}
}
