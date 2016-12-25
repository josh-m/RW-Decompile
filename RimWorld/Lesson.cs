using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class Lesson : IExposable
	{
		public const float KnowledgeForAutoVanish = 0.2f;

		public float startRealTime = -999f;

		protected float AgeSeconds
		{
			get
			{
				if (this.startRealTime < 0f)
				{
					this.startRealTime = Time.realtimeSinceStartup;
				}
				return Time.realtimeSinceStartup - this.startRealTime;
			}
		}

		public virtual ConceptDef Concept
		{
			get
			{
				return null;
			}
		}

		public virtual float MessagesYOffset
		{
			get
			{
				return 0f;
			}
		}

		public virtual string DefaultRejectInputMessage
		{
			get
			{
				return null;
			}
		}

		public virtual void ExposeData()
		{
		}

		public virtual void OnActivated()
		{
			this.startRealTime = Time.realtimeSinceStartup;
		}

		public virtual void PostDeactivated()
		{
		}

		public abstract void LessonOnGUI();

		public virtual void LessonUpdate()
		{
		}

		public virtual void Notify_KnowledgeDemonstrated(ConceptDef conc)
		{
		}

		public virtual void Notify_Event(EventPack ep)
		{
		}

		public virtual AcceptanceReport AllowAction(EventPack ep)
		{
			return true;
		}
	}
}
