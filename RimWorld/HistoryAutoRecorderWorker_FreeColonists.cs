using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorderWorker_FreeColonists : HistoryAutoRecorderWorker
	{
		public override float PullRecord()
		{
			return (float)PawnsFinder.AllMapsCaravansAndTravelingTransportPods_FreeColonists.Count<Pawn>();
		}
	}
}
