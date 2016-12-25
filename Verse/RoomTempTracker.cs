using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public sealed class RoomTempTracker
	{
		private const float ThinRoofEqualizeRate = 5E-05f;

		private const float NoRoofEqualizeRate = 0.0007f;

		private const float DeepEqualizeFractionPerTick = 5E-05f;

		private Room room;

		private float temperatureInt;

		private List<IntVec3> equalizeCells = new List<IntVec3>();

		private float noRoofCoverage;

		private float thickRoofCoverage;

		private int cycleIndex;

		private static int debugGetFrame = -999;

		private static float debugWallEq;

		private Map Map
		{
			get
			{
				return this.room.Map;
			}
		}

		private float ThinRoofCoverage
		{
			get
			{
				return 1f - (this.thickRoofCoverage + this.noRoofCoverage);
			}
		}

		public float Temperature
		{
			get
			{
				return this.temperatureInt;
			}
			set
			{
				this.temperatureInt = Mathf.Clamp(value, -270f, 2000f);
			}
		}

		public RoomTempTracker(Room room)
		{
			this.room = room;
			this.Temperature = this.Map.mapTemperature.OutdoorTemp;
		}

		public void RoofChanged()
		{
			this.RegenerateEqualizationData();
		}

		public void RoomChanged()
		{
			this.Map.autoBuildRoofAreaSetter.ResolveQueuedGenerateRoofs();
			this.RegenerateEqualizationData();
		}

		private void RegenerateEqualizationData()
		{
			this.thickRoofCoverage = 0f;
			this.noRoofCoverage = 0f;
			this.equalizeCells.Clear();
			if (!this.room.UsesOutdoorTemperature)
			{
				int num = 0;
				foreach (IntVec3 current in this.room.Cells)
				{
					RoofDef roof = current.GetRoof(this.Map);
					if (roof == null)
					{
						this.noRoofCoverage += 1f;
					}
					else if (roof.isThickRoof)
					{
						this.thickRoofCoverage += 1f;
					}
					num++;
				}
				this.thickRoofCoverage /= (float)num;
				this.noRoofCoverage /= (float)num;
				foreach (IntVec3 current2 in this.room.Cells)
				{
					int i = 0;
					while (i < 4)
					{
						IntVec3 intVec = current2 + GenAdj.CardinalDirections[i];
						IntVec3 intVec2 = current2 + GenAdj.CardinalDirections[i] * 2;
						if (!intVec.InBounds(this.Map))
						{
							goto IL_21A;
						}
						Region region = intVec.GetRegion(this.Map);
						if (region == null)
						{
							goto IL_21A;
						}
						if (region.portal != null)
						{
							bool flag = false;
							for (int j = 0; j < region.links.Count; j++)
							{
								Region regionA = region.links[j].RegionA;
								Region regionB = region.links[j].RegionB;
								if (regionA.Room != this.room && regionA.portal == null)
								{
									flag = true;
									break;
								}
								if (regionB.Room != this.room && regionB.portal == null)
								{
									flag = true;
									break;
								}
							}
							if (!flag)
							{
								goto IL_21A;
							}
						}
						IL_2AF:
						i++;
						continue;
						IL_21A:
						if (!intVec2.InBounds(this.Map))
						{
							goto IL_2AF;
						}
						Room room = intVec2.GetRoom(this.Map);
						if (room == this.room)
						{
							goto IL_2AF;
						}
						bool flag2 = false;
						for (int k = 0; k < 4; k++)
						{
							IntVec3 loc = intVec2 + GenAdj.CardinalDirections[k];
							if (loc.GetRoom(this.Map) == this.room)
							{
								flag2 = true;
								break;
							}
						}
						if (!flag2)
						{
							this.equalizeCells.Add(intVec2);
							goto IL_2AF;
						}
						goto IL_2AF;
					}
				}
				this.equalizeCells.Shuffle<IntVec3>();
			}
		}

		public void EqualizeTemperature()
		{
			if (this.room.UsesOutdoorTemperature)
			{
				this.Temperature = this.Map.mapTemperature.OutdoorTemp;
			}
			else
			{
				if (this.room.Regions.Count == 1 && this.room.Regions[0].portal != null)
				{
					return;
				}
				float num = this.ThinRoofEqualizationTempChangePerInterval();
				float num2 = this.NoRoofEqualizationTempChangePerInterval();
				float num3 = this.WallEqualizationTempChangePerInterval();
				float num4 = this.DeepEqualizationTempChangePerInterval();
				this.Temperature += num + num2 + num3 + num4;
			}
		}

		private float WallEqualizationTempChangePerInterval()
		{
			if (this.equalizeCells.Count == 0)
			{
				return 0f;
			}
			float num = 0f;
			int num2 = Mathf.CeilToInt((float)this.equalizeCells.Count * 0.2f);
			for (int i = 0; i < num2; i++)
			{
				this.cycleIndex++;
				int index = this.cycleIndex % this.equalizeCells.Count;
				float num3;
				if (GenTemperature.TryGetDirectAirTemperatureForCell(this.equalizeCells[index], this.Map, out num3))
				{
					num += num3 - this.Temperature;
				}
				else
				{
					num += Mathf.Lerp(this.Temperature, this.Map.mapTemperature.OutdoorTemp, 0.5f) - this.Temperature;
				}
			}
			float num4 = num / (float)num2;
			float num5 = num4 * (float)this.equalizeCells.Count;
			return num5 * 120f * 0.00017f / (float)this.room.CellCount;
		}

		private float TempDiffFromOutdoorsAdjusted()
		{
			float num = this.Map.mapTemperature.OutdoorTemp - this.temperatureInt;
			if (Mathf.Abs(num) < 100f)
			{
				return num;
			}
			return Mathf.Sign(num) * 100f + 5f * (num - Mathf.Sign(num) * 100f);
		}

		private float ThinRoofEqualizationTempChangePerInterval()
		{
			if (this.ThinRoofCoverage < 0.001f)
			{
				return 0f;
			}
			float num = this.TempDiffFromOutdoorsAdjusted();
			float num2 = num * this.ThinRoofCoverage * 5E-05f;
			return num2 * 120f;
		}

		private float NoRoofEqualizationTempChangePerInterval()
		{
			if (this.noRoofCoverage < 0.001f)
			{
				return 0f;
			}
			float num = this.TempDiffFromOutdoorsAdjusted();
			float num2 = num * this.noRoofCoverage * 0.0007f;
			return num2 * 120f;
		}

		private float DeepEqualizationTempChangePerInterval()
		{
			if (this.thickRoofCoverage < 0.001f)
			{
				return 0f;
			}
			float num = 15f - this.temperatureInt;
			if (num > 0f)
			{
				return 0f;
			}
			float num2 = num * this.thickRoofCoverage * 5E-05f;
			return num2 * 120f;
		}

		public void DebugDraw()
		{
			foreach (IntVec3 current in this.equalizeCells)
			{
				CellRenderer.RenderCell(current, 0.5f);
			}
		}

		internal string DebugString()
		{
			if (this.room.UsesOutdoorTemperature)
			{
				return "uses outdoor temperature";
			}
			if (Time.frameCount > RoomTempTracker.debugGetFrame + 120)
			{
				RoomTempTracker.debugWallEq = 0f;
				for (int i = 0; i < 40; i++)
				{
					RoomTempTracker.debugWallEq += this.WallEqualizationTempChangePerInterval();
				}
				RoomTempTracker.debugWallEq /= 40f;
				RoomTempTracker.debugGetFrame = Time.frameCount;
			}
			return string.Concat(new object[]
			{
				"  thick roof coverage: ",
				this.thickRoofCoverage.ToStringPercent("F0"),
				"\n  thin roof coverage: ",
				this.ThinRoofCoverage.ToStringPercent("F0"),
				"\n  no roof coverage: ",
				this.noRoofCoverage.ToStringPercent("F0"),
				"\n\n  wall equalization: ",
				RoomTempTracker.debugWallEq.ToStringTemperatureOffset("F3"),
				"\n  thin roof equalization: ",
				this.ThinRoofEqualizationTempChangePerInterval().ToStringTemperatureOffset("F3"),
				"\n  no roof equalization: ",
				this.NoRoofEqualizationTempChangePerInterval().ToStringTemperatureOffset("F3"),
				"\n  deep equalization: ",
				this.DeepEqualizationTempChangePerInterval().ToStringTemperatureOffset("F3"),
				"\n\n  temp diff from outdoors, adjusted: ",
				this.TempDiffFromOutdoorsAdjusted().ToStringTemperatureOffset("F3"),
				"\n  tempChange e=20 targ= 200C: ",
				GenTemperature.ControlTemperatureTempChange(this.room.Cells.First<IntVec3>(), this.room.Map, 20f, 200f),
				"\n  tempChange e=20 targ=-200C: ",
				GenTemperature.ControlTemperatureTempChange(this.room.Cells.First<IntVec3>(), this.room.Map, 20f, -200f),
				"\n  equalize interval ticks: ",
				120,
				"\n  equalize cells count:",
				this.equalizeCells.Count
			});
		}
	}
}
