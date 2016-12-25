using System;

namespace Verse
{
	public class DamageWorker_RestoreBodyPart : DamageWorker
	{
		public override float Apply(DamageInfo dinfo, Thing thing)
		{
			Pawn pawn = thing as Pawn;
			if (pawn != null)
			{
				if (dinfo.Part.Value.Part == null)
				{
					Log.Warning("Tried to restore null body part.");
					return 0f;
				}
				pawn.health.RestorePart(dinfo.Part.Value.Part, null, true);
			}
			return 0f;
		}
	}
}
