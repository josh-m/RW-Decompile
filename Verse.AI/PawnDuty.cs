using RimWorld;
using System;

namespace Verse.AI
{
	public class PawnDuty : IExposable
	{
		public DutyDef def;

		public LocalTargetInfo focus = LocalTargetInfo.Invalid;

		public LocalTargetInfo focusSecond = LocalTargetInfo.Invalid;

		public float radius = -1f;

		public LocomotionUrgency locomotion;

		public Danger maxDanger;

		public CellRect spectateRect = default(CellRect);

		public SpectateRectSide spectateRectAllowedSides = SpectateRectSide.All;

		public bool canDig;

		public PawnsToGather pawnsToGather;

		public int transportersGroup = -1;

		public PawnDuty()
		{
		}

		public PawnDuty(DutyDef def)
		{
			this.def = def;
		}

		public PawnDuty(DutyDef def, LocalTargetInfo focus, float radius = -1f) : this(def)
		{
			this.focus = focus;
			this.radius = radius;
		}

		public PawnDuty(DutyDef def, LocalTargetInfo focus, LocalTargetInfo focusSecond, float radius = -1f) : this(def, focus, radius)
		{
			this.focusSecond = focusSecond;
		}

		public void ExposeData()
		{
			Scribe_Defs.LookDef<DutyDef>(ref this.def, "def");
			Scribe_TargetInfo.LookTargetInfo(ref this.focus, "focus", LocalTargetInfo.Invalid);
			Scribe_TargetInfo.LookTargetInfo(ref this.focusSecond, "focusSecond", LocalTargetInfo.Invalid);
			Scribe_Values.LookValue<float>(ref this.radius, "radius", -1f, false);
			Scribe_Values.LookValue<LocomotionUrgency>(ref this.locomotion, "locomotion", LocomotionUrgency.None, false);
			Scribe_Values.LookValue<Danger>(ref this.maxDanger, "maxDanger", Danger.Unspecified, false);
			Scribe_Values.LookValue<CellRect>(ref this.spectateRect, "spectateRect", default(CellRect), false);
			Scribe_Values.LookValue<SpectateRectSide>(ref this.spectateRectAllowedSides, "spectateRectAllowedSides", SpectateRectSide.All, false);
			Scribe_Values.LookValue<bool>(ref this.canDig, "canDig", false, false);
			Scribe_Values.LookValue<PawnsToGather>(ref this.pawnsToGather, "pawnsToGather", PawnsToGather.None, false);
			Scribe_Values.LookValue<int>(ref this.transportersGroup, "transportersGroup", -1, false);
		}

		public override string ToString()
		{
			string text = (!this.focus.IsValid) ? string.Empty : this.focus.ToString();
			string text2 = (!this.focusSecond.IsValid) ? string.Empty : (", second=" + this.focusSecond.ToString());
			string text3 = (this.radius <= 0f) ? string.Empty : (", rad=" + this.radius.ToString("F2"));
			return string.Concat(new object[]
			{
				"(",
				this.def,
				" ",
				text,
				text2,
				text3,
				")"
			});
		}

		internal void DrawDebug(Pawn pawn)
		{
			if (this.focus.IsValid)
			{
				GenDraw.DrawLineBetween(pawn.DrawPos, this.focus.Cell.ToVector3Shifted());
				if (this.radius > 0f)
				{
					GenDraw.DrawRadiusRing(this.focus.Cell, this.radius);
				}
			}
		}
	}
}
