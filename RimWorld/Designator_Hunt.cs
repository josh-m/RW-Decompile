using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_Hunt : Designator
	{
		private List<Pawn> justDesignated = new List<Pawn>();

		public override int DraggableDimensions
		{
			get
			{
				return 2;
			}
		}

		public Designator_Hunt()
		{
			this.defaultLabel = "DesignatorHunt".Translate();
			this.defaultDesc = "DesignatorHuntDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/Hunt", true);
			this.soundDragSustain = SoundDefOf.DesignateDragStandard;
			this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
			this.useMouseIcon = true;
			this.soundSucceeded = SoundDefOf.DesignateHunt;
			this.hotKey = KeyBindingDefOf.Misc11;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds())
			{
				return false;
			}
			if (!this.HuntablesInCell(c).Any<Pawn>())
			{
				return "MessageMustDesignateHuntable".Translate();
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 loc)
		{
			foreach (Pawn current in this.HuntablesInCell(loc))
			{
				this.DesignateThing(current);
			}
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			Pawn pawn = t as Pawn;
			if (pawn != null && pawn.def.race.Animal && pawn.Faction == null && Find.DesignationManager.DesignationOn(pawn, DesignationDefOf.Hunt) == null)
			{
				return true;
			}
			return false;
		}

		public override void DesignateThing(Thing t)
		{
			Find.DesignationManager.RemoveAllDesignationsOn(t, false);
			Find.DesignationManager.AddDesignation(new Designation(t, DesignationDefOf.Hunt));
			this.justDesignated.Add((Pawn)t);
		}

		protected override void FinalizeDesignationSucceeded()
		{
			base.FinalizeDesignationSucceeded();
			foreach (PawnKindDef kind in (from p in this.justDesignated
			select p.kindDef).Distinct<PawnKindDef>())
			{
				if (kind.RaceProps.manhunterOnDamageChance > 0.2f)
				{
					Messages.Message("MessageAnimalsGoPsychoHunted".Translate(new object[]
					{
						kind.label
					}), this.justDesignated.First((Pawn x) => x.kindDef == kind), MessageSound.Standard);
				}
			}
			this.justDesignated.Clear();
		}

		[DebuggerHidden]
		private IEnumerable<Pawn> HuntablesInCell(IntVec3 c)
		{
			if (!c.Fogged())
			{
				List<Thing> thingList = c.GetThingList();
				for (int i = 0; i < thingList.Count; i++)
				{
					if (this.CanDesignateThing(thingList[i]).Accepted)
					{
						yield return (Pawn)thingList[i];
					}
				}
			}
		}
	}
}
