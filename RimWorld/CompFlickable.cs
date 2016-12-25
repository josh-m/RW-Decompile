using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class CompFlickable : ThingComp
	{
		public const string FlickedOnSignal = "FlickedOn";

		public const string FlickedOffSignal = "FlickedOff";

		private bool switchOnInt = true;

		private bool wantSwitchOn = true;

		public bool SwitchIsOn
		{
			get
			{
				return this.switchOnInt;
			}
			set
			{
				if (this.switchOnInt == value)
				{
					return;
				}
				this.switchOnInt = value;
				if (this.switchOnInt)
				{
					this.parent.BroadcastCompSignal("FlickedOn");
				}
				else
				{
					this.parent.BroadcastCompSignal("FlickedOff");
				}
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.LookValue<bool>(ref this.switchOnInt, "switchOn", true, false);
			Scribe_Values.LookValue<bool>(ref this.wantSwitchOn, "wantSwitchOn", true, false);
		}

		public bool WantsFlick()
		{
			return this.wantSwitchOn != this.switchOnInt;
		}

		public void DoFlick()
		{
			this.SwitchIsOn = !this.SwitchIsOn;
			SoundDefOf.FlickSwitch.PlayOneShot(this.parent.Position);
		}

		public void ResetToOn()
		{
			this.switchOnInt = true;
			this.wantSwitchOn = true;
		}

		[DebuggerHidden]
		public override IEnumerable<Command> CompGetGizmosExtra()
		{
			foreach (Command c in base.CompGetGizmosExtra())
			{
				yield return c;
			}
			if (this.parent.Faction == Faction.OfPlayer)
			{
				yield return new Command_Toggle
				{
					hotKey = KeyBindingDefOf.CommandTogglePower,
					icon = TexCommand.DesirePower,
					defaultLabel = "CommandDesignateTogglePowerLabel".Translate(),
					defaultDesc = "CommandDesignateTogglePowerDesc".Translate(),
					isActive = (() => this.<>f__this.wantSwitchOn),
					toggleAction = delegate
					{
						this.<>f__this.wantSwitchOn = !this.<>f__this.wantSwitchOn;
						FlickUtility.UpdateFlickDesignation(this.<>f__this.parent);
					}
				};
			}
		}
	}
}
