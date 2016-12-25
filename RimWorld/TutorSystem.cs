using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class TutorSystem
	{
		public static bool TutorialMode
		{
			get
			{
				return Find.Storyteller != null && Find.Storyteller.def != null && Find.Storyteller.def.tutorialMode;
			}
		}

		public static bool AdaptiveTrainingEnabled
		{
			get
			{
				return Prefs.AdaptiveTrainingEnabled && (Find.Storyteller == null || Find.Storyteller.def == null || !Find.Storyteller.def.disableAdaptiveTraining);
			}
		}

		public static void Notify_Event(string eventTag, IntVec3 cell)
		{
			TutorSystem.Notify_Event(new EventPack(eventTag, cell));
		}

		public static void Notify_Event(EventPack ep)
		{
			if (!TutorSystem.TutorialMode)
			{
				return;
			}
			if (DebugViewSettings.logTutor)
			{
				Log.Message("Notify_Event: " + ep);
			}
			if (Current.Game == null)
			{
				return;
			}
			if (Find.ActiveLesson.Current != null)
			{
				Find.ActiveLesson.Current.Notify_Event(ep);
			}
			foreach (InstructionDef current in DefDatabase<InstructionDef>.AllDefs)
			{
				if (current.eventTagInitiate == ep.Tag && (TutorSystem.TutorialMode || !current.tutorialModeOnly))
				{
					Find.ActiveLesson.Activate(current);
					break;
				}
			}
		}

		public static bool AllowAction(EventPack ep)
		{
			if (!TutorSystem.TutorialMode)
			{
				return true;
			}
			if (DebugViewSettings.logTutor)
			{
				Log.Message("AllowAction: " + ep);
			}
			if (ep.Cells != null && ep.Cells.Count<IntVec3>() == 1)
			{
				return TutorSystem.AllowAction(new EventPack(ep.Tag, ep.Cells.First<IntVec3>()));
			}
			if (Find.ActiveLesson.Current != null)
			{
				AcceptanceReport acceptanceReport = Find.ActiveLesson.Current.AllowAction(ep);
				if (!acceptanceReport.Accepted)
				{
					string text = acceptanceReport.Reason.NullOrEmpty() ? Find.ActiveLesson.Current.DefaultRejectInputMessage : acceptanceReport.Reason;
					Messages.Message(text, MessageSound.RejectInput);
					return false;
				}
			}
			return true;
		}
	}
}
