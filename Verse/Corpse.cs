using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Verse
{
	public class Corpse : ThingWithComps, IBillGiver, IThoughtGiver, IStrippable
	{
		private const int VanishAfterTicks = 3000000;

		public Pawn innerPawn;

		private int timeOfDeath = -1000;

		private int vanishAfterTimestamp = -1000;

		private BillStack operationsBillStack;

		public bool everBuriedInSarcophagus;

		public int Age
		{
			get
			{
				return Find.TickManager.TicksGame - this.timeOfDeath;
			}
			set
			{
				this.timeOfDeath = Find.TickManager.TicksGame - value;
			}
		}

		public override string Label
		{
			get
			{
				return "DeadLabel".Translate(new object[]
				{
					this.innerPawn.LabelCap
				});
			}
		}

		public override bool IngestibleNow
		{
			get
			{
				return this.innerPawn.RaceProps.IsFlesh && this.GetRotStage() != RotStage.Dessicated;
			}
		}

		public RotDrawMode CurRotDrawMode
		{
			get
			{
				CompRottable comp = base.GetComp<CompRottable>();
				if (comp != null)
				{
					if (comp.Stage == RotStage.Rotting)
					{
						return RotDrawMode.Rotting;
					}
					if (comp.Stage == RotStage.Dessicated)
					{
						return RotDrawMode.Dessicated;
					}
				}
				return RotDrawMode.Fresh;
			}
		}

		private bool ShouldVanish
		{
			get
			{
				return this.innerPawn.RaceProps.Animal && this.vanishAfterTimestamp > 0 && this.Age >= this.vanishAfterTimestamp && this.holder == null && this.GetRoom().TouchesMapEdge && !Find.RoofGrid.Roofed(base.Position);
			}
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats
		{
			get
			{
				foreach (StatDrawEntry s in base.SpecialDisplayStats)
				{
					yield return s;
				}
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Nutrition".Translate(), FoodUtility.GetBodyPartNutrition(this.innerPawn, this.innerPawn.RaceProps.body.corePart).ToString("0.##"), 0);
				StatDef meatAmount = StatDefOf.MeatAmount;
				yield return new StatDrawEntry(meatAmount.category, meatAmount, this.innerPawn.GetStatValue(meatAmount, true), StatRequest.For(this.innerPawn), ToStringNumberSense.Undefined);
				StatDef leatherAmount = StatDefOf.LeatherAmount;
				yield return new StatDrawEntry(leatherAmount.category, leatherAmount, this.innerPawn.GetStatValue(leatherAmount, true), StatRequest.For(this.innerPawn), ToStringNumberSense.Undefined);
			}
		}

		public BillStack BillStack
		{
			get
			{
				return this.operationsBillStack;
			}
		}

		public IEnumerable<IntVec3> IngredientStackCells
		{
			get
			{
				yield return this.InteractionCell;
			}
		}

		public bool Bugged
		{
			get
			{
				return this.innerPawn == null;
			}
		}

		public Corpse()
		{
			this.operationsBillStack = new BillStack(this);
		}

		public bool CurrentlyUsable()
		{
			return this.InteractionCell.IsValid;
		}

		public bool AnythingToStrip()
		{
			return this.innerPawn.AnythingToStrip();
		}

		public override void SpawnSetup()
		{
			base.SpawnSetup();
			if (this.Bugged)
			{
				Log.Error(this + " spawned in bugged state. Null innerPawn");
				return;
			}
			if (this.timeOfDeath < 0)
			{
				this.timeOfDeath = Find.TickManager.TicksGame;
			}
			this.innerPawn.Rotation = Rot4.South;
			this.NotifyColonistBar();
		}

		public override void DeSpawn()
		{
			base.DeSpawn();
			if (!this.Bugged)
			{
				this.NotifyColonistBar();
			}
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			if (!this.Bugged)
			{
				if (this.innerPawn.ownership != null)
				{
					this.innerPawn.ownership.UnclaimAll();
				}
				if (this.innerPawn.equipment != null)
				{
					this.innerPawn.equipment.DestroyAllEquipment(DestroyMode.Vanish);
				}
				this.innerPawn.inventory.DestroyAll(DestroyMode.Vanish);
				if (this.innerPawn.apparel != null)
				{
					this.innerPawn.apparel.DestroyAll(DestroyMode.Vanish);
				}
				this.innerPawn.corpse = null;
			}
			base.Destroy(mode);
			if (!this.Bugged)
			{
				Find.WorldPawns.DiscardIfUnimportant(this.innerPawn);
				this.NotifyColonistBar();
			}
		}

		public override void TickRare()
		{
			base.TickRare();
			if (this.Bugged)
			{
				Log.Error(this + " has null innerPawn. Destroying.");
				this.Destroy(DestroyMode.Vanish);
				return;
			}
			this.innerPawn.TickRare();
			if (this.vanishAfterTimestamp < 0 || this.GetRotStage() != RotStage.Dessicated)
			{
				this.vanishAfterTimestamp = this.Age + 3000000;
			}
			if (this.ShouldVanish)
			{
				this.Destroy(DestroyMode.Vanish);
			}
		}

		protected override void IngestedCalculateAmounts(Pawn ingester, float nutritionWanted, out int numTaken, out float nutritionIngested)
		{
			BodyPartRecord bodyPartRecord = this.GetBestBodyPartToEat(ingester, nutritionWanted);
			if (bodyPartRecord == null)
			{
				Log.Error(string.Concat(new object[]
				{
					ingester,
					" ate ",
					this,
					" but no body part was found. Replacing with core part."
				}));
				bodyPartRecord = this.innerPawn.RaceProps.body.corePart;
			}
			float bodyPartNutrition = FoodUtility.GetBodyPartNutrition(this.innerPawn, bodyPartRecord);
			if (bodyPartRecord == this.innerPawn.RaceProps.body.corePart)
			{
				if (PawnUtility.ShouldSendNotificationAbout(this.innerPawn) && this.innerPawn.RaceProps.Humanlike)
				{
					Messages.Message("MessageEatenByPredator".Translate(new object[]
					{
						this.innerPawn.LabelShort,
						ingester.LabelIndefinite()
					}).CapitalizeFirst(), ingester, MessageSound.Negative);
				}
				numTaken = 1;
			}
			else
			{
				Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, this.innerPawn, bodyPartRecord);
				hediff_MissingPart.lastInjury = HediffDefOf.Bite;
				hediff_MissingPart.IsFresh = true;
				this.innerPawn.health.AddHediff(hediff_MissingPart, null, null);
				numTaken = 0;
			}
			if (ingester.RaceProps.Humanlike && Rand.Value < 0.05f)
			{
				FoodUtility.AddFoodPoisoningHediff(ingester, this);
			}
			nutritionIngested = bodyPartNutrition;
		}

		[DebuggerHidden]
		public override IEnumerable<Thing> ButcherProducts(Pawn butcher, float efficiency)
		{
			foreach (Thing t in this.innerPawn.ButcherProducts(butcher, efficiency))
			{
				yield return t;
			}
			if (this.innerPawn.RaceProps.BloodDef != null)
			{
				FilthMaker.MakeFilth(butcher.Position, this.innerPawn.RaceProps.BloodDef, this.innerPawn.LabelIndefinite(), 1);
			}
			if (this.innerPawn.RaceProps.Humanlike)
			{
				butcher.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.ButcheredHumanlikeCorpse, null);
				foreach (Pawn p in Find.MapPawns.SpawnedPawnsInFaction(butcher.Faction))
				{
					if (p != butcher && p.needs != null && p.needs.mood != null && p.needs.mood.thoughts != null)
					{
						p.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.KnowButcheredHumanlikeCorpse, null);
					}
				}
				TaleRecorder.RecordTale(TaleDefOf.ButcheredHumanlikeCorpse, new object[]
				{
					butcher
				});
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.timeOfDeath, "timeOfDeath", 0, false);
			Scribe_Values.LookValue<int>(ref this.vanishAfterTimestamp, "vanishAfterTimestamp", 0, false);
			Scribe_Values.LookValue<bool>(ref this.everBuriedInSarcophagus, "everBuriedInSarcophagus", false, false);
			Scribe_Deep.LookDeep<BillStack>(ref this.operationsBillStack, "operationsBillStack", new object[]
			{
				this
			});
			Scribe_References.LookReference<Pawn>(ref this.innerPawn, "innerPawn", true);
		}

		public void Strip()
		{
			if (this.innerPawn.equipment != null)
			{
				this.innerPawn.equipment.DropAllEquipment(base.Position, false);
			}
			if (this.innerPawn.apparel != null)
			{
				this.innerPawn.apparel.DropAll(base.Position, false);
			}
			if (this.innerPawn.inventory != null)
			{
				this.innerPawn.inventory.DropAllNearPawn(base.Position, false);
			}
		}

		public override void DrawAt(Vector3 drawLoc)
		{
			Building building = this.StoringBuilding();
			if (building != null && building.def == ThingDefOf.Grave)
			{
				return;
			}
			this.innerPawn.Drawer.renderer.RenderPawnAt(drawLoc, this.CurRotDrawMode);
		}

		public Thought_Memory GiveObservedThought()
		{
			if (!this.innerPawn.RaceProps.Humanlike)
			{
				return null;
			}
			if (this.StoringBuilding() == null)
			{
				Thought_MemoryObservation thought_MemoryObservation;
				if (this.IsNotFresh())
				{
					thought_MemoryObservation = (Thought_MemoryObservation)ThoughtMaker.MakeThought(ThoughtDefOf.ObservedLayingRottingCorpse);
				}
				else
				{
					thought_MemoryObservation = (Thought_MemoryObservation)ThoughtMaker.MakeThought(ThoughtDefOf.ObservedLayingCorpse);
				}
				thought_MemoryObservation.Target = this;
				return thought_MemoryObservation;
			}
			return null;
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (this.innerPawn.Faction != null)
			{
				stringBuilder.AppendLine("Faction".Translate() + ": " + this.innerPawn.Faction);
			}
			stringBuilder.AppendLine("DeadTime".Translate(new object[]
			{
				this.Age.ToStringTicksToPeriod(false)
			}));
			float num = 1f - this.innerPawn.health.hediffSet.GetCoverageOfNotMissingNaturalParts(this.innerPawn.RaceProps.body.corePart);
			if (num != 0f)
			{
				stringBuilder.AppendLine("CorpsePercentMissing".Translate() + ": " + num.ToStringPercent());
			}
			stringBuilder.AppendLine(base.GetInspectString());
			return stringBuilder.ToString();
		}

		public void RotStageChanged()
		{
			PortraitsCache.SetDirty(this.innerPawn);
			this.NotifyColonistBar();
		}

		private BodyPartRecord GetBestBodyPartToEat(Pawn ingester, float nutritionWanted)
		{
			IEnumerable<BodyPartRecord> source = from x in this.innerPawn.health.hediffSet.GetNotMissingParts(null, null)
			where x.depth == BodyPartDepth.Outside && FoodUtility.GetBodyPartNutrition(this.innerPawn, x) > 0.001f
			select x;
			if (!source.Any<BodyPartRecord>())
			{
				return null;
			}
			return source.MinBy((BodyPartRecord x) => Mathf.Abs(FoodUtility.GetBodyPartNutrition(this.innerPawn, x) - nutritionWanted));
		}

		private void NotifyColonistBar()
		{
			if (this.innerPawn.Faction == Faction.OfPlayer && Current.ProgramState == ProgramState.MapPlaying)
			{
				Find.ColonistBar.MarkColonistsListDirty();
			}
		}
	}
}
