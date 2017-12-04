using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class TransferableOneWayWidget
	{
		private struct Section
		{
			public string title;

			public IEnumerable<TransferableOneWay> transferables;

			public List<TransferableOneWay> cachedTransferables;
		}

		private List<TransferableOneWayWidget.Section> sections = new List<TransferableOneWayWidget.Section>();

		private string sourceLabel;

		private string destinationLabel;

		private string sourceCountDesc;

		private bool drawMass;

		private IgnorePawnsInventoryMode ignorePawnInventoryMass = IgnorePawnsInventoryMode.DontIgnore;

		private bool includePawnsMassInMassUsage;

		private Func<float> availableMassGetter;

		private float extraHeaderSpace;

		private bool ignoreCorpseGearAndInventoryMass;

		private bool drawMarketValue;

		private int drawDaysUntilRotForTile;

		private bool transferablesCached;

		private Vector2 scrollPosition;

		private TransferableSorterDef sorter1;

		private TransferableSorterDef sorter2;

		private static List<TransferableCountToTransferStoppingPoint> stoppingPoints = new List<TransferableCountToTransferStoppingPoint>();

		private const float TopAreaHeight = 55f;

		protected readonly Vector2 AcceptButtonSize = new Vector2(160f, 40f);

		protected readonly Vector2 OtherBottomButtonSize = new Vector2(160f, 40f);

		private const float ColumnWidth = 120f;

		private const float FirstTransferableY = 6f;

		private const float RowInterval = 30f;

		public const float CountColumnWidth = 75f;

		public const float AdjustColumnWidth = 240f;

		public const float MassColumnWidth = 100f;

		public static readonly Color ItemMassColor = new Color(0.7f, 0.7f, 0.7f);

		private const float MarketValueColumnWidth = 100f;

		private const float ExtraSpaceAfterSectionTitle = 5f;

		private const float DaysUntilRotColumnWidth = 75f;

		public const float TopAreaWidth = 515f;

		public float TotalNumbersColumnsWidths
		{
			get
			{
				float num = 315f;
				if (this.drawMass)
				{
					num += 100f;
				}
				if (this.drawMarketValue)
				{
					num += 100f;
				}
				if (this.drawDaysUntilRotForTile >= 0)
				{
					num += 75f;
				}
				return num;
			}
		}

		private bool AnyTransferable
		{
			get
			{
				if (!this.transferablesCached)
				{
					this.CacheTransferables();
				}
				for (int i = 0; i < this.sections.Count; i++)
				{
					if (this.sections[i].cachedTransferables.Any<TransferableOneWay>())
					{
						return true;
					}
				}
				return false;
			}
		}

		public TransferableOneWayWidget(IEnumerable<TransferableOneWay> transferables, string sourceLabel, string destinationLabel, string sourceCountDesc, bool drawMass = false, IgnorePawnsInventoryMode ignorePawnInventoryMass = IgnorePawnsInventoryMode.DontIgnore, bool includePawnsMassInMassUsage = false, Func<float> availableMassGetter = null, float extraHeaderSpace = 0f, bool ignoreCorpseGearAndInventoryMass = false, bool drawMarketValue = false, int drawDaysUntilRotForTile = -1)
		{
			if (transferables != null)
			{
				this.AddSection(null, transferables);
			}
			this.sourceLabel = sourceLabel;
			this.destinationLabel = destinationLabel;
			this.sourceCountDesc = sourceCountDesc;
			this.drawMass = drawMass;
			this.ignorePawnInventoryMass = ignorePawnInventoryMass;
			this.includePawnsMassInMassUsage = includePawnsMassInMassUsage;
			this.availableMassGetter = availableMassGetter;
			this.extraHeaderSpace = extraHeaderSpace;
			this.ignoreCorpseGearAndInventoryMass = ignoreCorpseGearAndInventoryMass;
			this.drawMarketValue = drawMarketValue;
			this.drawDaysUntilRotForTile = drawDaysUntilRotForTile;
			this.sorter1 = TransferableSorterDefOf.Category;
			this.sorter2 = TransferableSorterDefOf.MarketValue;
		}

		public void AddSection(string title, IEnumerable<TransferableOneWay> transferables)
		{
			TransferableOneWayWidget.Section item = default(TransferableOneWayWidget.Section);
			item.title = title;
			item.transferables = transferables;
			item.cachedTransferables = new List<TransferableOneWay>();
			this.sections.Add(item);
			this.transferablesCached = false;
		}

		private void CacheTransferables()
		{
			this.transferablesCached = true;
			for (int i = 0; i < this.sections.Count; i++)
			{
				List<TransferableOneWay> cachedTransferables = this.sections[i].cachedTransferables;
				cachedTransferables.Clear();
				cachedTransferables.AddRange(this.sections[i].transferables.OrderBy((TransferableOneWay tr) => tr, this.sorter1.Comparer).ThenBy((TransferableOneWay tr) => tr, this.sorter2.Comparer).ThenBy(new Func<TransferableOneWay, float>(TransferableUIUtility.DefaultListOrderPriority)).ToList<TransferableOneWay>());
			}
		}

		public void OnGUI(Rect inRect)
		{
			bool flag;
			this.OnGUI(inRect, out flag);
		}

		public void OnGUI(Rect inRect, out bool anythingChanged)
		{
			if (!this.transferablesCached)
			{
				this.CacheTransferables();
			}
			TransferableUIUtility.DoTransferableSorters(this.sorter1, this.sorter2, delegate(TransferableSorterDef x)
			{
				this.sorter1 = x;
				this.CacheTransferables();
			}, delegate(TransferableSorterDef x)
			{
				this.sorter2 = x;
				this.CacheTransferables();
			});
			float num = inRect.width - 515f;
			Rect position = new Rect(inRect.x + num, inRect.y, inRect.width - num, 55f);
			GUI.BeginGroup(position);
			Text.Font = GameFont.Medium;
			Rect rect = new Rect(0f, 0f, position.width / 2f, position.height);
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.Label(rect, this.sourceLabel);
			Rect rect2 = new Rect(position.width / 2f, 0f, position.width / 2f, position.height);
			Text.Anchor = TextAnchor.UpperRight;
			Widgets.Label(rect2, this.destinationLabel);
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.EndGroup();
			Rect mainRect = new Rect(inRect.x, inRect.y + 55f + this.extraHeaderSpace, inRect.width, inRect.height - 55f - this.extraHeaderSpace);
			this.FillMainRect(mainRect, out anythingChanged);
		}

		private void FillMainRect(Rect mainRect, out bool anythingChanged)
		{
			anythingChanged = false;
			Text.Font = GameFont.Small;
			if (this.AnyTransferable)
			{
				float num = 6f;
				for (int i = 0; i < this.sections.Count; i++)
				{
					num += (float)this.sections[i].cachedTransferables.Count * 30f;
					if (this.sections[i].title != null)
					{
						num += 30f;
					}
				}
				float num2 = 6f;
				float availableMass = (this.availableMassGetter == null) ? 3.40282347E+38f : this.availableMassGetter();
				Rect viewRect = new Rect(0f, 0f, mainRect.width - 16f, num);
				Widgets.BeginScrollView(mainRect, ref this.scrollPosition, viewRect, true);
				float num3 = this.scrollPosition.y - 30f;
				float num4 = this.scrollPosition.y + mainRect.height;
				for (int j = 0; j < this.sections.Count; j++)
				{
					List<TransferableOneWay> cachedTransferables = this.sections[j].cachedTransferables;
					if (cachedTransferables.Any<TransferableOneWay>())
					{
						if (this.sections[j].title != null)
						{
							Widgets.ListSeparator(ref num2, viewRect.width, this.sections[j].title);
							num2 += 5f;
						}
						for (int k = 0; k < cachedTransferables.Count; k++)
						{
							if (num2 > num3 && num2 < num4)
							{
								Rect rect = new Rect(0f, num2, viewRect.width, 30f);
								int countToTransfer = cachedTransferables[k].CountToTransfer;
								this.DoRow(rect, cachedTransferables[k], k, availableMass);
								if (countToTransfer != cachedTransferables[k].CountToTransfer)
								{
									anythingChanged = true;
								}
							}
							num2 += 30f;
						}
					}
				}
				Widgets.EndScrollView();
			}
			else
			{
				GUI.color = Color.gray;
				Text.Anchor = TextAnchor.UpperCenter;
				Widgets.Label(mainRect, "NoneBrackets".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
			}
		}

		private void DoRow(Rect rect, TransferableOneWay trad, int index, float availableMass)
		{
			if (index % 2 == 1)
			{
				GUI.DrawTexture(rect, TradeUI.TradeAlternativeBGTex);
			}
			Text.Font = GameFont.Small;
			GUI.BeginGroup(rect);
			float num = rect.width;
			int maxCount = trad.MaxCount;
			Rect rect2 = new Rect(num - 240f, 0f, 240f, rect.height);
			TransferableOneWayWidget.stoppingPoints.Clear();
			if (this.availableMassGetter != null && (!(trad.AnyThing is Pawn) || this.includePawnsMassInMassUsage))
			{
				float num2 = availableMass + this.GetMass(trad.AnyThing) * (float)trad.CountToTransfer;
				int threshold = (num2 > 0f) ? Mathf.FloorToInt(num2 / this.GetMass(trad.AnyThing)) : 0;
				TransferableOneWayWidget.stoppingPoints.Add(new TransferableCountToTransferStoppingPoint(threshold, "M<", ">M"));
			}
			Rect rect3 = rect2;
			int min = 0;
			int max = maxCount;
			List<TransferableCountToTransferStoppingPoint> extraStoppingPoints = TransferableOneWayWidget.stoppingPoints;
			TransferableUIUtility.DoCountAdjustInterface(rect3, trad, index, min, max, false, extraStoppingPoints);
			num -= 240f;
			if (this.drawMarketValue)
			{
				Rect rect4 = new Rect(num - 100f, 0f, 100f, rect.height);
				Text.Anchor = TextAnchor.MiddleLeft;
				this.DrawMarketValue(rect4, trad);
				num -= 100f;
			}
			if (this.drawMass)
			{
				Rect rect5 = new Rect(num - 100f, 0f, 100f, rect.height);
				Text.Anchor = TextAnchor.MiddleLeft;
				this.DrawMass(rect5, trad, availableMass);
				num -= 100f;
			}
			if (this.drawDaysUntilRotForTile >= 0)
			{
				Rect rect6 = new Rect(num - 75f, 0f, 75f, rect.height);
				Text.Anchor = TextAnchor.MiddleLeft;
				this.DrawDaysUntilRot(rect6, trad);
				num -= 75f;
			}
			Rect rect7 = new Rect(num - 75f, 0f, 75f, rect.height);
			if (Mouse.IsOver(rect7))
			{
				Widgets.DrawHighlight(rect7);
			}
			Text.Anchor = TextAnchor.MiddleLeft;
			Rect rect8 = rect7;
			rect8.xMin += 5f;
			rect8.xMax -= 5f;
			Widgets.Label(rect8, maxCount.ToStringCached());
			TooltipHandler.TipRegion(rect7, this.sourceCountDesc);
			num -= 75f;
			Rect idRect = new Rect(0f, 0f, num, rect.height);
			TransferableUIUtility.DrawTransferableInfo(trad, idRect, Color.white);
			GenUI.ResetLabelAlign();
			GUI.EndGroup();
		}

		private void DrawDaysUntilRot(Rect rect, TransferableOneWay trad)
		{
			if (!trad.HasAnyThing)
			{
				return;
			}
			if (!trad.ThingDef.IsNutritionGivingIngestible)
			{
				return;
			}
			int num = 2147483647;
			for (int i = 0; i < trad.things.Count; i++)
			{
				CompRottable compRottable = trad.things[i].TryGetComp<CompRottable>();
				if (compRottable != null)
				{
					num = Mathf.Min(num, compRottable.ApproxTicksUntilRotWhenAtTempOfTile(this.drawDaysUntilRotForTile));
				}
			}
			if (num >= 36000000)
			{
				return;
			}
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			float num2 = (float)num / 60000f;
			GUI.color = Color.yellow;
			Widgets.Label(rect, num2.ToString("0.#"));
			GUI.color = Color.white;
			TooltipHandler.TipRegion(rect, "DaysUntilRotTip".Translate());
		}

		private void DrawMarketValue(Rect rect, TransferableOneWay trad)
		{
			if (!trad.HasAnyThing)
			{
				return;
			}
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			Widgets.Label(rect, trad.AnyThing.MarketValue.ToStringMoney());
			TooltipHandler.TipRegion(rect, "MarketValueTip".Translate());
		}

		private void DrawMass(Rect rect, TransferableOneWay trad, float availableMass)
		{
			if (!trad.HasAnyThing)
			{
				return;
			}
			Thing anyThing = trad.AnyThing;
			Pawn pawn = anyThing as Pawn;
			if (pawn != null && !this.includePawnsMassInMassUsage && !MassUtility.CanEverCarryAnything(pawn))
			{
				return;
			}
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			if (pawn == null || this.includePawnsMassInMassUsage)
			{
				float mass = this.GetMass(anyThing);
				if (pawn != null)
				{
					float gearMass = 0f;
					float invMass = 0f;
					gearMass = MassUtility.GearMass(pawn);
					if (!InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, this.ignorePawnInventoryMass))
					{
						invMass = MassUtility.InventoryMass(pawn);
					}
					TooltipHandler.TipRegion(rect, () => this.GetPawnMassTip(trad, 0f, mass - gearMass - invMass, gearMass, invMass), trad.GetHashCode() * 59);
				}
				else
				{
					TooltipHandler.TipRegion(rect, "ItemWeightTip".Translate());
				}
				if (mass > availableMass)
				{
					GUI.color = Color.red;
				}
				else
				{
					GUI.color = TransferableOneWayWidget.ItemMassColor;
				}
				Widgets.Label(rect, mass.ToStringMass());
			}
			else
			{
				float cap = MassUtility.Capacity(pawn);
				float gearMass = MassUtility.GearMass(pawn);
				float invMass = (!InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, this.ignorePawnInventoryMass)) ? MassUtility.InventoryMass(pawn) : 0f;
				float num = cap - gearMass - invMass;
				if (num > 0f)
				{
					GUI.color = Color.green;
				}
				else if (num < 0f)
				{
					GUI.color = Color.red;
				}
				else
				{
					GUI.color = Color.gray;
				}
				Widgets.Label(rect, num.ToStringMassOffset());
				TooltipHandler.TipRegion(rect, () => this.GetPawnMassTip(trad, cap, 0f, gearMass, invMass), trad.GetHashCode() * 59);
			}
			GUI.color = Color.white;
		}

		private string GetPawnMassTip(TransferableOneWay trad, float capacity, float pawnMass, float gearMass, float invMass)
		{
			if (!trad.HasAnyThing)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (capacity != 0f)
			{
				stringBuilder.Append("MassCapacity".Translate() + ": " + capacity.ToStringMass());
			}
			else
			{
				stringBuilder.Append("Mass".Translate() + ": " + pawnMass.ToStringMass());
			}
			if (gearMass != 0f)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("EquipmentAndApparelMass".Translate() + ": " + gearMass.ToStringMass());
			}
			if (invMass != 0f)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("InventoryMass".Translate() + ": " + invMass.ToStringMass());
			}
			return stringBuilder.ToString();
		}

		private float GetMass(Thing thing)
		{
			if (thing == null)
			{
				return 0f;
			}
			float num = thing.GetStatValue(StatDefOf.Mass, true);
			Pawn pawn = thing as Pawn;
			if (pawn != null)
			{
				if (InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, this.ignorePawnInventoryMass))
				{
					num -= MassUtility.InventoryMass(pawn);
				}
			}
			else if (this.ignoreCorpseGearAndInventoryMass)
			{
				Corpse corpse = thing as Corpse;
				if (corpse != null)
				{
					num -= MassUtility.GearAndInventoryMass(corpse.InnerPawn);
				}
			}
			return num;
		}
	}
}
