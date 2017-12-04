using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_Party : LordToil
	{
		private IntVec3 spot;

		private int ticksPerPartyPulse = 600;

		private const int DefaultTicksPerPartyPulse = 600;

		private LordToilData_Party Data
		{
			get
			{
				return (LordToilData_Party)this.data;
			}
		}

		public LordToil_Party(IntVec3 spot, int ticksPerPartyPulse = 600)
		{
			this.spot = spot;
			this.ticksPerPartyPulse = ticksPerPartyPulse;
			this.data = new LordToilData_Party();
			this.Data.ticksToNextPulse = ticksPerPartyPulse;
		}

		public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p)
		{
			return DutyDefOf.Party.hook;
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				this.lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.Party, this.spot, -1f);
			}
		}

		public override void LordToilTick()
		{
			if (--this.Data.ticksToNextPulse <= 0)
			{
				this.Data.ticksToNextPulse = this.ticksPerPartyPulse;
				List<Pawn> ownedPawns = this.lord.ownedPawns;
				for (int i = 0; i < ownedPawns.Count; i++)
				{
					if (PartyUtility.InPartyArea(ownedPawns[i].Position, this.spot, base.Map))
					{
						ownedPawns[i].needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.AttendedParty, null);
						LordJob_Joinable_Party lordJob_Joinable_Party = this.lord.LordJob as LordJob_Joinable_Party;
						if (lordJob_Joinable_Party != null)
						{
							TaleRecorder.RecordTale(TaleDefOf.AttendedParty, new object[]
							{
								ownedPawns[i],
								lordJob_Joinable_Party.Organizer
							});
						}
					}
				}
			}
		}
	}
}
