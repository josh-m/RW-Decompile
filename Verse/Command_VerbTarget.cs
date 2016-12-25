using RimWorld;
using System;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	internal class Command_VerbTarget : Command
	{
		public Verb verb;

		public override Color IconDrawColor
		{
			get
			{
				if (this.verb.ownerEquipment != null)
				{
					return this.verb.ownerEquipment.DrawColor;
				}
				return base.IconDrawColor;
			}
		}

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			SoundDefOf.TickTiny.PlayOneShotOnCamera();
			Targeter targeter = Find.Targeter;
			if (this.verb.CasterIsPawn && targeter.targetingVerb != null && targeter.targetingVerb.verbProps == this.verb.verbProps)
			{
				Pawn casterPawn = this.verb.CasterPawn;
				if (!targeter.IsPawnTargeting(casterPawn))
				{
					targeter.targetingVerbAdditionalPawns.Add(casterPawn);
				}
			}
			else
			{
				Find.Targeter.BeginTargeting(this.verb);
			}
		}
	}
}
