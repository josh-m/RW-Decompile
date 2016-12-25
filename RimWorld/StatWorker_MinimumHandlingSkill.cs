using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StatWorker_MinimumHandlingSkill : StatWorker
	{
		public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
		{
			return this.ValueFromReq(req);
		}

		public override string GetExplanation(StatRequest req, ToStringNumberSense numberSense)
		{
			float wildness = ((ThingDef)req.Def).race.wildness;
			return string.Concat(new string[]
			{
				"Wildness".Translate(),
				" ",
				wildness.ToStringPercent(),
				": ",
				this.ValueFromReq(req).ToString("F0")
			});
		}

		private float ValueFromReq(StatRequest req)
		{
			float wildness = ((ThingDef)req.Def).race.wildness;
			return Mathf.Clamp(GenMath.LerpDouble(0.3f, 1f, 0f, 9f, wildness), 0f, 20f);
		}
	}
}
