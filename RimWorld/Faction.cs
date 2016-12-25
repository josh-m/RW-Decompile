using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class Faction : ICommunicable, ILoadReferenceable, IExposable
	{
		private const float InitialHostileThreshold = -40f;

		private const float BecomeHostileThreshold = -80f;

		private const float BecomeNeutralThreshold = 0f;

		public const float GoodwillScale = 100f;

		private const float GoodwillLossPerDamage = 1.3f;

		public FactionDef def;

		private string name;

		public IntVec2 homeSquare = IntVec2.Invalid;

		public int loadID = -1;

		public int randomKey;

		private List<FactionRelation> relations = new List<FactionRelation>();

		public Pawn leader;

		private FactionTacticalMemory tacticalMemoryInt = new FactionTacticalMemory();

		public KidnappedPawnsTracker kidnapped;

		private List<PredatorThreat> predatorThreats = new List<PredatorThreat>();

		public ByteGrid avoidGridBasic;

		public ByteGrid avoidGridSmart;

		public string Name
		{
			get
			{
				if (this.HasName)
				{
					return this.name;
				}
				return this.def.LabelCap;
			}
			set
			{
				this.name = value;
			}
		}

		public bool HasName
		{
			get
			{
				return this.name != null;
			}
		}

		public bool IsPlayer
		{
			get
			{
				return this.def.isPlayer;
			}
		}

		public float PlayerGoodwill
		{
			get
			{
				return this.GoodwillWith(Faction.OfPlayer);
			}
		}

		public FactionTacticalMemory TacticalMemory
		{
			get
			{
				if (this.tacticalMemoryInt == null)
				{
					this.tacticalMemoryInt = new FactionTacticalMemory();
				}
				return this.tacticalMemoryInt;
			}
		}

		public static Faction OfPlayer
		{
			get
			{
				if (Find.Map != null)
				{
					return Find.FactionManager.FactionInWorldSquare(Find.Map.WorldCoords);
				}
				if (Find.GameInitData.playerFaction != null)
				{
					return Find.GameInitData.playerFaction;
				}
				Log.Error("Cannot get faction OfPlayer since both Map and MapInitData.playerFaction are null.");
				return null;
			}
		}

		public static Faction OfMechanoids
		{
			get
			{
				return Find.FactionManager.FirstFactionOfDef(FactionDefOf.Mechanoid);
			}
		}

		public static Faction OfInsects
		{
			get
			{
				return Find.FactionManager.FirstFactionOfDef(FactionDefOf.Insect);
			}
		}

		public Faction()
		{
			this.randomKey = Rand.Range(0, 2147483647);
			this.kidnapped = new KidnappedPawnsTracker(this);
		}

		public void ExposeData()
		{
			Scribe_Defs.LookDef<FactionDef>(ref this.def, "def");
			Scribe_Values.LookValue<string>(ref this.name, "name", null, false);
			Scribe_Values.LookValue<IntVec2>(ref this.homeSquare, "homeSquare", IntVec2.Invalid, false);
			Scribe_Values.LookValue<int>(ref this.loadID, "loadID", 0, false);
			Scribe_Values.LookValue<int>(ref this.randomKey, "randomKey", 0, false);
			Scribe_References.LookReference<Pawn>(ref this.leader, "leader", false);
			Scribe_Collections.LookList<FactionRelation>(ref this.relations, "relations", LookMode.Deep, new object[0]);
			Scribe_Deep.LookDeep<FactionTacticalMemory>(ref this.tacticalMemoryInt, "tacticalMemory", new object[0]);
			Scribe_Deep.LookDeep<KidnappedPawnsTracker>(ref this.kidnapped, "kidnapped", new object[]
			{
				this
			});
			Scribe_Collections.LookList<PredatorThreat>(ref this.predatorThreats, "predatorThreats", LookMode.Deep, new object[0]);
			Scribe_Deep.LookDeep<ByteGrid>(ref this.avoidGridBasic, "avoidGridBasic", new object[0]);
			Scribe_Deep.LookDeep<ByteGrid>(ref this.avoidGridSmart, "avoidGridSmart", new object[0]);
		}

		public void FactionTick()
		{
			if (Find.TickManager.TicksGame % 1001 == 0 && this != Faction.OfPlayer)
			{
				if (this.PlayerGoodwill < this.def.naturalColonyGoodwill.min)
				{
					this.AffectGoodwillWith(Faction.OfPlayer, this.def.goodwillDailyGain * 0f);
				}
				else if (this.PlayerGoodwill > this.def.naturalColonyGoodwill.max)
				{
					this.AffectGoodwillWith(Faction.OfPlayer, -this.def.goodwillDailyFall * 0f);
				}
			}
			this.kidnapped.KidnappedPawnsTrackerTick();
			for (int i = this.predatorThreats.Count - 1; i >= 0; i--)
			{
				PredatorThreat predatorThreat = this.predatorThreats[i];
				if (predatorThreat.Expired)
				{
					this.predatorThreats.RemoveAt(i);
					Find.AttackTargetsCache.UpdateTarget(predatorThreat.predator);
				}
			}
			if (this.IsPlayer && !this.HasName && Find.TickManager.TicksGame % 1000 == 200 && GenDate.DaysPassed > 2 && Find.MapPawns.FreeColonistsSpawnedCount >= 2 && !Find.GameEnder.gameEnding)
			{
				if (!Find.AttackTargetsCache.TargetsHostileToColony.Any((IAttackTarget x) => !x.ThreatDisabled()))
				{
					Find.WindowStack.Add(new Dialog_NamePlayerFaction());
				}
			}
		}

		public void TryMakeInitialRelationsWith(Faction other)
		{
			if (this.RelationWith(other, true) != null)
			{
				return;
			}
			float a = this.def.startingGoodwill.RandomInRange;
			if (this.IsPlayer)
			{
				a = 100f;
			}
			float b = other.def.startingGoodwill.RandomInRange;
			if (other.IsPlayer)
			{
				b = 100f;
			}
			float num = Mathf.Min(a, b);
			FactionRelation factionRelation = new FactionRelation();
			factionRelation.other = other;
			factionRelation.goodwill = num;
			factionRelation.hostile = (num < -40f);
			this.relations.Add(factionRelation);
			FactionRelation factionRelation2 = new FactionRelation();
			factionRelation2.other = this;
			factionRelation2.goodwill = num;
			factionRelation2.hostile = (num < -40f);
			other.relations.Add(factionRelation2);
		}

		public FactionRelation RelationWith(Faction other, bool allowNull = false)
		{
			if (other == this)
			{
				Log.Error("Tried to get relation between faction " + this + " and itself.");
				return new FactionRelation();
			}
			for (int i = 0; i < this.relations.Count; i++)
			{
				if (this.relations[i].other == other)
				{
					return this.relations[i];
				}
			}
			if (!allowNull)
			{
				Log.Error(string.Concat(new string[]
				{
					"Faction ",
					this.name,
					" has null relation with ",
					this.name,
					". Returning dummy relation."
				}));
				return new FactionRelation();
			}
			return null;
		}

		public float GoodwillWith(Faction other)
		{
			return this.RelationWith(other, false).goodwill;
		}

		public void AffectGoodwillWith(Faction other, float goodwillChange)
		{
			if (this.def.hidden || other.def.hidden)
			{
				return;
			}
			if (goodwillChange > 0f && !this.def.appreciative)
			{
				return;
			}
			float value = this.GoodwillWith(other) + goodwillChange;
			FactionRelation factionRelation = this.RelationWith(other, false);
			factionRelation.goodwill = Mathf.Clamp(value, -100f, 100f);
			if (!this.HostileTo(other) && this.GoodwillWith(other) < -80f)
			{
				this.SetHostileTo(other, true);
				if (Current.ProgramState == ProgramState.MapPlaying && Find.TickManager.TicksGame > 100 && other == Faction.OfPlayer)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelRelationsChangeBad".Translate(), "RelationsBrokenDown".Translate(new object[]
					{
						this.name
					}), LetterType.BadNonUrgent, null);
				}
			}
			if (this.HostileTo(other) && this.GoodwillWith(other) > 0f)
			{
				this.SetHostileTo(other, false);
				if (Current.ProgramState == ProgramState.MapPlaying && Find.TickManager.TicksGame > 100 && other == Faction.OfPlayer)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelRelationsChangeGood".Translate(), "RelationsWarmed".Translate(new object[]
					{
						this.name
					}), LetterType.BadNonUrgent, null);
				}
			}
		}

		public void SetHostileTo(Faction other, bool hostile)
		{
			if (this.def.hidden || other.def.hidden)
			{
				return;
			}
			FactionRelation factionRelation = this.RelationWith(other, false);
			if (hostile)
			{
				if (Current.ProgramState == ProgramState.MapPlaying)
				{
					foreach (Pawn current in Find.MapPawns.PawnsInFaction(this).ToList<Pawn>())
					{
						if (current.HostFaction == other)
						{
							current.guest.SetGuestStatus(current.HostFaction, true);
						}
					}
				}
				if (!factionRelation.hostile)
				{
					other.RelationWith(this, false).hostile = true;
					factionRelation.hostile = true;
					if (factionRelation.goodwill > -80f)
					{
						factionRelation.goodwill = -80f;
					}
				}
			}
			else if (factionRelation.hostile)
			{
				other.RelationWith(this, false).hostile = false;
				factionRelation.hostile = false;
				if (factionRelation.goodwill < 0f)
				{
					factionRelation.goodwill = 0f;
				}
			}
			if (Current.ProgramState == ProgramState.MapPlaying)
			{
				Find.AttackTargetsCache.Notify_FactionHostilityChanged(this, other);
			}
		}

		public void RemoveAllRelations()
		{
			foreach (Faction current in Find.FactionManager.AllFactionsListForReading)
			{
				if (current != this)
				{
					current.relations.RemoveAll((FactionRelation x) => x.other == this);
				}
			}
			this.relations.Clear();
		}

		public void Notify_MemberTookDamage(Pawn member, DamageInfo dinfo)
		{
			if (dinfo.Instigator == null)
			{
				return;
			}
			Pawn pawn = dinfo.Instigator as Pawn;
			if (pawn != null && pawn.CurJob != null && pawn.CurJob.def == JobDefOf.PredatorHunt)
			{
				this.TookDamageFromPredator(pawn);
			}
			if (dinfo.Instigator.Faction == null || !dinfo.Def.externalViolence || this.HostileTo(dinfo.Instigator.Faction))
			{
				return;
			}
			if (member.InMentalState && member.MentalStateDef.IsAggro)
			{
				return;
			}
			Pawn pawn2 = dinfo.Instigator as Pawn;
			if (pawn2 != null && pawn2.InMentalState && pawn2.MentalStateDef == MentalStateDefOf.Berserk)
			{
				return;
			}
			if (dinfo.Instigator.Faction == Faction.OfPlayer && PrisonBreakUtility.IsPrisonBreaking(member))
			{
				return;
			}
			float num = (float)Mathf.Min(100, dinfo.Amount);
			float goodwillChange = -1.3f * num;
			if (dinfo.Instigator.Faction == Faction.OfPlayer && this != Faction.OfPlayer)
			{
				this.AffectGoodwillWith(dinfo.Instigator.Faction, goodwillChange);
			}
		}

		public void Notify_MemberCaptured(Pawn member, Faction violator)
		{
			if (violator == this || this.HostileTo(violator) || member.Faction.def.hidden || violator.def.hidden)
			{
				return;
			}
			this.SetHostileTo(violator, true);
			Find.LetterStack.ReceiveLetter("LetterLabelRelationsChangeBad".Translate(), "RelationsBrokenCapture".Translate(new object[]
			{
				member,
				this.name
			}), LetterType.BadNonUrgent, member, null);
		}

		public void Notify_LeaderDied()
		{
			Pawn pawn = this.leader;
			this.GenerateNewLeader();
			Find.LetterStack.ReceiveLetter("LetterLeadersDeathLabel".Translate(), "LetterLeadersDeath".Translate(new object[]
			{
				pawn.Name.ToStringFull,
				this.name,
				this.leader.Name.ToStringFull
			}).CapitalizeFirst(), LetterType.BadNonUrgent, null);
		}

		public void GenerateNewLeader()
		{
			List<PawnKindDef> list = new List<PawnKindDef>();
			if (this.def.pawnGroupMakers != null)
			{
				foreach (PawnGroupMaker_Normal current in this.def.pawnGroupMakers.OfType<PawnGroupMaker_Normal>())
				{
					foreach (PawnGenOption current2 in current.options)
					{
						if (current2.kind.factionLeader)
						{
							list.Add(current2.kind);
						}
					}
				}
				PawnKindDef kindDef;
				if (list.TryRandomElement(out kindDef))
				{
					this.leader = PawnGenerator.GeneratePawn(kindDef, this);
					if (this.leader.RaceProps.IsFlesh)
					{
						this.leader.relations.everSeenByPlayer = true;
					}
					if (!Find.WorldPawns.Contains(this.leader))
					{
						Find.WorldPawns.PassToWorld(this.leader, PawnDiscardDecideMode.Keep);
					}
				}
			}
		}

		public string GetCallLabel()
		{
			return this.name;
		}

		public string GetInfoText()
		{
			string text = this.def.LabelCap;
			string text2 = text;
			text = string.Concat(new string[]
			{
				text2,
				"\n",
				"ColonyGoodwill".Translate(),
				": ",
				this.PlayerGoodwill.ToString("###0")
			});
			if (this.HostileTo(Faction.OfPlayer))
			{
				text = text + "\n" + "Hostile".Translate();
			}
			else
			{
				text = text + "\n" + "Neutral".Translate();
			}
			return text;
		}

		public void TryOpenComms(Pawn negotiator)
		{
			Dialog_Negotiation dialog_Negotiation = new Dialog_Negotiation(negotiator, this, FactionDialogMaker.FactionDialogFor(negotiator, this), true);
			dialog_Negotiation.soundAmbient = SoundDefOf.RadioComms_Ambience;
			Find.WindowStack.Add(dialog_Negotiation);
		}

		private void TookDamageFromPredator(Pawn predator)
		{
			for (int i = 0; i < this.predatorThreats.Count; i++)
			{
				if (this.predatorThreats[i].predator == predator)
				{
					this.predatorThreats[i].lastAttackTicks = Find.TickManager.TicksGame;
					return;
				}
			}
			PredatorThreat predatorThreat = new PredatorThreat();
			predatorThreat.predator = predator;
			predatorThreat.lastAttackTicks = Find.TickManager.TicksGame;
			this.predatorThreats.Add(predatorThreat);
		}

		public bool HasPredatorRecentlyAttackedAnyone(Pawn predator)
		{
			for (int i = 0; i < this.predatorThreats.Count; i++)
			{
				if (this.predatorThreats[i].predator == predator)
				{
					return true;
				}
			}
			return false;
		}

		public string GetUniqueLoadID()
		{
			return "Faction_" + this.loadID;
		}

		public override string ToString()
		{
			return this.name;
		}

		public string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (this.avoidGridSmart != null)
			{
				stringBuilder.Append("Avoidgrid val at mouse: ");
				if (Gen.MouseCell().InBounds())
				{
					stringBuilder.AppendLine(this.avoidGridSmart[Gen.MouseCell()].ToString());
				}
				stringBuilder.Append("Avoidgrid pathcost at mouse: ");
				if (Gen.MouseCell().InBounds())
				{
					stringBuilder.AppendLine(((int)(this.avoidGridSmart[Gen.MouseCell()] * 8)).ToString());
				}
			}
			return stringBuilder.ToString();
		}

		public void DebugDraw()
		{
			if (this.avoidGridSmart != null)
			{
				this.avoidGridSmart.DebugDraw();
			}
		}
	}
}
