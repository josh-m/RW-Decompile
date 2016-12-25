using System;
using Verse;

namespace RimWorld
{
	public struct AlertReport
	{
		public bool active;

		public Thing culprit;

		public static AlertReport Active
		{
			get
			{
				return new AlertReport
				{
					active = true
				};
			}
		}

		public static AlertReport Inactive
		{
			get
			{
				return new AlertReport
				{
					active = false
				};
			}
		}

		public static AlertReport CulpritIs(Thing culp)
		{
			return new AlertReport
			{
				active = (culp != null),
				culprit = culp
			};
		}

		public static implicit operator AlertReport(bool b)
		{
			return new AlertReport
			{
				active = b
			};
		}

		public static implicit operator AlertReport(Thing culprit)
		{
			return AlertReport.CulpritIs(culprit);
		}
	}
}
