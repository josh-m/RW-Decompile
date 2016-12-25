using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Verse.AI.Group
{
	[StaticConstructorOnStartup]
	public class Lord : ILoadReferenceable, IExposable
	{
		private const int AttackTargetCacheInterval = 60;

		public LordManager lordManager;

		private LordToil curLordToil;

		private StateGraph graph;

		public int loadID = -1;

		private LordJob curJob;

		public Faction faction;

		public List<Pawn> ownedPawns = new List<Pawn>();

		public List<Thing> extraForbiddenThings = new List<Thing>();

		private bool initialized;

		public int ticksInToil;

		public int numPawnsLostViolently;

		public int numPawnsEverGained;

		public int initialColonyHealthTotal;

		public int lastPawnHarmTick = -99999;

		private static readonly Material FlagTex = MaterialPool.MatFrom("UI/Overlays/SquadFlag");

		private int tmpCurLordToilIdx = -1;

		private Dictionary<int, LordToilData> tmpLordToilData = new Dictionary<int, LordToilData>();

		private Dictionary<int, TriggerData> tmpTriggerData = new Dictionary<int, TriggerData>();

		public Map Map
		{
			get
			{
				return this.lordManager.map;
			}
		}

		public StateGraph Graph
		{
			get
			{
				return this.graph;
			}
		}

		public LordToil CurLordToil
		{
			get
			{
				return this.curLordToil;
			}
		}

		public LordJob LordJob
		{
			get
			{
				return this.curJob;
			}
		}

		private bool CanExistWithoutPawns
		{
			get
			{
				return this.curJob is LordJob_VoluntarilyJoinable;
			}
		}

		private void Init()
		{
			this.initialized = true;
			this.initialColonyHealthTotal = this.Map.wealthWatcher.HealthTotal;
		}

		public string GetUniqueLoadID()
		{
			return "Lord_" + this.loadID;
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.loadID, "loadID", 0, false);
			Scribe_References.LookReference<Faction>(ref this.faction, "faction", false);
			Scribe_Collections.LookList<Thing>(ref this.extraForbiddenThings, "extraForbiddenThings", LookMode.Reference, new object[0]);
			Scribe_Collections.LookList<Pawn>(ref this.ownedPawns, "ownedPawns", LookMode.Reference, new object[0]);
			Scribe_Deep.LookDeep<LordJob>(ref this.curJob, "lordJob", new object[0]);
			Scribe_Values.LookValue<bool>(ref this.initialized, "initialized", true, false);
			Scribe_Values.LookValue<int>(ref this.ticksInToil, "ticksInToil", 0, false);
			Scribe_Values.LookValue<int>(ref this.numPawnsEverGained, "numPawnsEverGained", 0, false);
			Scribe_Values.LookValue<int>(ref this.numPawnsLostViolently, "numPawnsLostViolently", 0, false);
			Scribe_Values.LookValue<int>(ref this.initialColonyHealthTotal, "initialColonyHealthTotal", 0, false);
			Scribe_Values.LookValue<int>(ref this.lastPawnHarmTick, "lastPawnHarmTick", -99999, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.extraForbiddenThings.RemoveAll((Thing x) => x == null);
			}
			this.ExposeData_StateGraph();
		}

		private void ExposeData_StateGraph()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.tmpLordToilData.Clear();
				for (int i = 0; i < this.graph.lordToils.Count; i++)
				{
					if (this.graph.lordToils[i].data != null)
					{
						this.tmpLordToilData.Add(i, this.graph.lordToils[i].data);
					}
				}
				this.tmpTriggerData.Clear();
				int num = 0;
				for (int j = 0; j < this.graph.transitions.Count; j++)
				{
					for (int k = 0; k < this.graph.transitions[j].triggers.Count; k++)
					{
						if (this.graph.transitions[j].triggers[k].data != null)
						{
							this.tmpTriggerData.Add(num, this.graph.transitions[j].triggers[k].data);
						}
						num++;
					}
				}
				this.tmpCurLordToilIdx = this.graph.lordToils.IndexOf(this.curLordToil);
			}
			Scribe_Collections.LookDictionary<int, LordToilData>(ref this.tmpLordToilData, "lordToilData", LookMode.Value, LookMode.Deep);
			Scribe_Collections.LookDictionary<int, TriggerData>(ref this.tmpTriggerData, "triggerData", LookMode.Value, LookMode.Deep);
			Scribe_Values.LookValue<int>(ref this.tmpCurLordToilIdx, "curLordToilIdx", -1, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (this.curJob.LostImportantReferenceDuringLoading)
				{
					this.lordManager.RemoveLord(this);
				}
				else
				{
					LordJob job = this.curJob;
					this.curJob = null;
					this.SetJob(job);
					foreach (KeyValuePair<int, LordToilData> current in this.tmpLordToilData)
					{
						if (current.Key < 0 || current.Key >= this.graph.lordToils.Count)
						{
							Log.Error(string.Concat(new object[]
							{
								"Could not find lord toil for lord toil data of type \"",
								current.Value.GetType(),
								"\" (lord job: \"",
								this.curJob.GetType(),
								"\"), because lord toil index is out of bounds: ",
								current.Key
							}));
						}
						else
						{
							this.graph.lordToils[current.Key].data = current.Value;
						}
					}
					this.tmpLordToilData.Clear();
					foreach (KeyValuePair<int, TriggerData> current2 in this.tmpTriggerData)
					{
						Trigger triggerByIndex = this.GetTriggerByIndex(current2.Key);
						if (triggerByIndex == null)
						{
							Log.Error(string.Concat(new object[]
							{
								"Could not find trigger for trigger data of type \"",
								current2.Value.GetType(),
								"\" (lord job: \"",
								this.curJob.GetType(),
								"\"), because trigger index is out of bounds: ",
								current2.Key
							}));
						}
						else
						{
							triggerByIndex.data = current2.Value;
						}
					}
					this.tmpTriggerData.Clear();
					if (this.tmpCurLordToilIdx < 0 || this.tmpCurLordToilIdx >= this.graph.lordToils.Count)
					{
						Log.Error(string.Concat(new object[]
						{
							"Current lord toil index out of bounds (lord job: \"",
							this.curJob.GetType(),
							"\"): ",
							this.tmpCurLordToilIdx
						}));
					}
					else
					{
						this.curLordToil = this.graph.lordToils[this.tmpCurLordToilIdx];
					}
				}
			}
		}

		public void SetJob(LordJob lordJob)
		{
			if (this.curJob != null)
			{
				this.curJob.Cleanup();
			}
			this.curJob = lordJob;
			this.curLordToil = null;
			lordJob.lord = this;
			Rand.PushSeed();
			Rand.Seed = this.loadID * 193;
			this.graph = lordJob.CreateGraph();
			Rand.PopSeed();
			this.graph.ErrorCheck();
			if (this.faction.def.autoFlee)
			{
				LordToil_PanicFlee lordToil_PanicFlee = new LordToil_PanicFlee();
				lordToil_PanicFlee.avoidGridMode = AvoidGridMode.Smart;
				for (int i = 0; i < this.graph.lordToils.Count; i++)
				{
					Transition transition = new Transition(this.graph.lordToils[i], lordToil_PanicFlee);
					transition.AddPreAction(new TransitionAction_Message("MessageFightersFleeing".Translate(new object[]
					{
						this.faction.def.pawnsPlural.CapitalizeFirst(),
						this.faction.Name
					})));
					transition.AddTrigger(new Trigger_FractionPawnsLost(0.5f));
					this.graph.AddTransition(transition);
				}
				this.graph.AddToil(lordToil_PanicFlee);
			}
			for (int j = 0; j < this.graph.lordToils.Count; j++)
			{
				this.graph.lordToils[j].lord = this;
			}
			for (int k = 0; k < this.ownedPawns.Count; k++)
			{
				this.Map.attackTargetsCache.UpdateTarget(this.ownedPawns[k]);
			}
		}

		public void Cleanup()
		{
			this.curJob.Cleanup();
			if (this.curLordToil != null)
			{
				this.curLordToil.Cleanup();
			}
			for (int i = 0; i < this.ownedPawns.Count; i++)
			{
				if (this.ownedPawns[i].mindState != null)
				{
					this.ownedPawns[i].mindState.duty = null;
				}
				this.Map.attackTargetsCache.UpdateTarget(this.ownedPawns[i]);
			}
		}

		public void AddPawn(Pawn p)
		{
			if (this.ownedPawns.Contains(p))
			{
				Log.Error(string.Concat(new object[]
				{
					"Lord for ",
					this.faction,
					" tried to add ",
					p,
					" whom it already controls."
				}));
				return;
			}
			if (p.GetLord() != null)
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to add pawn ",
					p,
					" to lord ",
					this,
					" but this pawn is already a member of lord ",
					p.GetLord(),
					". Pawns can't be members of more than one lord at the same time."
				}));
				return;
			}
			this.ownedPawns.Add(p);
			this.numPawnsEverGained++;
			this.Map.attackTargetsCache.UpdateTarget(p);
			this.curLordToil.UpdateAllDuties();
			this.curJob.Notify_PawnAdded(p);
		}

		private void RemovePawn(Pawn p)
		{
			this.ownedPawns.Remove(p);
			if (p.mindState != null)
			{
				p.mindState.duty = null;
			}
			this.Map.attackTargetsCache.UpdateTarget(p);
		}

		public void GotoToil(LordToil newLordToil)
		{
			LordToil previousToil = this.curLordToil;
			if (this.curLordToil != null)
			{
				this.curLordToil.Cleanup();
			}
			this.curLordToil = newLordToil;
			this.ticksInToil = 0;
			if (this.curLordToil.lord != this)
			{
				Log.Error("curLordToil lord is " + ((this.curLordToil.lord != null) ? this.curLordToil.lord.ToString() : "null (forgot to add toil to graph?)"));
				this.curLordToil.lord = this;
			}
			this.curLordToil.Init();
			for (int i = 0; i < this.graph.transitions.Count; i++)
			{
				if (this.graph.transitions[i].sources.Contains(this.curLordToil))
				{
					this.graph.transitions[i].SourceToilBecameActive(this.graph.transitions[i], previousToil);
				}
			}
			this.curLordToil.UpdateAllDuties();
		}

		public void LordTick()
		{
			if (!this.initialized)
			{
				this.Init();
			}
			this.curLordToil.LordToilTick();
			this.CheckTransitionOnSignal(TriggerSignal.ForTick);
			this.ticksInToil++;
		}

		private Trigger GetTriggerByIndex(int index)
		{
			int num = 0;
			for (int i = 0; i < this.graph.transitions.Count; i++)
			{
				for (int j = 0; j < this.graph.transitions[i].triggers.Count; j++)
				{
					if (num == index)
					{
						return this.graph.transitions[i].triggers[j];
					}
					num++;
				}
			}
			return null;
		}

		public void ReceiveMemo(string memo)
		{
			this.CheckTransitionOnSignal(TriggerSignal.ForMemo(memo));
		}

		public void Notify_FactionRelationsChanged(Faction otherFaction)
		{
			this.CheckTransitionOnSignal(new TriggerSignal
			{
				type = TriggerSignalType.FactionRelationsChanged,
				faction = otherFaction
			});
		}

		public void Notify_PawnLost(Pawn pawn, PawnLostCondition cond)
		{
			if (this.ownedPawns.Contains(pawn))
			{
				this.RemovePawn(pawn);
				if (cond == PawnLostCondition.IncappedOrKilled || cond == PawnLostCondition.MadePrisoner)
				{
					this.numPawnsLostViolently++;
				}
				if (this.ownedPawns.Count == 0 && !this.CanExistWithoutPawns)
				{
					this.lordManager.RemoveLord(this);
				}
				else
				{
					this.curLordToil.Notify_PawnLost(pawn, cond);
					this.curJob.Notify_PawnLost(pawn, cond);
					this.CheckTransitionOnSignal(new TriggerSignal
					{
						type = TriggerSignalType.PawnLost,
						pawn = pawn,
						condition = cond
					});
				}
				return;
			}
			Log.Error(string.Concat(new object[]
			{
				"Lord lost pawn ",
				pawn,
				" it didn't have. Condition=",
				cond
			}));
		}

		public void Notify_PawnTookDamage(Pawn victim, DamageInfo dinfo)
		{
			this.CheckTransitionOnSignal(new TriggerSignal
			{
				type = TriggerSignalType.PawnDamaged,
				pawn = victim,
				dinfo = dinfo
			});
		}

		public void Notify_PawnAttemptArrested(Pawn victim)
		{
			this.CheckTransitionOnSignal(new TriggerSignal
			{
				type = TriggerSignalType.PawnArrestAttempted,
				pawn = victim
			});
		}

		public void Notify_PawnAcquiredTarget(Pawn detector, Thing newTarg)
		{
		}

		public void Notify_ReachedDutyLocation(Pawn pawn)
		{
			this.curLordToil.Notify_ReachedDutyLocation(pawn);
		}

		public void Notify_ConstructionFailed(Pawn pawn, Frame frame, Blueprint_Build newBlueprint)
		{
			this.curLordToil.Notify_ConstructionFailed(pawn, frame, newBlueprint);
		}

		private bool CheckTransitionOnSignal(TriggerSignal signal)
		{
			if (Trigger_PawnHarmed.SignalIsHarm(signal))
			{
				this.lastPawnHarmTick = Find.TickManager.TicksGame;
			}
			for (int i = 0; i < this.graph.transitions.Count; i++)
			{
				if (this.graph.transitions[i].sources.Contains(this.curLordToil) && this.graph.transitions[i].CheckSignal(this, signal))
				{
					return true;
				}
			}
			return false;
		}

		private Vector3 DebugCenter()
		{
			Vector3 result = this.Map.Center.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
			if ((from p in this.ownedPawns
			where p.Spawned
			select p).Any<Pawn>())
			{
				result.x = (from p in this.ownedPawns
				where p.Spawned
				select p).Average((Pawn p) => p.DrawPos.x);
				result.z = (from p in this.ownedPawns
				where p.Spawned
				select p).Average((Pawn p) => p.DrawPos.z);
			}
			return result;
		}

		public void DebugDraw()
		{
			Vector3 a = this.DebugCenter();
			IntVec3 flagLoc = this.curLordToil.FlagLoc;
			if (flagLoc.IsValid)
			{
				Graphics.DrawMesh(MeshPool.plane14, flagLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Building), Quaternion.identity, Lord.FlagTex, 0);
			}
			GenDraw.DrawLineBetween(a, flagLoc.ToVector3Shifted(), SimpleColor.Red);
			foreach (Pawn current in this.ownedPawns)
			{
				SimpleColor color = current.InMentalState ? SimpleColor.Yellow : SimpleColor.White;
				GenDraw.DrawLineBetween(a, current.DrawPos, color);
			}
		}

		public void DebugOnGUI()
		{
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Tiny;
			string label;
			if (this.CurLordToil != null)
			{
				label = string.Concat(new object[]
				{
					"toil ",
					this.graph.lordToils.IndexOf(this.CurLordToil),
					"\n",
					this.CurLordToil.ToString()
				});
			}
			else
			{
				label = "toil=NULL";
			}
			Vector2 vector = this.DebugCenter().MapToUIPosition();
			Widgets.Label(new Rect(vector.x - 100f, vector.y - 100f, 200f, 200f), label);
			Text.Anchor = TextAnchor.UpperLeft;
		}

		public string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Start steal threshold: " + StealAIUtility.StartStealingMarketValueThreshold(this).ToString("F0"));
			stringBuilder.AppendLine("Duties:");
			foreach (Pawn current in this.ownedPawns)
			{
				stringBuilder.AppendLine(string.Concat(new object[]
				{
					"   ",
					current.LabelCap,
					" - ",
					current.mindState.duty
				}));
			}
			stringBuilder.AppendLine("Faction data:");
			stringBuilder.AppendLine(this.faction.DebugString());
			stringBuilder.AppendLine("Raw save data:");
			stringBuilder.AppendLine(Scribe.DebugOutputFor(this));
			return stringBuilder.ToString();
		}

		private bool ShouldDoDebugOutput()
		{
			IntVec3 a = UI.MouseCell();
			IntVec3 flagLoc = this.curLordToil.FlagLoc;
			if (flagLoc.IsValid && a == flagLoc)
			{
				return true;
			}
			for (int i = 0; i < this.ownedPawns.Count; i++)
			{
				if (a == this.ownedPawns[i].Position)
				{
					return true;
				}
			}
			return false;
		}
	}
}
