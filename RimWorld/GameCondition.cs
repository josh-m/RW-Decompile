using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GameCondition : IExposable
	{
		public GameConditionManager gameConditionManager;

		public GameConditionDef def;

		public int startTick;

		public int duration = -1;

		protected Map Map
		{
			get
			{
				return this.gameConditionManager.map;
			}
		}

		public virtual string Label
		{
			get
			{
				return this.def.label;
			}
		}

		public virtual string LabelCap
		{
			get
			{
				return this.Label.CapitalizeFirst();
			}
		}

		public virtual bool Expired
		{
			get
			{
				return Find.TickManager.TicksGame > this.startTick + this.duration;
			}
		}

		public int TicksPassed
		{
			get
			{
				return Find.TickManager.TicksGame - this.startTick;
			}
		}

		public int TicksLeft
		{
			get
			{
				return this.duration - this.TicksPassed;
			}
		}

		public bool Permanent
		{
			get
			{
				return this.duration > 1000000000;
			}
			set
			{
				if (value)
				{
					this.duration = 2147483647;
				}
			}
		}

		public virtual string TooltipString
		{
			get
			{
				string text = this.def.LabelCap;
				if (this.Permanent)
				{
					text = text + "\n" + "Permanent".Translate().CapitalizeFirst();
				}
				else
				{
					Vector2 location;
					if (this.Map != null)
					{
						location = Find.WorldGrid.LongLatOf(this.Map.Tile);
					}
					else if (Find.VisibleMap != null)
					{
						location = Find.WorldGrid.LongLatOf(Find.VisibleMap.Tile);
					}
					else if (Find.AnyPlayerHomeMap != null)
					{
						location = Find.WorldGrid.LongLatOf(Find.AnyPlayerHomeMap.Tile);
					}
					else
					{
						location = Vector2.zero;
					}
					string text2 = text;
					text = string.Concat(new string[]
					{
						text2,
						"\n",
						"Started".Translate(),
						": ",
						GenDate.DateFullStringAt((long)GenDate.TickGameToAbs(this.startTick), location)
					});
					text2 = text;
					text = string.Concat(new string[]
					{
						text2,
						"\n",
						"Lasted".Translate(),
						": ",
						this.TicksPassed.ToStringTicksToPeriod(true, false, true)
					});
				}
				text += "\n";
				return text + "\n" + this.def.description;
			}
		}

		public virtual void ExposeData()
		{
			Scribe_Defs.Look<GameConditionDef>(ref this.def, "def");
			Scribe_Values.Look<int>(ref this.startTick, "startTick", 0, false);
			Scribe_Values.Look<int>(ref this.duration, "duration", 0, false);
		}

		public virtual void GameConditionTick()
		{
		}

		public virtual void GameConditionDraw()
		{
		}

		public virtual void Init()
		{
		}

		public virtual void End()
		{
			if (this.def.endMessage != null)
			{
				Messages.Message(this.def.endMessage, MessageSound.Standard);
			}
			this.gameConditionManager.ActiveConditions.Remove(this);
		}

		public virtual float TemperatureOffset()
		{
			return 0f;
		}

		public virtual float SkyTargetLerpFactor()
		{
			return 0f;
		}

		public virtual SkyTarget? SkyTarget()
		{
			return null;
		}

		public virtual float AnimalDensityFactor()
		{
			return 1f;
		}

		public virtual float PlantDensityFactor()
		{
			return 1f;
		}

		public virtual bool AllowEnjoyableOutsideNow()
		{
			return true;
		}

		public virtual List<SkyOverlay> SkyOverlays()
		{
			return null;
		}

		public virtual void DoCellSteadyEffects(IntVec3 c)
		{
		}
	}
}
