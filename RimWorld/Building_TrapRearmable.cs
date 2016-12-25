using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Building_TrapRearmable : Building_Trap
	{
		private bool autoRearm;

		private bool armedInt = true;

		private Graphic graphicUnarmedInt;

		private static readonly FloatRange TrapDamageFactor = new FloatRange(0.7f, 1.3f);

		private static readonly IntRange DamageCount = new IntRange(1, 2);

		public override bool Armed
		{
			get
			{
				return this.armedInt;
			}
		}

		public override Graphic Graphic
		{
			get
			{
				if (this.armedInt)
				{
					return base.Graphic;
				}
				if (this.graphicUnarmedInt == null)
				{
					this.graphicUnarmedInt = this.def.building.trapUnarmedGraphicData.GraphicColoredFor(this);
				}
				return this.graphicUnarmedInt;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<bool>(ref this.armedInt, "armed", false, false);
			Scribe_Values.LookValue<bool>(ref this.autoRearm, "autoRearm", false, false);
		}

		protected override void SpringSub(Pawn p)
		{
			this.armedInt = false;
			this.DamagePawn(p);
			if (this.autoRearm)
			{
				base.Map.designationManager.AddDesignation(new Designation(this, DesignationDefOf.RearmTrap));
			}
		}

		public void Rearm()
		{
			this.armedInt = true;
			SoundDef.Named("TrapArm").PlayOneShot(new TargetInfo(base.Position, base.Map, false));
		}

		private void DamagePawn(Pawn p)
		{
			BodyPartHeight height = (Rand.Value >= 0.666f) ? BodyPartHeight.Middle : BodyPartHeight.Top;
			int num = Mathf.RoundToInt(this.GetStatValue(StatDefOf.TrapMeleeDamage, true) * Building_TrapRearmable.TrapDamageFactor.RandomInRange);
			int randomInRange = Building_TrapRearmable.DamageCount.RandomInRange;
			for (int i = 0; i < randomInRange; i++)
			{
				if (num <= 0)
				{
					break;
				}
				int num2 = Mathf.Max(1, Mathf.RoundToInt(Rand.Value * (float)num));
				num -= num2;
				DamageInfo dinfo = new DamageInfo(DamageDefOf.Stab, num2, -1f, this, null, null);
				dinfo.SetBodyRegion(height, BodyPartDepth.Outside);
				p.TakeDamage(dinfo);
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo g in base.GetGizmos())
			{
				yield return g;
			}
			yield return new Command_Toggle
			{
				defaultLabel = "CommandAutoRearm".Translate(),
				defaultDesc = "CommandAutoRearmDesc".Translate(),
				hotKey = KeyBindingDefOf.Misc3,
				icon = TexCommand.RearmTrap,
				isActive = (() => this.<>f__this.autoRearm),
				toggleAction = delegate
				{
					this.<>f__this.autoRearm = !this.<>f__this.autoRearm;
				}
			};
		}
	}
}
