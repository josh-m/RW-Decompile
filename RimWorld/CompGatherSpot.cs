using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class CompGatherSpot : ThingComp
	{
		private bool active = true;

		public bool Active
		{
			get
			{
				return this.active;
			}
			set
			{
				if (value == this.active)
				{
					return;
				}
				this.active = value;
				if (this.parent.Spawned)
				{
					if (this.active)
					{
						GatherSpotLister.RegisterActivated(this);
					}
					else
					{
						GatherSpotLister.RegisterDeactivated(this);
					}
				}
			}
		}

		public override void PostExposeData()
		{
			Scribe_Values.LookValue<bool>(ref this.active, "active", false, false);
		}

		public override void PostSpawnSetup()
		{
			base.PostSpawnSetup();
			if (this.Active)
			{
				GatherSpotLister.RegisterActivated(this);
			}
		}

		public override void PostDeSpawn()
		{
			base.PostDeSpawn();
			if (this.Active)
			{
				GatherSpotLister.RegisterDeactivated(this);
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Command> CompGetGizmosExtra()
		{
			Command_Toggle com = new Command_Toggle();
			com.hotKey = KeyBindingDefOf.CommandTogglePower;
			com.defaultLabel = "CommandGatherSpotToggleLabel".Translate();
			com.icon = TexCommand.GatherSpotActive;
			com.isActive = (() => this.<>f__this.Active);
			com.toggleAction = delegate
			{
				this.<>f__this.Active = !this.<>f__this.Active;
			};
			if (this.Active)
			{
				com.defaultDesc = "CommandGatherSpotToggleDescActive".Translate();
			}
			else
			{
				com.defaultDesc = "CommandGatherSpotToggleDescInactive".Translate();
			}
			yield return com;
		}
	}
}
