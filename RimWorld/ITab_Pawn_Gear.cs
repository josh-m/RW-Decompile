using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class ITab_Pawn_Gear : ITab
	{
		private const float TopPadding = 20f;

		private const float ThingIconSize = 28f;

		private const float ThingRowHeight = 28f;

		private const float ThingLeftX = 36f;

		private const float StandardLineHeight = 22f;

		private Vector2 scrollPosition = Vector2.zero;

		private float scrollViewHeight;

		private static readonly Color ThingLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);

		private static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);

		private static List<Thing> workingInvList = new List<Thing>();

		public override bool IsVisible
		{
			get
			{
				return this.SelPawnForGear.RaceProps.ToolUser || this.SelPawnForGear.inventory.container.Any<Thing>();
			}
		}

		private bool CanControl
		{
			get
			{
				return this.SelPawnForGear.IsColonistPlayerControlled;
			}
		}

		private Pawn SelPawnForGear
		{
			get
			{
				if (base.SelPawn != null)
				{
					return base.SelPawn;
				}
				Corpse corpse = base.SelThing as Corpse;
				if (corpse != null)
				{
					return corpse.innerPawn;
				}
				throw new InvalidOperationException("Gear tab on non-pawn non-corpse " + base.SelThing);
			}
		}

		public ITab_Pawn_Gear()
		{
			this.size = new Vector2(440f, 450f);
			this.labelKey = "TabGear";
			this.tutorTag = "Gear";
		}

		protected override void FillTab()
		{
			Text.Font = GameFont.Small;
			Rect rect = new Rect(0f, 20f, this.size.x, this.size.y - 20f);
			Rect rect2 = rect.ContractedBy(10f);
			Rect position = new Rect(rect2.x, rect2.y, rect2.width, rect2.height);
			GUI.BeginGroup(position);
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			Rect outRect = new Rect(0f, 0f, position.width, position.height);
			Rect viewRect = new Rect(0f, 0f, position.width - 16f, this.scrollViewHeight);
			Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect);
			float num = 0f;
			this.TryDrawComfyTemperatureRange(ref num, viewRect.width);
			if (this.SelPawnForGear.apparel != null)
			{
				bool flag = false;
				this.TryDrawAverageArmor(ref num, viewRect.width, StatDefOf.ArmorRating_Blunt, "ArmorBlunt".Translate(), ref flag);
				this.TryDrawAverageArmor(ref num, viewRect.width, StatDefOf.ArmorRating_Sharp, "ArmorSharp".Translate(), ref flag);
				this.TryDrawAverageArmor(ref num, viewRect.width, StatDefOf.ArmorRating_Heat, "ArmorHeat".Translate(), ref flag);
				this.TryDrawAverageArmor(ref num, viewRect.width, StatDefOf.ArmorRating_Electric, "ArmorElectric".Translate(), ref flag);
			}
			if (this.SelPawnForGear.equipment != null)
			{
				Widgets.ListSeparator(ref num, viewRect.width, "Equipment".Translate());
				foreach (ThingWithComps current in this.SelPawnForGear.equipment.AllEquipment)
				{
					this.DrawThingRow(ref num, viewRect.width, current);
				}
			}
			if (this.SelPawnForGear.apparel != null)
			{
				Widgets.ListSeparator(ref num, viewRect.width, "Apparel".Translate());
				foreach (Apparel current2 in from ap in this.SelPawnForGear.apparel.WornApparel
				orderby ap.def.apparel.bodyPartGroups[0].listOrder descending
				select ap)
				{
					this.DrawThingRow(ref num, viewRect.width, current2);
				}
			}
			if (this.SelPawnForGear.inventory != null)
			{
				Widgets.ListSeparator(ref num, viewRect.width, "Inventory".Translate());
				ITab_Pawn_Gear.workingInvList.Clear();
				ITab_Pawn_Gear.workingInvList.AddRange(this.SelPawnForGear.inventory.container);
				for (int i = 0; i < ITab_Pawn_Gear.workingInvList.Count; i++)
				{
					this.DrawThingRow(ref num, viewRect.width, ITab_Pawn_Gear.workingInvList[i]);
				}
			}
			if (Event.current.type == EventType.Layout)
			{
				this.scrollViewHeight = num + 30f;
			}
			Widgets.EndScrollView();
			GUI.EndGroup();
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void DrawThingRow(ref float y, float width, Thing thing)
		{
			Rect rect = new Rect(0f, y, width, 28f);
			Widgets.InfoCardButton(rect.width - 24f, y, thing);
			rect.width -= 24f;
			if (this.CanControl)
			{
				Rect rect2 = new Rect(rect.width - 24f, y, 24f, 24f);
				TooltipHandler.TipRegion(rect2, "DropThing".Translate());
				if (Widgets.ButtonImage(rect2, TexButton.Drop))
				{
					SoundDefOf.TickHigh.PlayOneShotOnCamera();
					this.InterfaceDrop(thing);
				}
				rect.width -= 24f;
			}
			if (Mouse.IsOver(rect))
			{
				GUI.color = ITab_Pawn_Gear.HighlightColor;
				GUI.DrawTexture(rect, TexUI.HighlightTex);
			}
			if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
			{
				Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), thing);
			}
			Text.Anchor = TextAnchor.MiddleLeft;
			GUI.color = ITab_Pawn_Gear.ThingLabelColor;
			Rect rect3 = new Rect(36f, y, width - 36f, 28f);
			string text = thing.LabelCap;
			if (thing is Apparel && this.SelPawnForGear.outfits != null && this.SelPawnForGear.outfits.forcedHandler.IsForced((Apparel)thing))
			{
				text = text + ", " + "ApparelForcedLower".Translate();
			}
			Widgets.Label(rect3, text);
			y += 28f;
		}

		private void TryDrawAverageArmor(ref float curY, float width, StatDef stat, string label, ref bool separatorDrawn)
		{
			if (this.SelPawnForGear.RaceProps.body != BodyDefOf.Human)
			{
				return;
			}
			float num = 0f;
			List<Apparel> wornApparel = this.SelPawnForGear.apparel.WornApparel;
			for (int i = 0; i < wornApparel.Count; i++)
			{
				num += Mathf.Clamp01(wornApparel[i].GetStatValue(stat, true)) * wornApparel[i].def.apparel.HumanBodyCoverage;
			}
			num = Mathf.Clamp01(num);
			if (num > 0.005f)
			{
				if (!separatorDrawn)
				{
					separatorDrawn = true;
					Widgets.ListSeparator(ref curY, width, "AverageArmor".Translate());
				}
				Rect rect = new Rect(0f, curY, width, curY + 22f);
				Widgets.Label(rect, label);
				rect.xMin += 100f;
				Widgets.Label(rect, num.ToStringPercent());
				curY += 22f;
			}
		}

		private void TryDrawComfyTemperatureRange(ref float curY, float width)
		{
			if (this.SelPawnForGear.Dead)
			{
				return;
			}
			Rect rect = new Rect(0f, curY, width, 22f);
			float statValue = this.SelPawnForGear.GetStatValue(StatDefOf.ComfyTemperatureMin, true);
			float statValue2 = this.SelPawnForGear.GetStatValue(StatDefOf.ComfyTemperatureMax, true);
			Widgets.Label(rect, string.Concat(new string[]
			{
				"ComfyTemperatureRange".Translate(),
				": ",
				statValue.ToStringTemperature("F0"),
				" ~ ",
				statValue2.ToStringTemperature("F0")
			}));
			curY += 22f;
		}

		private void InterfaceDrop(Thing t)
		{
			ThingWithComps thingWithComps = t as ThingWithComps;
			Apparel apparel = t as Apparel;
			if (apparel != null)
			{
				Pawn selPawnForGear = this.SelPawnForGear;
				if (selPawnForGear.drafter.CanTakeOrderedJob())
				{
					Job job = new Job(JobDefOf.RemoveApparel, apparel);
					job.playerForced = true;
					selPawnForGear.drafter.TakeOrderedJob(job);
				}
			}
			else if (thingWithComps != null && this.SelPawnForGear.equipment.AllEquipment.Contains(thingWithComps))
			{
				ThingWithComps thingWithComps2;
				this.SelPawnForGear.equipment.TryDropEquipment(thingWithComps, out thingWithComps2, this.SelPawnForGear.Position, true);
			}
			else if (!t.def.destroyOnDrop)
			{
				Thing thing;
				this.SelPawnForGear.inventory.container.TryDrop(t, this.SelPawnForGear.Position, ThingPlaceMode.Near, out thing, null);
			}
		}
	}
}
