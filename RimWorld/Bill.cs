using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public abstract class Bill : IExposable, ILoadReferenceable
	{
		[Unsaved]
		public BillStack billStack;

		private int loadID = -1;

		public RecipeDef recipe;

		public bool suspended;

		public ThingFilter ingredientFilter;

		public float ingredientSearchRadius = 999f;

		public IntRange allowedSkillRange = new IntRange(0, 20);

		public Pawn pawnRestriction;

		public bool deleted;

		public int lastIngredientSearchFailTicks = -99999;

		public const int MaxIngredientSearchRadius = 999;

		public const float ButSize = 24f;

		private const float InterfaceBaseHeight = 53f;

		private const float InterfaceStatusLineHeight = 17f;

		public Map Map
		{
			get
			{
				return this.billStack.billGiver.Map;
			}
		}

		public virtual string Label
		{
			get
			{
				return this.recipe.label;
			}
		}

		public virtual string LabelCap
		{
			get
			{
				return this.Label.CapitalizeFirst();
			}
		}

		public virtual bool CheckIngredientsIfSociallyProper
		{
			get
			{
				return true;
			}
		}

		public virtual bool CompletableEver
		{
			get
			{
				return true;
			}
		}

		protected virtual string StatusString
		{
			get
			{
				return null;
			}
		}

		protected virtual float StatusLineMinHeight
		{
			get
			{
				return 0f;
			}
		}

		protected virtual bool CanCopy
		{
			get
			{
				return true;
			}
		}

		public bool DeletedOrDereferenced
		{
			get
			{
				if (this.deleted)
				{
					return true;
				}
				Thing thing = this.billStack.billGiver as Thing;
				return thing != null && thing.Destroyed;
			}
		}

		public Bill()
		{
		}

		public Bill(RecipeDef recipe)
		{
			this.recipe = recipe;
			this.ingredientFilter = new ThingFilter();
			this.ingredientFilter.CopyAllowancesFrom(recipe.defaultIngredientFilter);
			this.InitializeAfterClone();
		}

		public void InitializeAfterClone()
		{
			this.loadID = Find.UniqueIDsManager.GetNextBillID();
		}

		public virtual void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.loadID, "loadID", 0, false);
			Scribe_Defs.Look<RecipeDef>(ref this.recipe, "recipe");
			Scribe_Values.Look<bool>(ref this.suspended, "suspended", false, false);
			Scribe_Values.Look<float>(ref this.ingredientSearchRadius, "ingredientSearchRadius", 999f, false);
			Scribe_Values.Look<IntRange>(ref this.allowedSkillRange, "allowedSkillRange", default(IntRange), false);
			Scribe_References.Look<Pawn>(ref this.pawnRestriction, "pawnRestriction", false);
			if (Scribe.mode == LoadSaveMode.Saving && this.recipe.fixedIngredientFilter != null)
			{
				foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
				{
					if (!this.recipe.fixedIngredientFilter.Allows(current))
					{
						this.ingredientFilter.SetAllow(current, false);
					}
				}
			}
			Scribe_Deep.Look<ThingFilter>(ref this.ingredientFilter, "ingredientFilter", new object[0]);
		}

		public virtual bool PawnAllowedToStartAnew(Pawn p)
		{
			if (this.pawnRestriction != null)
			{
				return this.pawnRestriction == p;
			}
			if (this.recipe.workSkill != null)
			{
				int level = p.skills.GetSkill(this.recipe.workSkill).Level;
				if (level < this.allowedSkillRange.min)
				{
					JobFailReason.Is("UnderAllowedSkill".Translate(this.allowedSkillRange.min), this.Label);
					return false;
				}
				if (level > this.allowedSkillRange.max)
				{
					JobFailReason.Is("AboveAllowedSkill".Translate(this.allowedSkillRange.max), this.Label);
					return false;
				}
			}
			return true;
		}

		public virtual void Notify_PawnDidWork(Pawn p)
		{
		}

		public virtual void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
		{
		}

		public abstract bool ShouldDoNow();

		public virtual void Notify_DoBillStarted(Pawn billDoer)
		{
		}

		protected virtual void DoConfigInterface(Rect rect, Color baseColor)
		{
			rect.yMin += 29f;
			float y = rect.center.y;
			float num = rect.xMax - (rect.yMax - y);
			Widgets.InfoCardButton(num - 12f, y - 12f, this.recipe);
		}

		public virtual void DoStatusLineInterface(Rect rect)
		{
		}

		public Rect DoInterface(float x, float y, float width, int index)
		{
			Rect rect = new Rect(x, y, width, 53f);
			float num = 0f;
			if (!this.StatusString.NullOrEmpty())
			{
				num = Mathf.Max(17f, this.StatusLineMinHeight);
			}
			rect.height += num;
			Color white = Color.white;
			if (!this.ShouldDoNow())
			{
				white = new Color(1f, 0.7f, 0.7f, 0.7f);
			}
			GUI.color = white;
			Text.Font = GameFont.Small;
			if (index % 2 == 0)
			{
				Widgets.DrawAltRect(rect);
			}
			GUI.BeginGroup(rect);
			Rect rect2 = new Rect(0f, 0f, 24f, 24f);
			if (this.billStack.IndexOf(this) > 0)
			{
				if (Widgets.ButtonImage(rect2, TexButton.ReorderUp, white))
				{
					this.billStack.Reorder(this, -1);
					SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
				}
				TooltipHandler.TipRegion(rect2, "ReorderBillUpTip".Translate());
			}
			if (this.billStack.IndexOf(this) < this.billStack.Count - 1)
			{
				Rect rect3 = new Rect(0f, 24f, 24f, 24f);
				if (Widgets.ButtonImage(rect3, TexButton.ReorderDown, white))
				{
					this.billStack.Reorder(this, 1);
					SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
				}
				TooltipHandler.TipRegion(rect3, "ReorderBillDownTip".Translate());
			}
			Rect rect4 = new Rect(28f, 0f, rect.width - 48f - 20f, rect.height + 5f);
			Widgets.Label(rect4, this.LabelCap);
			this.DoConfigInterface(rect.AtZero(), white);
			Rect rect5 = new Rect(rect.width - 24f, 0f, 24f, 24f);
			if (Widgets.ButtonImage(rect5, TexButton.DeleteX, white, white * GenUI.SubtleMouseoverColor))
			{
				this.billStack.Delete(this);
				SoundDefOf.Click.PlayOneShotOnCamera(null);
			}
			TooltipHandler.TipRegion(rect5, "DeleteBillTip".Translate());
			Rect rect7;
			if (this.CanCopy)
			{
				Rect rect6 = new Rect(rect5);
				rect6.x -= rect6.width + 4f;
				if (Widgets.ButtonImageFitted(rect6, TexButton.Copy, white))
				{
					BillUtility.Clipboard = this.Clone();
					SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
				}
				TooltipHandler.TipRegion(rect6, "CopyBillTip".Translate());
				rect7 = new Rect(rect6);
			}
			else
			{
				rect7 = new Rect(rect5);
			}
			rect7.x -= rect7.width + 4f;
			if (Widgets.ButtonImage(rect7, TexButton.Suspend, white))
			{
				this.suspended = !this.suspended;
				SoundDefOf.Click.PlayOneShotOnCamera(null);
			}
			TooltipHandler.TipRegion(rect7, "SuspendBillTip".Translate());
			if (!this.StatusString.NullOrEmpty())
			{
				Text.Font = GameFont.Tiny;
				Rect rect8 = new Rect(24f, rect.height - num, rect.width - 24f, num);
				Widgets.Label(rect8, this.StatusString);
				this.DoStatusLineInterface(rect8);
			}
			GUI.EndGroup();
			if (this.suspended)
			{
				Text.Font = GameFont.Medium;
				Text.Anchor = TextAnchor.MiddleCenter;
				Rect rect9 = new Rect(rect.x + rect.width / 2f - 70f, rect.y + rect.height / 2f - 20f, 140f, 40f);
				GUI.DrawTexture(rect9, TexUI.GrayTextBG);
				Widgets.Label(rect9, "SuspendedCaps".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				Text.Font = GameFont.Small;
			}
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			return rect;
		}

		public bool IsFixedOrAllowedIngredient(Thing thing)
		{
			for (int i = 0; i < this.recipe.ingredients.Count; i++)
			{
				IngredientCount ingredientCount = this.recipe.ingredients[i];
				if (ingredientCount.IsFixedIngredient && ingredientCount.filter.Allows(thing))
				{
					return true;
				}
			}
			return this.recipe.fixedIngredientFilter.Allows(thing) && this.ingredientFilter.Allows(thing);
		}

		public bool IsFixedOrAllowedIngredient(ThingDef def)
		{
			for (int i = 0; i < this.recipe.ingredients.Count; i++)
			{
				IngredientCount ingredientCount = this.recipe.ingredients[i];
				if (ingredientCount.IsFixedIngredient && ingredientCount.filter.Allows(def))
				{
					return true;
				}
			}
			return this.recipe.fixedIngredientFilter.Allows(def) && this.ingredientFilter.Allows(def);
		}

		public static void CreateNoPawnsWithSkillDialog(RecipeDef recipe)
		{
			string text = "RecipeRequiresSkills".Translate(recipe.LabelCap);
			text += "\n\n";
			text += recipe.MinSkillString;
			Find.WindowStack.Add(new Dialog_MessageBox(text, null, null, null, null, null, false, null, null));
		}

		public virtual BillStoreModeDef GetStoreMode()
		{
			return BillStoreModeDefOf.BestStockpile;
		}

		public virtual Zone_Stockpile GetStoreZone()
		{
			return null;
		}

		public virtual void SetStoreMode(BillStoreModeDef mode, Zone_Stockpile zone = null)
		{
			Log.ErrorOnce("Tried to set store mode of a non-production bill", 27190980, false);
		}

		public virtual Bill Clone()
		{
			Bill bill = (Bill)Activator.CreateInstance(base.GetType());
			bill.recipe = this.recipe;
			bill.suspended = this.suspended;
			bill.ingredientFilter = new ThingFilter();
			bill.ingredientFilter.CopyAllowancesFrom(this.ingredientFilter);
			bill.ingredientSearchRadius = this.ingredientSearchRadius;
			bill.allowedSkillRange = this.allowedSkillRange;
			bill.pawnRestriction = this.pawnRestriction;
			return bill;
		}

		public virtual void ValidateSettings()
		{
			if (this.pawnRestriction != null && (this.pawnRestriction.Dead || this.pawnRestriction.Faction != Faction.OfPlayer || this.pawnRestriction.IsKidnapped()))
			{
				if (this != BillUtility.Clipboard)
				{
					Messages.Message("MessageBillValidationPawnUnavailable".Translate(this.pawnRestriction.LabelShortCap, this.LabelCap, this.billStack.billGiver.LabelShort.CapitalizeFirst()), this.billStack.billGiver as Thing, MessageTypeDefOf.NegativeEvent, true);
				}
				this.pawnRestriction = null;
				return;
			}
		}

		public string GetUniqueLoadID()
		{
			return string.Concat(new object[]
			{
				"Bill_",
				this.recipe.defName,
				"_",
				this.loadID
			});
		}

		public override string ToString()
		{
			return this.GetUniqueLoadID();
		}
	}
}
