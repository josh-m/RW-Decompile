using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public sealed class MapConditionManager : IExposable
	{
		public Map map;

		private List<MapCondition> activeConditions = new List<MapCondition>();

		public List<MapCondition> ActiveConditions
		{
			get
			{
				return this.activeConditions;
			}
		}

		public MapConditionManager(Map map)
		{
			this.map = map;
		}

		public void RegisterCondition(MapCondition cond)
		{
			this.activeConditions.Add(cond);
			cond.mapConditionManager = this;
			cond.Init();
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<MapCondition>(ref this.activeConditions, "activeConditions", LookMode.Deep, new object[0]);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				for (int i = 0; i < this.activeConditions.Count; i++)
				{
					this.activeConditions[i].mapConditionManager = this;
				}
			}
		}

		public void MapConditionManagerTick()
		{
			for (int i = this.activeConditions.Count - 1; i >= 0; i--)
			{
				MapCondition mapCondition = this.activeConditions[i];
				if (mapCondition.Expired)
				{
					mapCondition.End();
				}
				else
				{
					mapCondition.MapConditionTick();
				}
			}
		}

		public void MapConditionManagerDraw()
		{
			for (int i = this.activeConditions.Count - 1; i >= 0; i--)
			{
				this.activeConditions[i].MapConditionDraw();
			}
		}

		public bool ConditionIsActive(MapConditionDef def)
		{
			return this.GetActiveCondition(def) != null;
		}

		public MapCondition GetActiveCondition(MapConditionDef def)
		{
			for (int i = 0; i < this.activeConditions.Count; i++)
			{
				if (def == this.activeConditions[i].def)
				{
					return this.activeConditions[i];
				}
			}
			return null;
		}

		public T GetActiveCondition<T>() where T : MapCondition
		{
			for (int i = 0; i < this.activeConditions.Count; i++)
			{
				T t = this.activeConditions[i] as T;
				if (t != null)
				{
					return t;
				}
			}
			return (T)((object)null);
		}

		public float TotalHeightAt(float width)
		{
			float num = 0f;
			for (int i = 0; i < this.activeConditions.Count; i++)
			{
				num += Text.CalcHeight(this.activeConditions[i].LabelCap, width);
			}
			return num;
		}

		public void DoConditionsUI(Rect rect)
		{
			GUI.BeginGroup(rect);
			float num = 0f;
			for (int i = 0; i < this.activeConditions.Count; i++)
			{
				float width = rect.width - 15f;
				Rect rect2 = new Rect(0f, num, width, Text.CalcHeight(this.activeConditions[i].LabelCap, width));
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleRight;
				Widgets.Label(rect2, this.activeConditions[i].LabelCap);
				MapCondition localCond = this.activeConditions[i];
				TooltipHandler.TipRegion(rect2, () => localCond.TooltipString, i * 631);
				num += rect2.height;
			}
			GUI.EndGroup();
			Text.Anchor = TextAnchor.UpperLeft;
		}

		internal float AggregateSkyTargetLerpFactor()
		{
			float num = 0f;
			for (int i = 0; i < this.activeConditions.Count; i++)
			{
				num += (1f - num) * this.activeConditions[i].SkyTargetLerpFactor();
			}
			return Mathf.Clamp01(num);
		}

		internal SkyTarget? AggregateSkyTarget()
		{
			SkyTarget? result = null;
			float num = 0f;
			for (int i = 0; i < this.activeConditions.Count; i++)
			{
				MapCondition mapCondition = this.activeConditions[i];
				float num2 = mapCondition.SkyTargetLerpFactor();
				if (num2 > 0f)
				{
					num += num2;
					if (!result.HasValue)
					{
						result = mapCondition.SkyTarget();
					}
					else
					{
						result = new SkyTarget?(SkyTarget.Lerp(result.Value, mapCondition.SkyTarget().Value, num2 / num));
					}
				}
			}
			return result;
		}

		internal float AggregateTemperatureOffset()
		{
			float num = 0f;
			for (int i = 0; i < this.activeConditions.Count; i++)
			{
				num += this.activeConditions[i].TemperatureOffset();
			}
			return num;
		}

		internal float AggregateAnimalDensityFactor()
		{
			float num = 1f;
			for (int i = 0; i < this.activeConditions.Count; i++)
			{
				num *= this.activeConditions[i].AnimalDensityFactor();
			}
			return num;
		}

		internal float AggregatePlantDensityFactor()
		{
			float num = 1f;
			for (int i = 0; i < this.activeConditions.Count; i++)
			{
				num *= this.activeConditions[i].PlantDensityFactor();
			}
			return num;
		}

		internal bool AllowEnjoyableOutsideNow()
		{
			MapConditionDef mapConditionDef;
			return this.AllowEnjoyableOutsideNow(out mapConditionDef);
		}

		internal bool AllowEnjoyableOutsideNow(out MapConditionDef reason)
		{
			for (int i = 0; i < this.activeConditions.Count; i++)
			{
				MapCondition mapCondition = this.activeConditions[i];
				if (!mapCondition.AllowEnjoyableOutsideNow())
				{
					reason = mapCondition.def;
					return false;
				}
			}
			reason = null;
			return true;
		}

		public string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (MapCondition current in this.activeConditions)
			{
				stringBuilder.AppendLine(Scribe.DebugOutputFor(current));
			}
			return stringBuilder.ToString();
		}
	}
}
