using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class ThoughtWorker
	{
		public ThoughtDef def;

		public ThoughtState CurrentState(Pawn p)
		{
			return this.PostProcessedState(this.CurrentStateInternal(p));
		}

		public ThoughtState CurrentSocialState(Pawn p, Pawn otherPawn)
		{
			return this.PostProcessedState(this.CurrentSocialStateInternal(p, otherPawn));
		}

		public virtual IEnumerable<Pawn> PotentialPawnCandidates(Pawn p)
		{
			throw new NotImplementedException(this.def.defName + " (potential pawn candidates)");
		}

		private ThoughtState PostProcessedState(ThoughtState state)
		{
			if (this.def.invert)
			{
				if (state.Active)
				{
					state = ThoughtState.Inactive;
				}
				else
				{
					state = ThoughtState.ActiveAtStage(0);
				}
			}
			return state;
		}

		protected virtual ThoughtState CurrentStateInternal(Pawn p)
		{
			throw new NotImplementedException(this.def.defName + " (normal)");
		}

		protected virtual ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
		{
			throw new NotImplementedException(this.def.defName + " (social)");
		}
	}
}
