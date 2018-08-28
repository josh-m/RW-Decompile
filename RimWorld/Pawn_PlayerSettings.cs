using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class Pawn_PlayerSettings : IExposable
	{
		private Pawn pawn;

		private Area areaAllowedInt;

		public int joinTick = -1;

		private Pawn master;

		public bool followDrafted;

		public bool followFieldwork;

		public bool animalsReleased;

		public MedicalCareCategory medCare = MedicalCareCategory.NoMeds;

		public HostilityResponseMode hostilityResponse = HostilityResponseMode.Flee;

		public bool selfTend;

		public int displayOrder;

		public Pawn Master
		{
			get
			{
				return this.master;
			}
			set
			{
				if (this.master == value)
				{
					return;
				}
				if (value != null && !this.pawn.training.HasLearned(TrainableDefOf.Obedience))
				{
					Log.ErrorOnce("Attempted to set master for non-obedient pawn", 73908573, false);
					return;
				}
				bool flag = ThinkNode_ConditionalShouldFollowMaster.ShouldFollowMaster(this.pawn);
				this.master = value;
				if (this.pawn.Spawned && (flag || ThinkNode_ConditionalShouldFollowMaster.ShouldFollowMaster(this.pawn)))
				{
					this.pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
				}
			}
		}

		public Area EffectiveAreaRestrictionInPawnCurrentMap
		{
			get
			{
				if (this.areaAllowedInt != null && this.areaAllowedInt.Map != this.pawn.MapHeld)
				{
					return null;
				}
				return this.EffectiveAreaRestriction;
			}
		}

		public Area EffectiveAreaRestriction
		{
			get
			{
				if (!this.RespectsAllowedArea)
				{
					return null;
				}
				return this.areaAllowedInt;
			}
		}

		public Area AreaRestriction
		{
			get
			{
				return this.areaAllowedInt;
			}
			set
			{
				if (this.areaAllowedInt == value)
				{
					return;
				}
				this.areaAllowedInt = value;
				if (this.pawn.Spawned && value != null && value == this.EffectiveAreaRestrictionInPawnCurrentMap && value.TrueCount > 0 && !value[this.pawn.Position])
				{
					this.pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
				}
			}
		}

		public bool RespectsAllowedArea
		{
			get
			{
				return this.pawn.GetLord() == null && this.pawn.Faction == Faction.OfPlayer && this.pawn.HostFaction == null;
			}
		}

		public bool RespectsMaster
		{
			get
			{
				return this.Master != null && this.pawn.Faction == Faction.OfPlayer && this.Master.Faction == this.pawn.Faction;
			}
		}

		public Pawn RespectedMaster
		{
			get
			{
				return (!this.RespectsMaster) ? null : this.Master;
			}
		}

		public bool UsesConfigurableHostilityResponse
		{
			get
			{
				return this.pawn.IsColonist && this.pawn.HostFaction == null;
			}
		}

		public Pawn_PlayerSettings(Pawn pawn)
		{
			this.pawn = pawn;
			if (Current.ProgramState == ProgramState.Playing)
			{
				this.joinTick = Find.TickManager.TicksGame;
			}
			else
			{
				this.joinTick = 0;
			}
			this.Notify_FactionChanged();
		}

		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.joinTick, "joinTick", 0, false);
			Scribe_Values.Look<bool>(ref this.animalsReleased, "animalsReleased", false, false);
			Scribe_Values.Look<MedicalCareCategory>(ref this.medCare, "medCare", MedicalCareCategory.NoCare, false);
			Scribe_References.Look<Area>(ref this.areaAllowedInt, "areaAllowed", false);
			Scribe_References.Look<Pawn>(ref this.master, "master", false);
			Scribe_Values.Look<bool>(ref this.followDrafted, "followDrafted", false, false);
			Scribe_Values.Look<bool>(ref this.followFieldwork, "followFieldwork", false, false);
			Scribe_Values.Look<HostilityResponseMode>(ref this.hostilityResponse, "hostilityResponse", HostilityResponseMode.Flee, false);
			Scribe_Values.Look<bool>(ref this.selfTend, "selfTend", false, false);
			Scribe_Values.Look<int>(ref this.displayOrder, "displayOrder", 0, false);
		}

		[DebuggerHidden]
		public IEnumerable<Gizmo> GetGizmos()
		{
			if (this.pawn.Drafted)
			{
				int count = 0;
				bool anyCanRelease = false;
				foreach (Pawn current in PawnUtility.SpawnedMasteredPawns(this.pawn))
				{
					if (current.training.HasLearned(TrainableDefOf.Release))
					{
						anyCanRelease = true;
						if (ThinkNode_ConditionalShouldFollowMaster.ShouldFollowMaster(current))
						{
							count++;
						}
					}
				}
				if (anyCanRelease)
				{
					Command_Toggle c = new Command_Toggle();
					c.defaultLabel = "CommandReleaseAnimalsLabel".Translate() + ((count == 0) ? string.Empty : (" (" + count + ")"));
					c.defaultDesc = "CommandReleaseAnimalsDesc".Translate();
					c.icon = TexCommand.ReleaseAnimals;
					c.hotKey = KeyBindingDefOf.Misc7;
					c.isActive = (() => this.$this.animalsReleased);
					c.toggleAction = delegate
					{
						this.$this.animalsReleased = !this.$this.animalsReleased;
						if (this.$this.animalsReleased)
						{
							foreach (Pawn current2 in PawnUtility.SpawnedMasteredPawns(this.$this.pawn))
							{
								if (current2.caller != null)
								{
									current2.caller.Notify_Released();
								}
								current2.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
							}
						}
					};
					if (count == 0)
					{
						c.Disable("CommandReleaseAnimalsFail_NoAnimals".Translate());
					}
					yield return c;
				}
			}
		}

		public void Notify_FactionChanged()
		{
			this.ResetMedicalCare();
			this.areaAllowedInt = null;
		}

		public void Notify_MadePrisoner()
		{
			this.ResetMedicalCare();
		}

		public void ResetMedicalCare()
		{
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				return;
			}
			if (this.pawn.Faction == Faction.OfPlayer)
			{
				if (!this.pawn.RaceProps.Animal)
				{
					if (!this.pawn.IsPrisoner)
					{
						this.medCare = Find.PlaySettings.defaultCareForColonyHumanlike;
					}
					else
					{
						this.medCare = Find.PlaySettings.defaultCareForColonyPrisoner;
					}
				}
				else
				{
					this.medCare = Find.PlaySettings.defaultCareForColonyAnimal;
				}
			}
			else if (this.pawn.Faction == null && this.pawn.RaceProps.Animal)
			{
				this.medCare = Find.PlaySettings.defaultCareForNeutralAnimal;
			}
			else if (this.pawn.Faction == null || !this.pawn.Faction.HostileTo(Faction.OfPlayer))
			{
				this.medCare = Find.PlaySettings.defaultCareForNeutralFaction;
			}
			else
			{
				this.medCare = Find.PlaySettings.defaultCareForHostileFaction;
			}
		}

		public void Notify_AreaRemoved(Area area)
		{
			if (this.areaAllowedInt == area)
			{
				this.areaAllowedInt = null;
			}
		}
	}
}
