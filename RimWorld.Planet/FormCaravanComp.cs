using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class FormCaravanComp : WorldObjectComp
	{
		public static readonly Texture2D FormCaravanCommand = ContentFinder<Texture2D>.Get("UI/Commands/FormCaravan", true);

		private WorldObjectCompProperties_FormCaravan Props
		{
			get
			{
				return (WorldObjectCompProperties_FormCaravan)this.props;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			MapParent mapParent = this.parent as MapParent;
			if (mapParent.HasMap)
			{
				if (!this.Props.reformCaravan)
				{
					yield return new Command_Action
					{
						defaultLabel = "CommandFormCaravan".Translate(),
						defaultDesc = "CommandFormCaravanDesc".Translate(),
						icon = FormCaravanComp.FormCaravanCommand,
						hotKey = KeyBindingDefOf.Misc2,
						tutorTag = "FormCaravan",
						action = delegate
						{
							Find.WindowStack.Add(new Dialog_FormCaravan(mapParent.Map, false, null, true, false));
						}
					};
				}
				else if (mapParent.Map.mapPawns.FreeColonistsSpawnedCount != 0)
				{
					Command_Action reformCaravan = new Command_Action();
					reformCaravan.defaultLabel = "CommandReformCaravan".Translate();
					reformCaravan.defaultDesc = "CommandReformCaravanDesc".Translate();
					reformCaravan.icon = FormCaravanComp.FormCaravanCommand;
					reformCaravan.hotKey = KeyBindingDefOf.Misc2;
					reformCaravan.tutorTag = "ReformCaravan";
					reformCaravan.action = delegate
					{
						Find.WindowStack.Add(new Dialog_FormCaravan(mapParent.Map, true, null, true, false));
					};
					if (GenHostility.AnyHostileActiveThreatToPlayer(mapParent.Map))
					{
						reformCaravan.Disable("CommandReformCaravanFailHostilePawns".Translate());
					}
					yield return reformCaravan;
				}
			}
		}
	}
}
