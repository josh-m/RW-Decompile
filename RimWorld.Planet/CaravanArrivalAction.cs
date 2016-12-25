using System;
using Verse;

namespace RimWorld.Planet
{
	public abstract class CaravanArrivalAction : IExposable
	{
		public abstract string ReportString
		{
			get;
		}

		public virtual bool ShouldFail
		{
			get
			{
				return false;
			}
		}

		public virtual bool ArriveOnTouch
		{
			get
			{
				return false;
			}
		}

		public abstract void Arrived(Caravan caravan);

		public virtual void ExposeData()
		{
		}
	}
}
