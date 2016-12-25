using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI.Group;

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

		public int loadID = -1;

		public int randomKey;

		public float colorFromSpectrum;

		private List<FactionRelation> relations = new List<FactionRelation>();

		public Pawn leader;

		private FactionTacticalMemory tacticalMemoryInt = new FactionTacticalMemory();

		public KidnappedPawnsTracker kidnapped;

		private List<PredatorThreat> predatorThreats = new List<PredatorThreat>();

		public Dictionary<Map, ByteGrid> avoidGridsBasic = new Dictionary<Map, ByteGrid>();

		public Dictionary<Map, ByteGrid> avoidGridsSmart = new Dictionary<Map, ByteGrid>();

		public bool defeated;

		public int lastTraderRequestTick = -9999999;

		private List<Map> avoidGridsBasicKeysWorkingList;

		private List<ByteGrid> avoidGridsBasicValuesWorkingList;

		private List<Map> avoidGridsSmartKeysWorkingList;

		private List<ByteGrid> avoidGridsSmartValuesWorkingList;

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

		public Color Color
		{
			get
			{
				if (this.def.colorSpectrum.NullOrEmpty<Color>())
				{
					return Color.white;
				}
				return ColorsFromSpectrum.Get(this.def.colorSpectrum, this.colorFromSpectrum);
			}
		}

		public static Faction OfPlayer
		{
			get
			{
				Faction ofPlayerSilentFail = Faction.OfPlayerSilentFail;
				if (ofPlayerSilentFail == null)
				{
					Log.Error("Could not find player faction.");
				}
				return ofPlayerSilentFail;
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

		public static Faction OfPlayerSilentFail
		{
			get
			{
				if (Current.ProgramState != ProgramState.Playing)
				{
					GameInitData gameInitData = Find.GameInitData;
					if (gameInitData != null && gameInitData.playerFaction != null)
					{
						return gameInitData.playerFaction;
					}
				}
				List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
				for (int i = 0; i < allFactionsListForReading.Count; i++)
				{
					if (allFactionsListForReading[i].def.isPlayer)
					{
						return allFactionsListForReading[i];
					}
				}
				return null;
			}
		}

		public Faction()
		{
			this.randomKey = Rand.Range(0, 2147483647);
			this.kidnapped = new KidnappedPawnsTracker(this);
		}

		public void ExposeData()
		{
			Scribe_References.LookReference<Pawn>(ref this.leader, "leader", false);
			Scribe_Collections.LookDictionary<Map, ByteGrid>(ref this.avoidGridsBasic, "avoidGridsBasic", LookMode.Reference, LookMode.Deep, ref this.avoidGridsBasicKeysWorkingList, ref this.avoidGridsBasicValuesWorkingList);
			Scribe_Collections.LookDictionary<Map, ByteGrid>(ref this.avoidGridsSmart, "avoidGridsSmart", LookMode.Reference, LookMode.Deep, ref this.avoidGridsSmartKeysWorkingList, ref this.avoidGridsSmartValuesWorkingList);
			Scribe_Defs.LookDef<FactionDef>(ref this.def, "def");
			Scribe_Values.LookValue<string>(ref this.name, "name", null, false);
			Scribe_Values.LookValue<int>(ref this.loadID, "loadID", 0, false);
			Scribe_Values.LookValue<int>(ref this.randomKey, "randomKey", 0, false);
			Scribe_Values.LookValue<float>(ref this.colorFromSpectrum, "colorFromSpectrum", 0f, false);
			Scribe_Collections.LookList<FactionRelation>(ref this.relations, "relations", LookMode.Deep, new object[0]);
			Scribe_Deep.LookDeep<FactionTacticalMemory>(ref this.tacticalMemoryInt, "tacticalMemory", new object[0]);
			Scribe_Deep.LookDeep<KidnappedPawnsTracker>(ref this.kidnapped, "kidnapped", new object[]
			{
				this
			});
			Scribe_Collections.LookList<PredatorThreat>(ref this.predatorThreats, "predatorThreats", LookMode.Deep, new object[0]);
			Scribe_Values.LookValue<bool>(ref this.defeated, "defeated", false, false);
			Scribe_Values.LookValue<int>(ref this.lastTraderRequestTick, "lastTraderRequestTick", -9999999, false);
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
					if (predatorThreat.predator.Spawned)
					{
						predatorThreat.predator.Map.attackTargetsCache.UpdateTarget(predatorThreat.predator);
					}
				}
			}
			if (Find.TickManager.TicksGame % 1000 == 200 && this.IsPlayer)
			{
				if (NamePlayerFactionAndBaseUtility.CanNameFactionNow())
				{
					FactionBase factionBase = Find.WorldObjects.FactionBases.Find((FactionBase x) => NamePlayerFactionAndBaseUtility.CanNameFactionBaseSoon(x));
					if (factionBase != null)
					{
						Find.WindowStack.Add(new Dialog_NamePlayerFactionAndBase(factionBase));
					}
					else
					{
						Find.WindowStack.Add(new Dialog_NamePlayerFaction());
					}
				}
				else
				{
					FactionBase factionBase2 = Find.WorldObjects.FactionBases.Find((FactionBase x) => NamePlayerFactionAndBaseUtility.CanNameFactionBaseNow(x));
					if (factionBase2 != null)
					{
						if (NamePlayerFactionAndBaseUtility.CanNameFactionSoon())
						{
							Find.WindowStack.Add(new Dialog_NamePlayerFactionAndBase(factionBase2));
						}
						else
						{
							Find.WindowStack.Add(new Dialog_NamePlayerFactionBase(factionBase2));
						}
					}
				}
			}
		}

		public ByteGrid GetAvoidGridBasic(Map map)
		{
			ByteGrid result;
			if (this.avoidGridsBasic.TryGetValue(map, out result))
			{
				return result;
			}
			return null;
		}

		public ByteGrid GetAvoidGridSmart(Map map)
		{
			ByteGrid result;
			if (this.avoidGridsSmart.TryGetValue(map, out result))
			{
				return result;
			}
			return null;
		}

		public void Notify_MapRemoved(Map map)
		{
			this.avoidGridsBasic.Remove(map);
			this.avoidGridsSmart.Remove(map);
			this.tacticalMemoryInt.Notify_MapRemoved(map);
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
			if (goodwillChange > 0f && ((this.IsPlayer && FactionBaseUtility.IsPlayerAttackingAnyFactionBaseOf(other)) || (other.IsPlayer && FactionBaseUtility.IsPlayerAttackingAnyFactionBaseOf(this))))
			{
				return;
			}
			float value = this.GoodwillWith(other) + goodwillChange;
			FactionRelation factionRelation = this.RelationWith(other, false);
			factionRelation.goodwill = Mathf.Clamp(value, -100f, 100f);
			if (!this.HostileTo(other) && this.GoodwillWith(other) < -80f)
			{
				this.SetHostileTo(other, true);
				if (Current.ProgramState == ProgramState.Playing && Find.TickManager.TicksGame > 100 && other == Faction.OfPlayer)
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
				if (Current.ProgramState == ProgramState.Playing && Find.TickManager.TicksGame > 100 && other == Faction.OfPlayer)
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
				if (Current.ProgramState == ProgramState.Playing)
				{
					foreach (Pawn current in PawnsFinder.AllMapsAndWorld_Alive.ToList<Pawn>())
					{
						if ((current.Faction == this && current.HostFaction == other) || (current.Faction == other && current.HostFaction == this))
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
			if (Current.ProgramState == ProgramState.Playing)
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					maps[i].attackTargetsCache.Notify_FactionHostilityChanged(this, other);
					LordManager lordManager = maps[i].lordManager;
					for (int j = 0; j < lordManager.lords.Count; j++)
					{
						Lord lord = lordManager.lords[j];
						if (lord.faction == other)
						{
							lord.Notify_FactionRelationsChanged(this);
						}
						else if (lord.faction == this)
						{
							lord.Notify_FactionRelationsChanged(other);
						}
					}
				}
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
			Find.LetterStack.ReceiveLetter("LetterLabelRelationsChangeBad".Translate().CapitalizeFirst(), "RelationsBrokenCapture".Translate(new object[]
			{
				member,
				this.name
			}), LetterType.BadNonUrgent, member, null);
		}

		public void Notify_LeaderDied()
		{
			Pawn pawn = this.leader;
			this.GenerateNewLeader();
			Find.LetterStack.ReceiveLetter("LetterLeadersDeathLabel".Translate(new object[]
			{
				this.name,
				this.def.leaderTitle
			}).CapitalizeFirst(), "LetterLeadersDeath".Translate(new object[]
			{
				pawn.Name.ToStringFull,
				this.name,
				this.leader.Name.ToStringFull,
				this.def.leaderTitle
			}).CapitalizeFirst(), LetterType.Good, null);
		}

		public void Notify_LeaderLost()
		{
			Pawn pawn = this.leader;
			this.GenerateNewLeader();
			Find.LetterStack.ReceiveLetter("LetterLeaderChangedLabel".Translate(new object[]
			{
				this.name,
				this.def.leaderTitle
			}).CapitalizeFirst(), "LetterLeaderChanged".Translate(new object[]
			{
				pawn.Name.ToStringFull,
				this.name,
				this.leader.Name.ToStringFull,
				this.def.leaderTitle
			}).CapitalizeFirst(), LetterType.Good, null);
		}

		public void GenerateNewLeader()
		{
			List<PawnKindDef> list = new List<PawnKindDef>();
			if (this.def.pawnGroupMakers != null)
			{
				foreach (PawnGroupMaker current in from x in this.def.pawnGroupMakers
				where x.kindDef == PawnGroupKindDefOf.Normal
				select x)
				{
					foreach (PawnGenOption current2 in current.options)
					{
						if (current2.kind.factionLeader)
						{
							list.Add(current2.kind);
						}
					}
				}
				PawnKindDef kind;
				if (list.TryRandomElement(out kind))
				{
					PawnGenerationRequest request = new PawnGenerationRequest(kind, this, PawnGenerationContext.NonPlayer, null, false, false, false, false, true, false, 1f, false, true, true, null, null, null, null, null, null);
					this.leader = PawnGenerator.GeneratePawn(request);
					if (this.leader.RaceProps.IsFlesh)
					{
						this.leader.relations.everSeenByPlayer = true;
					}
					if (!Find.WorldPawns.Contains(this.leader))
					{
						Find.WorldPawns.PassToWorld(this.leader, PawnDiscardDecideMode.KeepForever);
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
			if (this.name != null)
			{
				return this.name;
			}
			return this.def.defName;
		}

		public string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			ByteGrid avoidGridSmart = this.GetAvoidGridSmart(Find.VisibleMap);
			if (avoidGridSmart != null)
			{
				stringBuilder.Append("Avoidgrid val at mouse: ");
				if (UI.MouseCell().InBounds(Find.VisibleMap))
				{
					stringBuilder.AppendLine(avoidGridSmart[UI.MouseCell()].ToString());
				}
				stringBuilder.Append("Avoidgrid pathcost at mouse: ");
				if (UI.MouseCell().InBounds(Find.VisibleMap))
				{
					stringBuilder.AppendLine(((int)(avoidGridSmart[UI.MouseCell()] * 8)).ToString());
				}
			}
			return stringBuilder.ToString();
		}

		public void DebugDrawOnMap()
		{
			ByteGrid avoidGridSmart = this.GetAvoidGridSmart(Find.VisibleMap);
			if (avoidGridSmart != null)
			{
				avoidGridSmart.DebugDraw();
			}
		}
	}
}
