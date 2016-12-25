using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_AreaBuildRoofExpand : Designator_AreaBuildRoof
	{
		public Designator_AreaBuildRoofExpand() : base(DesignateMode.Add)
		{
			this.defaultLabel = "DesignatorAreaBuildRoofExpand".Translate();
			this.defaultDesc = "DesignatorAreaBuildRoofExpandDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/BuildRoofAreaExpand", true);
			this.hotKey = KeyBindingDefOf.Misc10;
			this.soundDragSustain = SoundDefOf.DesignateDragAreaAdd;
			this.soundDragChanged = SoundDefOf.DesignateDragAreaAddChanged;
			this.soundSucceeded = SoundDefOf.DesignateAreaAdd;
			this.tutorTag = "AreaBuildRoofExpand";
		}
	}
}
