using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class Command_SetPlantToGrow : Command
	{
		public IPlantToGrowSettable settable;

		private List<IPlantToGrowSettable> settables;

		private static readonly Texture2D SetPlantToGrowTex = ContentFinder<Texture2D>.Get("UI/Commands/SetPlantToGrow", true);

		public Command_SetPlantToGrow()
		{
			this.tutorTag = "GrowingZoneSetPlant";
			ThingDef thingDef = null;
			bool flag = false;
			foreach (object current in Find.Selector.SelectedObjects)
			{
				IPlantToGrowSettable plantToGrowSettable = current as IPlantToGrowSettable;
				if (plantToGrowSettable != null)
				{
					if (thingDef != null && thingDef != plantToGrowSettable.GetPlantDefToGrow())
					{
						flag = true;
						break;
					}
					thingDef = plantToGrowSettable.GetPlantDefToGrow();
				}
			}
			if (flag)
			{
				this.icon = Command_SetPlantToGrow.SetPlantToGrowTex;
				this.defaultLabel = "CommandSelectPlantToGrowMulti".Translate();
			}
			else
			{
				this.icon = thingDef.uiIcon;
				this.defaultLabel = "CommandSelectPlantToGrow".Translate(new object[]
				{
					thingDef.label
				});
			}
		}

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			if (this.settables == null)
			{
				this.settables = new List<IPlantToGrowSettable>();
			}
			if (!this.settables.Contains(this.settable))
			{
				this.settables.Add(this.settable);
			}
			foreach (ThingDef current in GenPlant.ValidPlantTypesForGrowers(this.settables))
			{
				if (this.IsPlantAvailable(current))
				{
					ThingDef localPlantDef = current;
					string text = current.LabelCap;
					if (current.plant.sowMinSkill > 0)
					{
						string text2 = text;
						text = string.Concat(new object[]
						{
							text2,
							" (",
							"MinSkill".Translate(),
							": ",
							current.plant.sowMinSkill,
							")"
						});
					}
					List<FloatMenuOption> arg_121_0 = list;
					Func<Rect, bool> extraPartOnGUI = (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, localPlantDef);
					arg_121_0.Add(new FloatMenuOption(text, delegate
					{
						string s = this.tutorTag + "-" + localPlantDef.defName;
						if (!TutorSystem.AllowAction(s))
						{
							return;
						}
						for (int i = 0; i < this.settables.Count; i++)
						{
							this.settables[i].SetPlantDefToGrow(localPlantDef);
						}
						PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.SetGrowingZonePlant, KnowledgeAmount.Total);
						this.WarnAsAppropriate(localPlantDef);
						TutorSystem.Notify_Event(s);
					}, MenuOptionPriority.Default, null, null, 29f, extraPartOnGUI, null));
				}
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		public override bool InheritInteractionsFrom(Gizmo other)
		{
			if (this.settables == null)
			{
				this.settables = new List<IPlantToGrowSettable>();
			}
			this.settables.Add(((Command_SetPlantToGrow)other).settable);
			return false;
		}

		private void WarnAsAppropriate(ThingDef plantDef)
		{
			if (plantDef.plant.sowMinSkill > 0)
			{
				foreach (Pawn current in this.settable.Map.mapPawns.FreeColonistsSpawned)
				{
					if (current.skills.GetSkill(SkillDefOf.Growing).Level >= plantDef.plant.sowMinSkill && !current.Downed && current.workSettings.WorkIsActive(WorkTypeDefOf.Growing))
					{
						return;
					}
				}
				Find.WindowStack.Add(new Dialog_MessageBox("NoGrowerCanPlant".Translate(new object[]
				{
					plantDef.label,
					plantDef.plant.sowMinSkill
				}).CapitalizeFirst(), null, null, null, null, null, false));
			}
		}

		private bool IsPlantAvailable(ThingDef plantDef)
		{
			List<ResearchProjectDef> sowResearchPrerequisites = plantDef.plant.sowResearchPrerequisites;
			if (sowResearchPrerequisites == null)
			{
				return true;
			}
			for (int i = 0; i < sowResearchPrerequisites.Count; i++)
			{
				if (!sowResearchPrerequisites[i].IsFinished)
				{
					return false;
				}
			}
			return true;
		}
	}
}
