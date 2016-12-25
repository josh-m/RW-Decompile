using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ActiveLessonHandler : IExposable
	{
		private Lesson activeLesson;

		public Lesson Current
		{
			get
			{
				return this.activeLesson;
			}
		}

		public bool ActiveLessonVisible
		{
			get
			{
				return this.activeLesson != null && !Find.WindowStack.WindowsPreventDrawTutor;
			}
		}

		public void ExposeData()
		{
			Scribe_Deep.LookDeep<Lesson>(ref this.activeLesson, "activeLesson", new object[0]);
		}

		public void Activate(InstructionDef id)
		{
			Lesson_Instruction lesson_Instruction = this.activeLesson as Lesson_Instruction;
			if (lesson_Instruction != null && id == lesson_Instruction.def)
			{
				return;
			}
			Lesson_Instruction lesson_Instruction2 = (Lesson_Instruction)Activator.CreateInstance(id.instructionClass);
			lesson_Instruction2.def = id;
			this.activeLesson = lesson_Instruction2;
			this.activeLesson.OnActivated();
		}

		public void Activate(Lesson lesson)
		{
			Lesson_Note lesson_Note = lesson as Lesson_Note;
			if (lesson_Note != null && this.activeLesson != null)
			{
				lesson_Note.doFadeIn = false;
			}
			this.activeLesson = lesson;
			this.activeLesson.OnActivated();
		}

		public void Deactivate()
		{
			Lesson lesson = this.activeLesson;
			this.activeLesson = null;
			if (lesson != null)
			{
				lesson.PostDeactivated();
			}
		}

		public void ActiveLessonOnGUI()
		{
			if (Time.timeSinceLevelLoad < 0.01f || !this.ActiveLessonVisible)
			{
				return;
			}
			this.activeLesson.LessonOnGUI();
		}

		public void ActiveLessonUpdate()
		{
			if (Time.timeSinceLevelLoad < 0.01f || !this.ActiveLessonVisible)
			{
				return;
			}
			this.activeLesson.LessonUpdate();
		}

		public void Notify_KnowledgeDemonstrated(ConceptDef conc)
		{
			if (this.Current != null)
			{
				this.Current.Notify_KnowledgeDemonstrated(conc);
			}
		}
	}
}
