using System;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public static class JoyUtility
	{
		public const float BaseJoyGainPerHour = 0.36f;

		public static bool EnjoyableOutsideNow(Map map, StringBuilder outFailReason = null)
		{
			if (map.weatherManager.RainRate >= 0.25f)
			{
				if (outFailReason != null)
				{
					outFailReason.Append(map.weatherManager.curWeather.label);
				}
				return false;
			}
			MapConditionDef mapConditionDef;
			if (!map.mapConditionManager.AllowEnjoyableOutsideNow(out mapConditionDef))
			{
				if (outFailReason != null)
				{
					outFailReason.Append(mapConditionDef.label);
				}
				return false;
			}
			return true;
		}

		public static bool EnjoyableOutsideNow(Pawn pawn, StringBuilder outFailReason = null)
		{
			if (!JoyUtility.EnjoyableOutsideNow(pawn.Map, outFailReason))
			{
				return false;
			}
			if (!pawn.ComfortableTemperatureRange().Includes(pawn.Map.mapTemperature.OutdoorTemp))
			{
				if (outFailReason != null)
				{
					outFailReason.Append("NotEnjoyableOutsideTemperature".Translate());
				}
				return false;
			}
			return true;
		}

		public static void JoyTickCheckEnd(Pawn pawn, JoyTickFullJoyAction fullJoyAction = JoyTickFullJoyAction.EndJob, float extraJoyGainFactor = 1f)
		{
			Job curJob = pawn.CurJob;
			if (curJob.def.joyKind == null)
			{
				Log.Warning("This method can only be called for jobs with joyKind.");
				return;
			}
			pawn.needs.joy.GainJoy(extraJoyGainFactor * curJob.def.joyGainRate * 0.000144f, curJob.def.joyKind);
			if (curJob.def.joySkill != null)
			{
				pawn.skills.GetSkill(curJob.def.joySkill).Learn(curJob.def.joyXpPerTick, false);
			}
			if (!curJob.ignoreJoyTimeAssignment && !pawn.GetTimeAssignment().allowJoy)
			{
				pawn.jobs.curDriver.EndJobWith(JobCondition.InterruptForced);
			}
			if (pawn.needs.joy.CurLevel > 0.9999f)
			{
				if (fullJoyAction == JoyTickFullJoyAction.EndJob)
				{
					pawn.jobs.curDriver.EndJobWith(JobCondition.Succeeded);
				}
				else if (fullJoyAction == JoyTickFullJoyAction.GoToNextToil)
				{
					pawn.jobs.curDriver.ReadyForNextToil();
				}
			}
		}

		public static void TryGainRecRoomThought(Pawn pawn)
		{
			Room room = pawn.GetRoom();
			if (room != null)
			{
				int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
				if (ThoughtDefOf.AteInImpressiveDiningRoom.stages[scoreStageIndex] != null)
				{
					pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtMaker.MakeThought(ThoughtDefOf.JoyActivityInImpressiveRecRoom, scoreStageIndex), null);
				}
			}
		}

		public static bool LordPreventsGettingJoy(Pawn pawn)
		{
			Lord lord = pawn.GetLord();
			return lord != null && !lord.CurLordToil.AllowSatisfyLongNeeds;
		}

		public static bool TimetablePreventsGettingJoy(Pawn pawn)
		{
			TimeAssignmentDef timeAssignmentDef = (pawn.timetable != null) ? pawn.timetable.CurrentAssignment : TimeAssignmentDefOf.Anything;
			return !timeAssignmentDef.allowJoy;
		}
	}
}
