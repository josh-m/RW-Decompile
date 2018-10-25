using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_Build : Designator_Place
	{
		protected BuildableDef entDef;

		private ThingDef stuffDef;

		private bool writeStuff;

		private static readonly Vector2 TerrainTextureCroppedSize = new Vector2(64f, 64f);

		private static readonly Vector2 DragPriceDrawOffset = new Vector2(19f, 17f);

		private const float DragPriceDrawNumberX = 29f;

		public override BuildableDef PlacingDef
		{
			get
			{
				return this.entDef;
			}
		}

		public override string Label
		{
			get
			{
				ThingDef thingDef = this.entDef as ThingDef;
				if (thingDef != null && this.writeStuff)
				{
					return GenLabel.ThingLabel(thingDef, this.stuffDef, 1);
				}
				if (thingDef != null && thingDef.MadeFromStuff)
				{
					return this.entDef.label + "...";
				}
				return this.entDef.label;
			}
		}

		public override string Desc
		{
			get
			{
				return this.entDef.description;
			}
		}

		public override Color IconDrawColor
		{
			get
			{
				if (this.stuffDef != null)
				{
					return this.stuffDef.stuffProps.color;
				}
				return this.entDef.uiIconColor;
			}
		}

		public override bool Visible
		{
			get
			{
				if (DebugSettings.godMode)
				{
					return true;
				}
				if (this.entDef.minTechLevelToBuild != TechLevel.Undefined && Faction.OfPlayer.def.techLevel < this.entDef.minTechLevelToBuild)
				{
					return false;
				}
				if (this.entDef.maxTechLevelToBuild != TechLevel.Undefined && Faction.OfPlayer.def.techLevel > this.entDef.maxTechLevelToBuild)
				{
					return false;
				}
				if (!this.entDef.IsResearchFinished)
				{
					return false;
				}
				if (this.entDef.buildingPrerequisites != null)
				{
					for (int i = 0; i < this.entDef.buildingPrerequisites.Count; i++)
					{
						if (!base.Map.listerBuildings.ColonistsHaveBuilding(this.entDef.buildingPrerequisites[i]))
						{
							return false;
						}
					}
				}
				return true;
			}
		}

		public override int DraggableDimensions
		{
			get
			{
				return this.entDef.placingDraggableDimensions;
			}
		}

		public override bool DragDrawMeasurements
		{
			get
			{
				return true;
			}
		}

		public override float PanelReadoutTitleExtraRightMargin
		{
			get
			{
				return 20f;
			}
		}

		public override string HighlightTag
		{
			get
			{
				if (this.cachedHighlightTag == null && this.tutorTag != null)
				{
					this.cachedHighlightTag = "Designator-Build-" + this.tutorTag;
				}
				return this.cachedHighlightTag;
			}
		}

		public Designator_Build(BuildableDef entDef)
		{
			this.entDef = entDef;
			this.icon = entDef.uiIcon;
			this.iconAngle = entDef.uiIconAngle;
			this.iconOffset = entDef.uiIconOffset;
			this.hotKey = entDef.designationHotKey;
			this.tutorTag = entDef.defName;
			this.order = 20f;
			ThingDef thingDef = entDef as ThingDef;
			if (thingDef != null)
			{
				this.iconProportions = thingDef.graphicData.drawSize.RotatedBy(thingDef.defaultPlacingRot);
				this.iconDrawScale = GenUI.IconDrawScale(thingDef);
			}
			else
			{
				this.iconProportions = new Vector2(1f, 1f);
				this.iconDrawScale = 1f;
			}
			TerrainDef terrainDef = entDef as TerrainDef;
			if (terrainDef != null)
			{
				this.iconTexCoords = new Rect(0f, 0f, Designator_Build.TerrainTextureCroppedSize.x / (float)this.icon.width, Designator_Build.TerrainTextureCroppedSize.y / (float)this.icon.height);
			}
			this.ResetStuffToDefault();
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
		{
			GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth);
			ThingDef thingDef = this.entDef as ThingDef;
			if (thingDef != null && thingDef.MadeFromStuff)
			{
				Designator_Dropdown.DrawExtraOptionsIcon(topLeft, this.GetWidth(maxWidth));
			}
			return result;
		}

		public void ResetStuffToDefault()
		{
			ThingDef thingDef = this.entDef as ThingDef;
			if (thingDef != null && thingDef.MadeFromStuff)
			{
				this.stuffDef = GenStuff.DefaultStuffFor(thingDef);
			}
		}

		public override void DrawMouseAttachments()
		{
			base.DrawMouseAttachments();
			if (!ArchitectCategoryTab.InfoRect.Contains(UI.MousePositionOnUIInverted))
			{
				DesignationDragger dragger = Find.DesignatorManager.Dragger;
				int num;
				if (dragger.Dragging)
				{
					num = dragger.DragCells.Count<IntVec3>();
				}
				else
				{
					num = 1;
				}
				float num2 = 0f;
				Vector2 vector = Event.current.mousePosition + Designator_Build.DragPriceDrawOffset;
				List<ThingDefCountClass> list = this.entDef.CostListAdjusted(this.stuffDef, true);
				for (int i = 0; i < list.Count; i++)
				{
					ThingDefCountClass thingDefCountClass = list[i];
					float y = vector.y + num2;
					Rect rect = new Rect(vector.x, y, 27f, 27f);
					Widgets.ThingIcon(rect, thingDefCountClass.thingDef);
					Rect rect2 = new Rect(vector.x + 29f, y, 999f, 29f);
					int num3 = num * thingDefCountClass.count;
					string text = num3.ToString();
					if (base.Map.resourceCounter.GetCount(thingDefCountClass.thingDef) < num3)
					{
						GUI.color = Color.red;
						text = text + " (" + "NotEnoughStoredLower".Translate() + ")";
					}
					Text.Font = GameFont.Small;
					Text.Anchor = TextAnchor.MiddleLeft;
					Widgets.Label(rect2, text);
					Text.Anchor = TextAnchor.UpperLeft;
					GUI.color = Color.white;
					num2 += 29f;
				}
			}
		}

		public override void ProcessInput(Event ev)
		{
			if (!base.CheckCanInteract())
			{
				return;
			}
			ThingDef thingDef = this.entDef as ThingDef;
			if (thingDef == null || !thingDef.MadeFromStuff)
			{
				base.ProcessInput(ev);
			}
			else
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (ThingDef current in base.Map.resourceCounter.AllCountedAmounts.Keys)
				{
					if (current.IsStuff && current.stuffProps.CanMake(thingDef) && (DebugSettings.godMode || base.Map.listerThings.ThingsOfDef(current).Count > 0))
					{
						ThingDef localStuffDef = current;
						string label = GenLabel.ThingLabel(this.entDef, localStuffDef, 1).CapitalizeFirst();
						list.Add(new FloatMenuOption(label, delegate
						{
							base.ProcessInput(ev);
							Find.DesignatorManager.Select(this);
							this.stuffDef = localStuffDef;
							this.writeStuff = true;
						}, MenuOptionPriority.Default, null, null, 0f, null, null)
						{
							tutorTag = "SelectStuff-" + thingDef.defName + "-" + localStuffDef.defName
						});
					}
				}
				if (list.Count == 0)
				{
					Messages.Message("NoStuffsToBuildWith".Translate(), MessageTypeDefOf.RejectInput, false);
				}
				else
				{
					FloatMenu floatMenu = new FloatMenu(list);
					floatMenu.vanishIfMouseDistant = true;
					floatMenu.onCloseCallback = delegate
					{
						this.writeStuff = true;
					};
					Find.WindowStack.Add(floatMenu);
					Find.DesignatorManager.Select(this);
				}
			}
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			return GenConstruct.CanPlaceBlueprintAt(this.entDef, c, this.placingRot, base.Map, DebugSettings.godMode, null);
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			if (TutorSystem.TutorialMode && !TutorSystem.AllowAction(new EventPack(base.TutorTagDesignate, c)))
			{
				return;
			}
			if (DebugSettings.godMode || this.entDef.GetStatValueAbstract(StatDefOf.WorkToBuild, this.stuffDef) == 0f)
			{
				if (this.entDef is TerrainDef)
				{
					base.Map.terrainGrid.SetTerrain(c, (TerrainDef)this.entDef);
				}
				else
				{
					Thing thing = ThingMaker.MakeThing((ThingDef)this.entDef, this.stuffDef);
					thing.SetFactionDirect(Faction.OfPlayer);
					GenSpawn.Spawn(thing, c, base.Map, this.placingRot, WipeMode.Vanish, false);
				}
			}
			else
			{
				GenSpawn.WipeExistingThings(c, this.placingRot, this.entDef.blueprintDef, base.Map, DestroyMode.Deconstruct);
				GenConstruct.PlaceBlueprintForBuild(this.entDef, c, base.Map, this.placingRot, Faction.OfPlayer, this.stuffDef);
			}
			MoteMaker.ThrowMetaPuffs(GenAdj.OccupiedRect(c, this.placingRot, this.entDef.Size), base.Map);
			ThingDef thingDef = this.entDef as ThingDef;
			if (thingDef != null && thingDef.IsOrbitalTradeBeacon)
			{
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.BuildOrbitalTradeBeacon, KnowledgeAmount.Total);
			}
			if (TutorSystem.TutorialMode)
			{
				TutorSystem.Notify_Event(new EventPack(base.TutorTagDesignate, c));
			}
			if (this.entDef.PlaceWorkers != null)
			{
				for (int i = 0; i < this.entDef.PlaceWorkers.Count; i++)
				{
					this.entDef.PlaceWorkers[i].PostPlace(base.Map, this.entDef, c, this.placingRot);
				}
			}
		}

		public override void SelectedUpdate()
		{
			base.SelectedUpdate();
			BuildDesignatorUtility.TryDrawPowerGridAndAnticipatedConnection(this.entDef, this.placingRot);
		}

		public override void DrawPanelReadout(ref float curY, float width)
		{
			if (this.entDef.costStuffCount <= 0 && this.stuffDef != null)
			{
				this.stuffDef = null;
			}
			ThingDef thingDef = this.entDef as ThingDef;
			if (thingDef != null)
			{
				Widgets.InfoCardButton(width - 24f - 2f, 6f, thingDef, this.stuffDef);
			}
			else
			{
				Widgets.InfoCardButton(width - 24f - 2f, 6f, this.entDef);
			}
			Text.Font = GameFont.Small;
			List<ThingDefCountClass> list = this.entDef.CostListAdjusted(this.stuffDef, false);
			for (int i = 0; i < list.Count; i++)
			{
				ThingDefCountClass thingDefCountClass = list[i];
				Color color = GUI.color;
				Widgets.ThingIcon(new Rect(0f, curY, 20f, 20f), thingDefCountClass.thingDef);
				GUI.color = color;
				if (thingDefCountClass.thingDef != null && thingDefCountClass.thingDef.resourceReadoutPriority != ResourceCountPriority.Uncounted && base.Map.resourceCounter.GetCount(thingDefCountClass.thingDef) < thingDefCountClass.count)
				{
					GUI.color = Color.red;
				}
				Widgets.Label(new Rect(26f, curY + 2f, 50f, 100f), thingDefCountClass.count.ToString());
				GUI.color = Color.white;
				string text;
				if (thingDefCountClass.thingDef == null)
				{
					text = "(" + "UnchosenStuff".Translate() + ")";
				}
				else
				{
					text = thingDefCountClass.thingDef.LabelCap;
				}
				float width2 = width - 60f;
				float num = Text.CalcHeight(text, width2) - 5f;
				Widgets.Label(new Rect(60f, curY + 2f, width2, num + 5f), text);
				curY += num;
			}
			if (this.entDef.constructionSkillPrerequisite > 0)
			{
				Rect rect = new Rect(0f, curY + 2f, width, 24f);
				if (!this.AnyColonistWithConstructionSkill(this.entDef.constructionSkillPrerequisite, false))
				{
					GUI.color = Color.red;
					TooltipHandler.TipRegion(rect, "NoColonistWithConstructionSkillTip".Translate(Faction.OfPlayer.def.pawnsPlural));
				}
				else if (!this.AnyColonistWithConstructionSkill(this.entDef.constructionSkillPrerequisite, true))
				{
					GUI.color = Color.yellow;
					TooltipHandler.TipRegion(rect, "AllColonistsWithConstructionSkillHaveDisaledConstructingTip".Translate(Faction.OfPlayer.def.pawnsPlural, WorkTypeDefOf.Construction.gerundLabel));
				}
				else
				{
					GUI.color = new Color(0.72f, 0.87f, 0.72f);
				}
				Widgets.Label(rect, string.Format("{0}: {1}", "ConstructionNeeded".Translate(), this.entDef.constructionSkillPrerequisite));
				GUI.color = Color.white;
				curY += 18f;
			}
			curY += 4f;
		}

		private bool AnyColonistWithConstructionSkill(int skill, bool careIfDisabled)
		{
			foreach (Pawn current in Find.CurrentMap.mapPawns.FreeColonists)
			{
				if (current.skills.GetSkill(SkillDefOf.Construction).Level >= skill && (!careIfDisabled || current.workSettings.WorkIsActive(WorkTypeDefOf.Construction)))
				{
					return true;
				}
			}
			return false;
		}

		public void SetStuffDef(ThingDef stuffDef)
		{
			this.stuffDef = stuffDef;
		}

		public override void RenderHighlight(List<IntVec3> dragCells)
		{
			DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
		}
	}
}
