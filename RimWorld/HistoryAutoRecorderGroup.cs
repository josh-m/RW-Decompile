using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorderGroup : IExposable
	{
		public HistoryAutoRecorderGroupDef def;

		public List<HistoryAutoRecorder> recorders;

		private List<SimpleCurveDrawInfo> curves;

		private int cachedGraphTickCount = -1;

		public HistoryAutoRecorderGroup()
		{
			this.recorders = new List<HistoryAutoRecorder>();
			this.curves = new List<SimpleCurveDrawInfo>();
		}

		public void CreateRecorders()
		{
			foreach (HistoryAutoRecorderDef current in this.def.historyAutoRecorderDefs)
			{
				HistoryAutoRecorder historyAutoRecorder = new HistoryAutoRecorder();
				historyAutoRecorder.def = current;
				this.recorders.Add(historyAutoRecorder);
			}
		}

		public float GetMaxDay()
		{
			float num = 0f;
			foreach (HistoryAutoRecorder current in this.recorders)
			{
				int count = current.records.Count;
				if (count != 0)
				{
					float num2 = (float)((count - 1) * current.def.recordTicksFrequency) / 60000f;
					if (num2 > num)
					{
						num = num2;
					}
				}
			}
			return num;
		}

		public void Tick()
		{
			for (int i = 0; i < this.recorders.Count; i++)
			{
				this.recorders[i].Tick();
			}
		}

		public void DrawGraph(Rect graphRect, Rect legendRect, Vector2 section, SimpleCurveDrawerStyle curveDrawerStyle, List<CurveMark> marks)
		{
			int ticksGame = Find.TickManager.TicksGame;
			if (ticksGame != this.cachedGraphTickCount)
			{
				this.cachedGraphTickCount = ticksGame;
				this.curves.Clear();
				foreach (HistoryAutoRecorder current in this.recorders)
				{
					SimpleCurveDrawInfo simpleCurveDrawInfo = new SimpleCurveDrawInfo();
					simpleCurveDrawInfo.color = current.def.graphColor;
					simpleCurveDrawInfo.label = current.def.LabelCap;
					simpleCurveDrawInfo.curve = new SimpleCurve();
					for (int i = 0; i < current.records.Count; i++)
					{
						simpleCurveDrawInfo.curve.Add(new CurvePoint((float)i * (float)current.def.recordTicksFrequency / 60000f, current.records[i]));
					}
					if (current.records.Count == 1)
					{
						simpleCurveDrawInfo.curve.Add(new CurvePoint(1.66666669E-05f, current.records[0]));
					}
					this.curves.Add(simpleCurveDrawInfo);
				}
			}
			if (Mathf.Approximately(section.x, section.y))
			{
				section.y += 1.66666669E-05f;
			}
			curveDrawerStyle.FixedSection = section;
			curveDrawerStyle.LabelY = this.def.graphLabelY;
			curveDrawerStyle.UseFixedScale = this.def.useFixedScale;
			curveDrawerStyle.FixedScale = this.def.fixedScale;
			SimpleCurveDrawer.DrawCurves(graphRect, this.curves, curveDrawerStyle, marks, legendRect);
			Text.Anchor = TextAnchor.UpperLeft;
		}

		public void ExposeData()
		{
			Scribe_Defs.LookDef<HistoryAutoRecorderGroupDef>(ref this.def, "def");
			Scribe_Collections.LookList<HistoryAutoRecorder>(ref this.recorders, "recorders", LookMode.Deep, new object[0]);
		}
	}
}
