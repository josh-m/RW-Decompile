using RimWorld;
using System;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class Command_Target : Command
	{
		public Action<Thing> action;

		public TargetingParameters targetingParams;

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			SoundDefOf.TickTiny.PlayOneShotOnCamera();
			Find.Targeter.BeginTargeting(this.targetingParams, delegate(LocalTargetInfo target)
			{
				this.action(target.Thing);
			}, null, null, null);
		}

		public override bool InheritInteractionsFrom(Gizmo other)
		{
			return false;
		}
	}
}
