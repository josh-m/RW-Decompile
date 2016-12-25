using System;
using System.Collections.Generic;

namespace Verse
{
	public class FeedbackFloaters
	{
		protected List<FeedbackItem> feeders = new List<FeedbackItem>();

		public void AddFeedback(FeedbackItem newFeedback)
		{
			this.feeders.Add(newFeedback);
		}

		public void FeedbackUpdate()
		{
			for (int i = this.feeders.Count - 1; i >= 0; i--)
			{
				this.feeders[i].Update();
				if (this.feeders[i].TimeLeft <= 0f)
				{
					this.feeders.Remove(this.feeders[i]);
				}
			}
		}

		public void FeedbackOnGUI()
		{
			foreach (FeedbackItem current in this.feeders)
			{
				current.FeedbackOnGUI();
			}
		}
	}
}
