using System;
using Verse;

namespace RimWorld
{
	public abstract class CompUseEffect : ThingComp
	{
		private const float CameraShakeMag = 1f;

		public virtual float OrderPriority
		{
			get
			{
				return 0f;
			}
		}

		private CompProperties_UseEffect Props
		{
			get
			{
				return (CompProperties_UseEffect)this.props;
			}
		}

		public virtual void DoEffect(Pawn usedBy)
		{
			if (this.Props.doCameraShake && usedBy.Spawned && usedBy.MapHeld == Find.VisibleMap)
			{
				Find.CameraDriver.shaker.DoShake(1f);
			}
		}

		public virtual bool SelectedUseOption(Pawn p)
		{
			return false;
		}
	}
}
