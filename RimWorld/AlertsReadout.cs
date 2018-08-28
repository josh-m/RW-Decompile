using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class AlertsReadout
	{
		private List<Alert> activeAlerts = new List<Alert>(16);

		private int curAlertIndex;

		private float lastFinalY;

		private int mouseoverAlertIndex = -1;

		private readonly List<Alert> AllAlerts = new List<Alert>();

		private const int StartTickDelay = 600;

		public const float AlertListWidth = 164f;

		private static int AlertCycleLength = 20;

		private readonly List<AlertPriority> PriosInDrawOrder;

		public AlertsReadout()
		{
			this.AllAlerts.Clear();
			foreach (Type current in typeof(Alert).AllLeafSubclasses())
			{
				this.AllAlerts.Add((Alert)Activator.CreateInstance(current));
			}
			if (this.PriosInDrawOrder == null)
			{
				this.PriosInDrawOrder = new List<AlertPriority>();
				foreach (AlertPriority item in Enum.GetValues(typeof(AlertPriority)))
				{
					this.PriosInDrawOrder.Add(item);
				}
				this.PriosInDrawOrder.Reverse();
			}
		}

		public void AlertsReadoutUpdate()
		{
			if (Mathf.Max(Find.TickManager.TicksGame, Find.TutorialState.endTick) < 600)
			{
				return;
			}
			if (Find.Storyteller.def.disableAlerts)
			{
				this.activeAlerts.Clear();
				return;
			}
			this.curAlertIndex++;
			if (this.curAlertIndex >= AlertsReadout.AlertCycleLength)
			{
				this.curAlertIndex = 0;
			}
			for (int i = this.curAlertIndex; i < this.AllAlerts.Count; i += AlertsReadout.AlertCycleLength)
			{
				Alert alert = this.AllAlerts[i];
				try
				{
					if (alert.Active)
					{
						if (!this.activeAlerts.Contains(alert))
						{
							this.activeAlerts.Add(alert);
							alert.Notify_Started();
						}
					}
					else
					{
						for (int j = 0; j < this.activeAlerts.Count; j++)
						{
							if (this.activeAlerts[j] == alert)
							{
								this.activeAlerts.RemoveAt(j);
								break;
							}
						}
					}
				}
				catch (Exception ex)
				{
					Log.ErrorOnce("Exception processing alert " + alert.ToString() + ": " + ex.ToString(), 743575, false);
					if (this.activeAlerts.Contains(alert))
					{
						this.activeAlerts.Remove(alert);
					}
				}
			}
			for (int k = this.activeAlerts.Count - 1; k >= 0; k--)
			{
				Alert alert2 = this.activeAlerts[k];
				try
				{
					this.activeAlerts[k].AlertActiveUpdate();
				}
				catch (Exception ex2)
				{
					Log.ErrorOnce("Exception updating alert " + alert2.ToString() + ": " + ex2.ToString(), 743575, false);
					this.activeAlerts.RemoveAt(k);
				}
			}
			if (this.mouseoverAlertIndex >= 0 && this.mouseoverAlertIndex < this.activeAlerts.Count)
			{
				IEnumerable<GlobalTargetInfo> culprits = this.activeAlerts[this.mouseoverAlertIndex].GetReport().culprits;
				if (culprits != null)
				{
					foreach (GlobalTargetInfo current in culprits)
					{
						TargetHighlighter.Highlight(current, true, true, false);
					}
				}
			}
			this.mouseoverAlertIndex = -1;
		}

		public void AlertsReadoutOnGUI()
		{
			if (Event.current.type == EventType.Layout || Event.current.type == EventType.MouseDrag)
			{
				return;
			}
			if (this.activeAlerts.Count == 0)
			{
				return;
			}
			Alert alert = null;
			AlertPriority alertPriority = AlertPriority.Critical;
			bool flag = false;
			float num = Find.LetterStack.LastTopY - (float)this.activeAlerts.Count * 28f;
			Rect rect = new Rect((float)UI.screenWidth - 154f, num, 154f, this.lastFinalY - num);
			float num2 = GenUI.BackgroundDarkAlphaForText();
			if (num2 > 0.001f)
			{
				GUI.color = new Color(1f, 1f, 1f, num2);
				Widgets.DrawShadowAround(rect);
				GUI.color = Color.white;
			}
			float num3 = num;
			if (num3 < 0f)
			{
				num3 = 0f;
			}
			for (int i = 0; i < this.PriosInDrawOrder.Count; i++)
			{
				AlertPriority alertPriority2 = this.PriosInDrawOrder[i];
				for (int j = 0; j < this.activeAlerts.Count; j++)
				{
					Alert alert2 = this.activeAlerts[j];
					if (alert2.Priority == alertPriority2)
					{
						if (!flag)
						{
							alertPriority = alertPriority2;
							flag = true;
						}
						Rect rect2 = alert2.DrawAt(num3, alertPriority2 != alertPriority);
						if (Mouse.IsOver(rect2))
						{
							alert = alert2;
							this.mouseoverAlertIndex = j;
						}
						num3 += rect2.height;
					}
				}
			}
			this.lastFinalY = num3;
			UIHighlighter.HighlightOpportunity(rect, "Alerts");
			if (alert != null)
			{
				alert.DrawInfoPane();
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Alerts, KnowledgeAmount.FrameDisplayed);
			}
		}
	}
}
