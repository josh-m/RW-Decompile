using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_Strip : Designator
	{
		public override int DraggableDimensions
		{
			get
			{
				return 2;
			}
		}

		public Designator_Strip()
		{
			this.defaultLabel = "DesignatorStrip".Translate();
			this.defaultDesc = "DesignatorStripDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/Strip", true);
			this.soundDragSustain = SoundDefOf.DesignateDragStandard;
			this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
			this.useMouseIcon = true;
			this.soundSucceeded = SoundDefOf.DesignateClaim;
			this.hotKey = KeyBindingDefOf.Misc11;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}
			if (!this.StrippablesInCell(c).Any<Thing>())
			{
				return "MessageMustDesignateStrippable".Translate();
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			foreach (Thing current in this.StrippablesInCell(c))
			{
				this.DesignateThing(current);
			}
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			if (base.Map.designationManager.DesignationOn(t, DesignationDefOf.Strip) != null)
			{
				return false;
			}
			return StrippableUtility.CanBeStrippedByColony(t);
		}

		public override void DesignateThing(Thing t)
		{
			base.Map.designationManager.AddDesignation(new Designation(t, DesignationDefOf.Strip));
		}

		[DebuggerHidden]
		private IEnumerable<Thing> StrippablesInCell(IntVec3 c)
		{
			if (!c.Fogged(base.Map))
			{
				List<Thing> thingList = c.GetThingList(base.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					if (this.CanDesignateThing(thingList[i]).Accepted)
					{
						yield return thingList[i];
					}
				}
			}
		}
	}
}
