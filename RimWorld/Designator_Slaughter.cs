using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_Slaughter : Designator
	{
		public override int DraggableDimensions
		{
			get
			{
				return 2;
			}
		}

		public Designator_Slaughter()
		{
			this.defaultLabel = "DesignatorSlaughter".Translate();
			this.defaultDesc = "DesignatorSlaughterDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/Slaughter", true);
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
			if (!this.SlaughterablesInCell(c).Any<Pawn>())
			{
				return "MessageMustDesignateSlaughterable".Translate();
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 loc)
		{
			foreach (Pawn current in this.SlaughterablesInCell(loc))
			{
				this.DesignateThing(current);
			}
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			Pawn pawn = t as Pawn;
			if (pawn != null && pawn.def.race.Animal && pawn.Faction == Faction.OfPlayer && Find.DesignationManager.DesignationOn(pawn, DesignationDefOf.Slaughter) == null && !pawn.InAggroMentalState)
			{
				return true;
			}
			return false;
		}

		public override void DesignateThing(Thing t)
		{
			Find.DesignationManager.AddDesignation(new Designation(t, DesignationDefOf.Slaughter));
		}

		[DebuggerHidden]
		private IEnumerable<Pawn> SlaughterablesInCell(IntVec3 c)
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
