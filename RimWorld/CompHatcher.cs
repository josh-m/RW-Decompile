using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompHatcher : ThingComp
	{
		private const float MinEmbryoTemperatureThreshold = -25f;

		private const float FrozenPercentageChangeSpeed = 5E-05f;

		private float gestateProgress;

		public Pawn hatcheeParent;

		public Pawn otherParent;

		public Faction hatcheeFaction;

		public float frozenPercentage;

		public bool damagedEmbryo;

		private bool Frozen
		{
			get
			{
				return this.frozenPercentage >= 1f;
			}
		}

		public CompProperties_Hatcher Props
		{
			get
			{
				return (CompProperties_Hatcher)this.props;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.LookValue<float>(ref this.gestateProgress, "gestateProgress", 0f, false);
			Scribe_References.LookReference<Pawn>(ref this.hatcheeParent, "hatcheeParent", false);
			Scribe_References.LookReference<Pawn>(ref this.otherParent, "otherParent", false);
			Scribe_References.LookReference<Faction>(ref this.hatcheeFaction, "hatcheeFaction", false);
			Scribe_Values.LookValue<float>(ref this.frozenPercentage, "frozenPercentage", 0f, false);
			Scribe_Values.LookValue<bool>(ref this.damagedEmbryo, "damagedEmbryo", false, false);
		}

		public override void CompTick()
		{
			if (!this.damagedEmbryo)
			{
				float temperature = this.parent.Position.GetTemperature();
				if (this.parent.holder == null)
				{
					this.frozenPercentage -= (temperature - -25f) * 5E-05f;
				}
				if (this.frozenPercentage >= 1f)
				{
					this.damagedEmbryo = true;
				}
				else if (this.frozenPercentage < 0f)
				{
					this.frozenPercentage = 0f;
				}
				if (!this.damagedEmbryo)
				{
					float num = 1f / (this.Props.hatcherDaystoHatch * 60000f);
					this.gestateProgress += num;
					if (this.gestateProgress > 1f)
					{
						this.Hatch();
					}
				}
			}
		}

		public void Hatch()
		{
			PawnGenerationRequest request = new PawnGenerationRequest(this.Props.hatcherPawn, this.hatcheeFaction, PawnGenerationContext.NonPlayer, false, true, false, false, true, false, 1f, false, true, true, null, null, null, null, null, null);
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
						FilthMaker.MakeFilth(this.parent.Position, ThingDefOf.FilthAmnioticFluid, 1);
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
			float b2 = comp.frozenPercentage;
			this.frozenPercentage = Mathf.Lerp(this.frozenPercentage, b2, t);
		}

		public override bool AllowStackWith(Thing other)
		{
			CompHatcher comp = ((ThingWithComps)other).GetComp<CompHatcher>();
			return this.damagedEmbryo == comp.damagedEmbryo;
		}

		public override void PostSplitOff(Thing piece)
		{
			CompHatcher comp = ((ThingWithComps)piece).GetComp<CompHatcher>();
			comp.gestateProgress = this.gestateProgress;
			comp.frozenPercentage = this.frozenPercentage;
			comp.damagedEmbryo = this.damagedEmbryo;
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
			string text = "EggProgress".Translate() + ": ";
			if (this.damagedEmbryo)
			{
				text += "EggDamagedEmbryo".Translate();
			}
			else
			{
				text += this.gestateProgress.ToStringPercent();
			}
			return text;
		}
	}
}
