using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public abstract class LogEntry : IExposable
	{
		protected int ticksAbs = -1;

		protected int randSeed;

		public static readonly Texture2D Blood = ContentFinder<Texture2D>.Get("Things/Mote/BattleSymbols/Blood", true);

		public static readonly Texture2D BloodTarget = ContentFinder<Texture2D>.Get("Things/Mote/BattleSymbols/BloodTarget", true);

		public static readonly Texture2D Downed = ContentFinder<Texture2D>.Get("Things/Mote/BattleSymbols/Downed", true);

		public static readonly Texture2D DownedTarget = ContentFinder<Texture2D>.Get("Things/Mote/BattleSymbols/DownedTarget", true);

		public static readonly Texture2D Skull = ContentFinder<Texture2D>.Get("Things/Mote/BattleSymbols/Skull", true);

		public static readonly Texture2D SkullTarget = ContentFinder<Texture2D>.Get("Things/Mote/BattleSymbols/SkullTarget", true);

		public int Age
		{
			get
			{
				return Find.TickManager.TicksAbs - this.ticksAbs;
			}
		}

		public LogEntry()
		{
			this.ticksAbs = Find.TickManager.TicksAbs;
			this.randSeed = Rand.Int;
		}

		public virtual void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.ticksAbs, "ticksAbs", 0, false);
			Scribe_Values.Look<int>(ref this.randSeed, "randSeed", 0, false);
		}

		public abstract string ToGameStringFromPOV(Thing pov);

		public abstract bool Concerns(Thing t);

		public virtual void ClickedFromPOV(Thing pov)
		{
		}

		public virtual Texture2D IconFromPOV(Thing pov)
		{
			return null;
		}

		public virtual string GetTipString()
		{
			return "OccurredTimeAgo".Translate(new object[]
			{
				this.Age.ToStringTicksToPeriod(true, false, true)
			}).CapitalizeFirst() + ".";
		}

		public void Debug_OverrideTicks(int newTicks)
		{
			this.ticksAbs = newTicks;
		}
	}
}
