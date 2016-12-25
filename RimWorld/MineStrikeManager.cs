using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public static class MineStrikeManager
	{
		private struct StrikeRecord
		{
			public IntVec3 cell;

			public ThingDef def;

			public int ticksGame;

			public bool Expired
			{
				get
				{
					return Find.TickManager.TicksGame > this.ticksGame + 900000;
				}
			}

			public override string ToString()
			{
				return string.Concat(new object[]
				{
					"(",
					this.cell,
					", ",
					this.def,
					", ",
					this.ticksGame,
					")"
				});
			}
		}

		private const int RecentStrikeIgnoreRadius = 12;

		private const int StrikeRecordExpiryDays = 15;

		private static List<MineStrikeManager.StrikeRecord> strikeRecords = new List<MineStrikeManager.StrikeRecord>();

		private static readonly int RadialVisibleCells = GenRadial.NumCellsInRadius(6f);

		public static void CheckStruckOre(IntVec3 justMinedPos, ThingDef justMinedDef, Thing miner)
		{
			if (miner.Faction != Faction.OfPlayer)
			{
				return;
			}
			for (int i = 0; i < 4; i++)
			{
				IntVec3 intVec = justMinedPos + GenAdj.CardinalDirections[i];
				if (intVec.InBounds())
				{
					Building edifice = intVec.GetEdifice();
					if (edifice != null && edifice.def != justMinedDef && MineStrikeManager.MineableIsWorthLetter(edifice.def) && !MineStrikeManager.AlreadyVisibleNearby(intVec, edifice.def) && !MineStrikeManager.RecentlyStruck(intVec, edifice.def))
					{
						MineStrikeManager.StrikeRecord item = default(MineStrikeManager.StrikeRecord);
						item.cell = intVec;
						item.def = edifice.def;
						item.ticksGame = Find.TickManager.TicksGame;
						MineStrikeManager.strikeRecords.Add(item);
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

		public static bool AlreadyVisibleNearby(IntVec3 center, ThingDef mineableDef)
		{
			CellRect cellRect = CellRect.CenteredOn(center, 1);
			for (int i = 1; i < MineStrikeManager.RadialVisibleCells; i++)
			{
				IntVec3 c = center + GenRadial.RadialPattern[i];
				if (c.InBounds() && !c.Fogged() && !cellRect.Contains(c))
				{
					Building edifice = c.GetEdifice();
					if (edifice != null && edifice.def == mineableDef)
					{
						return true;
					}
				}
			}
			return false;
		}

		private static bool RecentlyStruck(IntVec3 cell, ThingDef def)
		{
			for (int i = MineStrikeManager.strikeRecords.Count - 1; i >= 0; i--)
			{
				if (MineStrikeManager.strikeRecords[i].Expired)
				{
					MineStrikeManager.strikeRecords.RemoveAt(i);
				}
				else if (MineStrikeManager.strikeRecords[i].def == def && MineStrikeManager.strikeRecords[i].cell.InHorDistOf(cell, 12f))
				{
					return true;
				}
			}
			return false;
		}

		private static bool MineableIsWorthLetter(ThingDef mineableDef)
		{
			return mineableDef.mineable && mineableDef.building.mineableThing.GetStatValueAbstract(StatDefOf.MarketValue, null) > 1.5f;
		}

		public static string DebugStrikeRecords()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (MineStrikeManager.StrikeRecord current in MineStrikeManager.strikeRecords)
			{
				stringBuilder.AppendLine(current.ToString());
			}
			return stringBuilder.ToString();
		}
	}
}
