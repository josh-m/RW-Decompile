using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompHatcher : ThingComp
	{
		private float gestateProgress;

		public Pawn hatcheeParent;

		public Pawn otherParent;

		public Faction hatcheeFaction;

		public CompProperties_Hatcher Props
		{
			get
			{
				return (CompProperties_Hatcher)this.props;
			}
		}

		private CompTemperatureRuinable FreezerComp
		{
			get
			{
				return this.parent.GetComp<CompTemperatureRuinable>();
			}
		}

		protected bool TemperatureDamaged
		{
			get
			{
				CompTemperatureRuinable freezerComp = this.FreezerComp;
				return freezerComp != null && this.FreezerComp.Ruined;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.LookValue<float>(ref this.gestateProgress, "gestateProgress", 0f, false);
			Scribe_References.LookReference<Pawn>(ref this.hatcheeParent, "hatcheeParent", false);
			Scribe_References.LookReference<Pawn>(ref this.otherParent, "otherParent", false);
			Scribe_References.LookReference<Faction>(ref this.hatcheeFaction, "hatcheeFaction", false);
		}

		public override void CompTick()
		{
			if (!this.TemperatureDamaged)
			{
				float num = 1f / (this.Props.hatcherDaystoHatch * 60000f);
				this.gestateProgress += num;
				if (this.gestateProgress > 1f)
				{
					this.Hatch();
				}
			}
		}

		public void Hatch()
		{
			PawnGenerationRequest request = new PawnGenerationRequest(this.Props.hatcherPawn, this.hatcheeFaction, PawnGenerationContext.NonPlayer, null, false, true, false, false, true, false, 1f, false, true, true, null, null, null, null, null, null);
			for (int i = 0; i < this.parent.stackCount; i++)
			{
				Pawn pawn = PawnGenerator.GeneratePawn(request);
				if (PawnUtility.TrySpawnHatchedOrBornPawn(pawn, this.parent))
				{
					if (pawn != null)
					{
						if (this.hatcheeParent != null)
						{
							if (pawn.playerSettings != null && this.hatcheeParent.playerSettings != null && this.hatcheeParent.Faction == this.hatcheeFaction)
							{
								pawn.playerSettings.AreaRestriction = this.hatcheeParent.playerSettings.AreaRestriction;
							}
							if (pawn.RaceProps.IsFlesh)
							{
								pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, this.hatcheeParent);
							}
						}
						if (this.otherParent != null && (this.hatcheeParent == null || this.hatcheeParent.gender != this.otherParent.gender) && pawn.RaceProps.IsFlesh)
						{
							pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, this.otherParent);
						}
					}
					if (this.parent.Spawned)
					{
						FilthMaker.MakeFilth(this.parent.Position, this.parent.Map, ThingDefOf.FilthAmnioticFluid, 1);
					}
				}
				else
				{
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				}
			}
			this.parent.Destroy(DestroyMode.Vanish);
		}

		public override void PreAbsorbStack(Thing otherStack, int count)
		{
			float t = (float)count / (float)(this.parent.stackCount + count);
			CompHatcher comp = ((ThingWithComps)otherStack).GetComp<CompHatcher>();
			float b = comp.gestateProgress;
			this.gestateProgress = Mathf.Lerp(this.gestateProgress, b, t);
		}

		public override void PostSplitOff(Thing piece)
		{
			CompHatcher comp = ((ThingWithComps)piece).GetComp<CompHatcher>();
			comp.gestateProgress = this.gestateProgress;
			comp.hatcheeParent = this.hatcheeParent;
			comp.otherParent = this.otherParent;
			comp.hatcheeFaction = this.hatcheeFaction;
		}

		public override void PrePreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
		{
			if (action == TradeAction.PlayerBuys)
			{
				this.hatcheeFaction = Faction.OfPlayer;
			}
			else if (action == TradeAction.PlayerSells)
			{
				this.hatcheeFaction = null;
			}
		}

		public override string CompInspectStringExtra()
		{
			if (!this.TemperatureDamaged)
			{
				return "EggProgress".Translate() + ": " + this.gestateProgress.ToStringPercent();
			}
			return null;
		}
	}
}
