using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Verse.AI.Group
{
	public sealed class LordManager : IExposable
	{
		public Map map;

		public List<Lord> lords = new List<Lord>();

		public LordManager(Map map)
		{
			this.map = map;
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<Lord>(ref this.lords, "lords", LookMode.Deep, new object[0]);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				for (int i = 0; i < this.lords.Count; i++)
				{
					this.lords[i].lordManager = this;
				}
			}
		}

		public void LordManagerTick()
		{
			for (int i = 0; i < this.lords.Count; i++)
			{
				this.lords[i].LordTick();
			}
			for (int j = this.lords.Count - 1; j >= 0; j--)
			{
				LordToil curLordToil = this.lords[j].CurLordToil;
				if (curLordToil.ShouldFail)
				{
					this.RemoveLord(this.lords[j]);
				}
			}
		}

		public void LordManagerUpdate()
		{
			if (DebugViewSettings.drawLords)
			{
				for (int i = 0; i < this.lords.Count; i++)
				{
					this.lords[i].DebugDraw();
				}
			}
		}

		public void LordManagerOnGUI()
		{
			if (DebugViewSettings.drawLords)
			{
				for (int i = 0; i < this.lords.Count; i++)
				{
					this.lords[i].DebugOnGUI();
				}
			}
			if (DebugViewSettings.drawDuties)
			{
				Text.Anchor = TextAnchor.MiddleCenter;
				Text.Font = GameFont.Tiny;
				foreach (Pawn current in this.map.mapPawns.AllPawns)
				{
					if (current.Spawned)
					{
						string text = string.Empty;
						if (!current.Dead && current.mindState.duty != null)
						{
							text = current.mindState.duty.ToString();
						}
						if (current.InMentalState)
						{
							text = text + "\nMentalState=" + current.MentalState.ToString();
						}
						Vector2 vector = current.DrawPos.MapToUIPosition();
						Widgets.Label(new Rect(vector.x - 100f, vector.y - 100f, 200f, 200f), text);
					}
				}
				Text.Anchor = TextAnchor.UpperLeft;
			}
		}

		public void AddLord(Lord newLord)
		{
			this.lords.Add(newLord);
			newLord.lordManager = this;
		}

		public void RemoveLord(Lord oldLord)
		{
			this.lords.Remove(oldLord);
			oldLord.Cleanup();
		}

		public Lord LordOf(Pawn p)
		{
			for (int i = 0; i < this.lords.Count; i++)
			{
				Lord lord = this.lords[i];
				for (int j = 0; j < lord.ownedPawns.Count; j++)
				{
					if (lord.ownedPawns[j] == p)
					{
						return lord;
					}
				}
			}
			return null;
		}

		public void LogLords()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("======= Lords =======");
			stringBuilder.AppendLine("Count: " + this.lords.Count);
			for (int i = 0; i < this.lords.Count; i++)
			{
				Lord lord = this.lords[i];
				stringBuilder.AppendLine();
				stringBuilder.Append("#" + (i + 1) + ": ");
				if (lord.LordJob == null)
				{
					stringBuilder.AppendLine("no-job");
				}
				else
				{
					stringBuilder.AppendLine(lord.LordJob.GetType().Name);
				}
				stringBuilder.Append("Current toil: ");
				if (lord.CurLordToil == null)
				{
					stringBuilder.AppendLine("null");
				}
				else
				{
					stringBuilder.AppendLine(lord.CurLordToil.GetType().Name);
				}
				stringBuilder.AppendLine("Members (count: " + lord.ownedPawns.Count + "):");
				for (int j = 0; j < lord.ownedPawns.Count; j++)
				{
					stringBuilder.AppendLine(string.Concat(new object[]
					{
						"  ",
						lord.ownedPawns[j].LabelShort,
						" (",
						lord.ownedPawns[j].Faction,
						")"
					}));
				}
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
