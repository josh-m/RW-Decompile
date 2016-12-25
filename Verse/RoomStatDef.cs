using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class RoomStatDef : Def
	{
		public Type workerClass;

		public float updatePriority;

		public bool displayRounded;

		public bool isHidden;

		public float defaultScore;

		public List<RoomStatScoreStage> scoreStages;

		[Unsaved]
		private RoomStatWorker workerInt;

		public RoomStatWorker Worker
		{
			get
			{
				if (this.workerInt == null)
				{
					this.workerInt = (RoomStatWorker)Activator.CreateInstance(this.workerClass);
				}
				return this.workerInt;
			}
		}

		public RoomStatScoreStage GetScoreStage(float score)
		{
			if (this.scoreStages.NullOrEmpty<RoomStatScoreStage>())
			{
				return null;
			}
			return this.scoreStages[this.GetScoreStageIndex(score)];
		}

		public int GetScoreStageIndex(float score)
		{
			if (this.scoreStages.NullOrEmpty<RoomStatScoreStage>())
			{
				throw new InvalidOperationException("No score stages available.");
			}
			int result = 0;
			for (int i = 0; i < this.scoreStages.Count; i++)
			{
				if (score < this.scoreStages[i].minScore)
				{
					break;
				}
				result = i;
			}
			return result;
		}

		public string ScoreToString(float score)
		{
			if (this.displayRounded)
			{
				return Mathf.RoundToInt(score).ToString();
			}
			return score.ToString("F2");
		}
	}
}
