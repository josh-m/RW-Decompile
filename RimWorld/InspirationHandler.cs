using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class InspirationHandler : IExposable
	{
		public Pawn pawn;

		private Inspiration curState;

		private const int CheckStartInspirationIntervalTicks = 100;

		private const float MinMood = 0.5f;

		private const float StartInspirationMTBDaysAtMaxMood = 10f;

		public bool Inspired
		{
			get
			{
				return this.curState != null;
			}
		}

		public Inspiration CurState
		{
			get
			{
				return this.curState;
			}
		}

		public InspirationDef CurStateDef
		{
			get
			{
				return (this.curState == null) ? null : this.curState.def;
			}
		}

		private float StartInspirationMTBDays
		{
			get
			{
				if (this.pawn.needs.mood == null)
				{
					return -1f;
				}
				float curLevel = this.pawn.needs.mood.CurLevel;
				if (curLevel < 0.5f)
				{
					return -1f;
				}
				return GenMath.LerpDouble(0.5f, 1f, 210f, 10f, curLevel);
			}
		}

		public InspirationHandler(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Deep.Look<Inspiration>(ref this.curState, "curState", new object[0]);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && this.curState != null)
			{
				this.curState.pawn = this.pawn;
			}
		}

		public void InspirationHandlerTick()
		{
			if (this.curState != null)
			{
				this.curState.InspirationTick();
			}
			if (this.pawn.IsHashIntervalTick(100))
			{
				this.CheckStartRandomInspiration();
			}
		}

		public bool TryStartInspiration(InspirationDef def)
		{
			if (this.Inspired)
			{
				return false;
			}
			if (!def.Worker.InspirationCanOccur(this.pawn))
			{
				return false;
			}
			this.curState = (Inspiration)Activator.CreateInstance(def.inspirationClass);
			this.curState.def = def;
			this.curState.pawn = this.pawn;
			this.curState.PostStart();
			return true;
		}

		public void EndInspiration(Inspiration inspiration)
		{
			if (inspiration == null)
			{
				return;
			}
			if (this.curState != inspiration)
			{
				Log.Error("Tried to end inspiration " + inspiration.ToStringSafe<Inspiration>() + " but current inspiration is " + this.curState.ToStringSafe<Inspiration>(), false);
				return;
			}
			this.curState = null;
			inspiration.PostEnd();
		}

		public void EndInspiration(InspirationDef inspirationDef)
		{
			if (this.curState != null && this.curState.def == inspirationDef)
			{
				this.EndInspiration(this.curState);
			}
		}

		public void Reset()
		{
			this.curState = null;
		}

		private void CheckStartRandomInspiration()
		{
			if (this.Inspired)
			{
				return;
			}
			float startInspirationMTBDays = this.StartInspirationMTBDays;
			if (startInspirationMTBDays < 0f)
			{
				return;
			}
			if (Rand.MTBEventOccurs(startInspirationMTBDays, 60000f, 100f))
			{
				InspirationDef randomAvailableInspirationDef = this.GetRandomAvailableInspirationDef();
				if (randomAvailableInspirationDef != null)
				{
					this.TryStartInspiration(randomAvailableInspirationDef);
				}
			}
		}

		private InspirationDef GetRandomAvailableInspirationDef()
		{
			return (from x in DefDatabase<InspirationDef>.AllDefsListForReading
			where x.Worker.InspirationCanOccur(this.pawn)
			select x).RandomElementByWeightWithFallback((InspirationDef x) => x.Worker.CommonalityFor(this.pawn), null);
		}
	}
}
