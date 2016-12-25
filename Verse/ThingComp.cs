using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse
{
	public abstract class ThingComp
	{
		public ThingWithComps parent;

		public CompProperties props;

		public virtual void Initialize(CompProperties props)
		{
			this.props = props;
		}

		public virtual void ReceiveCompSignal(string signal)
		{
		}

		public virtual void PostExposeData()
		{
		}

		public virtual void PostSpawnSetup()
		{
		}

		public virtual void PostDeSpawn()
		{
		}

		public virtual void PostDestroy(DestroyMode mode, bool wasSpawned)
		{
		}

		public virtual void CompTick()
		{
		}

		public virtual void CompTickRare()
		{
		}

		public virtual void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
		{
			absorbed = false;
		}

		public virtual void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
		}

		public virtual void PostDraw()
		{
		}

		public virtual void PostDrawExtraSelectionOverlays()
		{
		}

		public virtual void PostPrintOnto(SectionLayer layer)
		{
		}

		public virtual void CompPrintForPowerGrid(SectionLayer layer)
		{
		}

		public virtual void PreAbsorbStack(Thing otherStack, int count)
		{
		}

		public virtual void PostSplitOff(Thing piece)
		{
		}

		public virtual string TransformLabel(string label)
		{
			return label;
		}

		[DebuggerHidden]
		public virtual IEnumerable<Command> CompGetGizmosExtra()
		{
		}

		public virtual bool AllowStackWith(Thing other)
		{
			return true;
		}

		public virtual string CompInspectStringExtra()
		{
			return null;
		}

		public virtual string GetDescriptionPart()
		{
			return null;
		}

		[DebuggerHidden]
		public virtual IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
		}

		public virtual void PrePreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
		{
		}

		public virtual void PostIngested(Pawn ingester)
		{
		}
	}
}
