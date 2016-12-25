using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Building_BlastingCharge : Building
	{
		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			Command_Action com = new Command_Action();
			com.icon = ContentFinder<Texture2D>.Get("UI/Commands/Detonate", true);
			com.defaultDesc = "CommandDetonateDesc".Translate();
			com.action = new Action(this.Command_Detonate);
			if (base.GetComp<CompExplosive>().wickStarted)
			{
				com.Disable(null);
			}
			com.defaultLabel = "CommandDetonateLabel".Translate();
			yield return com;
		}

		private void Command_Detonate()
		{
			base.GetComp<CompExplosive>().StartWick(null);
		}
	}
}
