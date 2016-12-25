using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class MineStrikeManager : IExposable
	{
		private const int RecentStrikeIgnoreRadius = 12;

		private List<StrikeRecord> strikeRecords = new List<StrikeRecord>();

		private static readonly int RadialVisibleCells = GenRadial.NumCellsInRadius(5.9f);

		public void ExposeData()
		{
			Scribe_Collections.LookList<StrikeRecord>(ref this.strikeRecords, "strikeRecords", LookMode.Deep, new object[0]);
		}

		public void CheckStruckOre(IntVec3 justMinedPos, ThingDef justMinedDef, Thing miner)
		{
			if (miner.Faction != Faction.OfPlayer)
			{
				return;
			}
			for (int i = 0; i < 4; i++)
			{
				IntVec3 intVec = justMinedPos + GenAdj.CardinalDirections[i];
				if (intVec.InBounds(miner.Map))
				{
					Building edifice = intVec.GetEdifice(miner.Map);
					if (edifice != null && edifice.def != justMinedDef && this.MineableIsWorthLetter(edifice.def) && !this.AlreadyVisibleNearby(intVec, miner.Map, edifice.def) && !this.RecentlyStruck(intVec, edifice.def))
					{
						StrikeRecord item = default(StrikeRecord);
						item.cell = intVec;
						item.def = edifice.def;
						item.ticksGame = Find.TickManager.TicksGame;
						this.strikeRecords.Add(item);
						Messages.Message("StruckMineable".Translate(new object[]
						{
							edifice.def.label
						}), edifice, MessageSound.Benefit);
						TaleRecorder.RecordTale(TaleDefOf.StruckMineable, new object[]
						{
							miner,
							edifice
						});
					}
				}
			}
		}

		public bool AlreadyVisibleNearby(IntVec3 center, Map map, ThingDef mineableDef)
		{
			CellRect cellRect = CellRect.CenteredOn(center, 1);
			for (int i = 1; i < MineStrikeManager.RadialVisibleCells; i++)
			{
				IntVec3 c = center + GenRadial.RadialPattern[i];
				if (c.InBounds(map) && !c.Fogged(map) && !cellRect.Contains(c))
				{
					Building edifice = c.GetEdifice(map);
					if (edifice != null && edifice.def == mineableDef)
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool RecentlyStruck(IntVec3 cell, ThingDef def)
		{
			for (int i = this.strikeRecords.Count - 1; i >= 0; i--)
			{
				if (this.strikeRecords[i].Expired)
				{
					this.strikeRecords.RemoveAt(i);
				}
				else if (this.strikeRecords[i].def == def && this.strikeRecords[i].cell.InHorDistOf(cell, 12f))
				{
					return true;
				}
			}
			return false;
		}

		private bool MineableIsWorthLetter(ThingDef mineableDef)
		{
			return mineableDef.mineable && mineableDef.building.mineableThing.GetStatValueAbstract(StatDefOf.MarketValue, null) * (float)mineableDef.building.mineableYield > 10f;
		}

		public string DebugStrikeRecords()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (StrikeRecord current in this.strikeRecords)
			{
				stringBuilder.AppendLine(current.ToString());
			}
			return stringBuilder.ToString();
		}
	}
}
