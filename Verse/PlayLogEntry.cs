using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public abstract class PlayLogEntry : IExposable
	{
		protected int ticksAbs = -1;

		public int Age
		{
			get
			{
				return Find.TickManager.TicksAbs - this.ticksAbs;
			}
		}

		public virtual Texture2D Icon
		{
			get
			{
				return null;
			}
		}

		public virtual void ExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.ticksAbs, "ticksAbs", 0, false);
		}

		public abstract string ToGameStringFromPOV(Thing pov);

		public abstract bool Concerns(Thing t);

		public virtual void ClickedFromPOV(Thing pov)
		{
		}

		public virtual void PostRemove()
		{
		}

		public virtual string GetTipString()
		{
			return "OccurredTimeAgo".Translate(new object[]
			{
				this.Age.ToStringTicksToPeriod(true)
			}).CapitalizeFirst() + ".";
		}
	}
}
