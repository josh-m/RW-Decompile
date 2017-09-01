using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

		public Pawn master;

		public bool followDrafted = true;

		public bool followFieldwork = true;

		public bool animalsReleased;

		public MedicalCareCategory medCare = MedicalCareCategory.NoMeds;

		public HostilityResponseMode hostilityResponse = HostilityResponseMode.Flee;

		public bool selfTend;

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
				this.areaAllowedInt = value;
			}
		}

		public bool RespectsAllowedArea
		{
			get
			{
				return this.pawn.GetLord() == null && this.pawn.Faction == Faction.OfPlayer && this.pawn.HostFaction == null;
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
		}

		[DebuggerHidden]
		public IEnumerable<Gizmo> GetGizmos()
		{
			if (this.pawn.Drafted)
			{
				if (PawnUtility.SpawnedMasteredPawns(this.pawn).Any((Pawn p) => p.training.IsCompleted(TrainableDefOf.Release)))
				{
					yield return new Command_Toggle
					{
						defaultLabel = "CommandReleaseAnimalsLabel".Translate(),
						defaultDesc = "CommandReleaseAnimalsDesc".Translate(),
						icon = TexCommand.ReleaseAnimals,
						hotKey = KeyBindingDefOf.Misc7,
						isActive = (() => this.<>f__this.animalsReleased),
						toggleAction = delegate
						{
							this.<>f__this.animalsReleased = !this.<>f__this.animalsReleased;
							if (this.<>f__this.animalsReleased)
							{
								foreach (Pawn current in PawnUtility.SpawnedMasteredPawns(this.<>f__this.pawn))
								{
									if (current.caller != null)
									{
										current.caller.Notify_Released();
									}
									current.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
								}
							}
						}
					};
				}
			}
		}

		public void Notify_FactionChanged()
		{
			this.ResetMedicalCare();
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
						this.medCare = Find.World.settings.defaultCareForColonyHumanlike;
					}
					else
					{
						this.medCare = Find.World.settings.defaultCareForColonyPrisoner;
					}
				}
				else
				{
					this.medCare = Find.World.settings.defaultCareForColonyAnimal;
				}
			}
			else if (this.pawn.Faction == null && this.pawn.RaceProps.Animal)
			{
				this.medCare = Find.World.settings.defaultCareForNeutralAnimal;
			}
			else if (this.pawn.Faction == null || !this.pawn.Faction.HostileTo(Faction.OfPlayer))
			{
				this.medCare = Find.World.settings.defaultCareForNeutralFaction;
			}
			else
			{
				this.medCare = Find.World.settings.defaultCareForHostileFaction;
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
