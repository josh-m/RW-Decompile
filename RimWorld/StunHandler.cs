using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StunHandler : IExposable
	{
		private const float StunDurationFactor_Standard = 20f;

		private const float StunDurationFactor_EMP = 15f;

		public Thing parent;

		private int stunTicksLeft;

		private Mote moteStun;

		private int EMPAdaptedTicksLeft;

		public bool Stunned
		{
			get
			{
				return this.stunTicksLeft > 0;
			}
		}

		private int EMPAdaptationTicksDuration
		{
			get
			{
				Pawn pawn = this.parent as Pawn;
				if (pawn != null && pawn.RaceProps.IsMechanoid)
				{
					return 2200;
				}
				return 0;
			}
		}

		public StunHandler(Thing parent)
		{
			this.parent = parent;
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.stunTicksLeft, "stunTicksLeft", 0, false);
			Scribe_Values.LookValue<int>(ref this.EMPAdaptedTicksLeft, "EMPAdaptedTicksLeft", 0, false);
		}

		public void StunHandlerTick()
		{
			if (this.EMPAdaptedTicksLeft > 0)
			{
				this.EMPAdaptedTicksLeft--;
			}
			if (this.stunTicksLeft > 0)
			{
				this.stunTicksLeft--;
				if (this.moteStun == null || this.moteStun.Destroyed)
				{
					this.moteStun = MoteMaker.MakeStunOverlay(this.parent);
				}
				Pawn pawn = this.parent as Pawn;
				if (pawn != null && pawn.Downed)
				{
					this.stunTicksLeft = 0;
				}
				if (this.moteStun != null)
				{
					this.moteStun.Maintain();
				}
			}
		}

		public void Notify_DamageApplied(DamageInfo dinfo, bool affectedByEMP)
		{
			if (dinfo.Def == DamageDefOf.Stun)
			{
				this.StunFor(Mathf.RoundToInt((float)dinfo.Amount * 20f));
			}
			else if (dinfo.Def == DamageDefOf.EMP && affectedByEMP)
			{
				if (this.EMPAdaptedTicksLeft <= 0)
				{
					this.StunFor(Mathf.RoundToInt((float)dinfo.Amount * 15f));
					this.EMPAdaptedTicksLeft = this.EMPAdaptationTicksDuration;
				}
				else
				{
					Vector3 loc = new Vector3((float)this.parent.Position.x + 1f, (float)this.parent.Position.y, (float)this.parent.Position.z + 1f);
					MoteMaker.ThrowText(loc, this.parent.Map, "Adapted".Translate(), Color.white, -1f);
				}
			}
		}

		public void StunFor(int ticks)
		{
			this.stunTicksLeft = Mathf.Max(this.stunTicksLeft, ticks);
		}
	}
}
