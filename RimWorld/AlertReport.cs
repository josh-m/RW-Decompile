using RimWorld.Planet;
using System;
using Verse;

namespace RimWorld
{
	public struct AlertReport
	{
		public bool active;

		public GlobalTargetInfo culprit;

		public static AlertReport Active
		{
			get
			{
				return new AlertReport
				{
					active = true,
					culprit = GlobalTargetInfo.Invalid
				};
			}
		}

		public static AlertReport Inactive
		{
			get
			{
				return new AlertReport
				{
					active = false,
					culprit = GlobalTargetInfo.Invalid
				};
			}
		}

		public static AlertReport CulpritIs(GlobalTargetInfo culp)
		{
			return new AlertReport
			{
				active = culp.IsValid,
				culprit = culp
			};
		}

		public static implicit operator AlertReport(bool b)
		{
			return new AlertReport
			{
				active = b,
				culprit = GlobalTargetInfo.Invalid
			};
		}

		public static implicit operator AlertReport(Thing culprit)
		{
			return AlertReport.CulpritIs(culprit);
		}

		public static implicit operator AlertReport(WorldObject culprit)
		{
			return AlertReport.CulpritIs(culprit);
		}

		public static implicit operator AlertReport(GlobalTargetInfo culprit)
		{
			return AlertReport.CulpritIs(culprit);
		}
	}
}
