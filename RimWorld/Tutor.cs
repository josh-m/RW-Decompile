using System;
using Verse;

namespace RimWorld
{
	public class Tutor : IExposable
	{
		public ActiveLessonHandler activeLesson = new ActiveLessonHandler();

		public LearningReadout learningReadout = new LearningReadout();

		public TutorialState tutorialState = new TutorialState();

		public void ExposeData()
		{
			Scribe_Deep.LookDeep<ActiveLessonHandler>(ref this.activeLesson, "activeLesson", new object[0]);
			Scribe_Deep.LookDeep<LearningReadout>(ref this.learningReadout, "learningReadout", new object[0]);
			Scribe_Deep.LookDeep<TutorialState>(ref this.tutorialState, "tutorialState", new object[0]);
		}

		internal void TutorUpdate()
		{
			this.activeLesson.ActiveLessonUpdate();
			this.learningReadout.LearningReadoutUpdate();
		}

		internal void TutorOnGUI()
		{
			this.activeLesson.ActiveLessonOnGUI();
			this.learningReadout.LearningReadoutOnGUI();
		}
	}
}
