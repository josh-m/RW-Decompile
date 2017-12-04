using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompHibernatable : ThingComp
	{
		private HibernatableStateDef state = HibernatableStateDefOf.Running;

		private int endStartupTick;

		public const string HibernateStartingSignal = "HibernateStarting";

		public const string HibernateRunningSignal = "HibernateRunning";

		private CompProperties_Hibernatable Props
		{
			get
			{
				return (CompProperties_Hibernatable)this.props;
			}
		}

		public HibernatableStateDef State
		{
			get
			{
				return this.state;
			}
			set
			{
				if (this.state == value)
				{
					return;
				}
				this.state = value;
				if (this.state == HibernatableStateDefOf.Starting)
				{
					this.parent.BroadcastCompSignal("HibernateStarting");
				}
				if (this.state == HibernatableStateDefOf.Running)
				{
					this.parent.BroadcastCompSignal("HibernateRunning");
				}
			}
		}

		public bool Running
		{
			get
			{
				return this.State == HibernatableStateDefOf.Running;
			}
		}

		public void Startup()
		{
			if (this.State != HibernatableStateDefOf.Hibernating)
			{
				Log.ErrorOnce("Attempted to start a non-hibernating object", 34361223);
				return;
			}
			this.State = HibernatableStateDefOf.Starting;
			this.endStartupTick = Mathf.RoundToInt((float)Find.TickManager.TicksGame + this.Props.startupDays * 60000f);
			EscapeShipComp component = this.parent.Map.info.parent.GetComponent<EscapeShipComp>();
			if (component != null)
			{
				component.raidBeaconEnabled = true;
			}
		}

		public override string CompInspectStringExtra()
		{
			if (this.State == HibernatableStateDefOf.Hibernating)
			{
				return "HibernatableHibernating".Translate();
			}
			if (this.State == HibernatableStateDefOf.Starting)
			{
				return string.Format("{0}: {1}", "HibernatableStartingUp".Translate(), (this.endStartupTick - Find.TickManager.TicksGame).ToStringTicksToPeriod(true, false, true));
			}
			return null;
		}

		public override void CompTick()
		{
			if (this.State == HibernatableStateDefOf.Starting && Find.TickManager.TicksGame > this.endStartupTick)
			{
				this.State = HibernatableStateDefOf.Running;
				this.endStartupTick = 0;
				Find.LetterStack.ReceiveLetter("HibernateCompleteLabel".Translate(), "HibernateComplete".Translate(), LetterDefOf.PositiveEvent, new GlobalTargetInfo(this.parent), null);
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Defs.Look<HibernatableStateDef>(ref this.state, "hibernateState");
			Scribe_Values.Look<int>(ref this.endStartupTick, "hibernateendStartupTick", 0, false);
		}
	}
}
