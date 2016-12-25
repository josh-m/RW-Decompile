using System;
using UnityEngine;

namespace Verse
{
	public class SimpleCurveDrawerStyle
	{
		public bool DrawBackground
		{
			get;
			set;
		}

		public bool DrawBackgroundLines
		{
			get;
			set;
		}

		public bool DrawMeasures
		{
			get;
			set;
		}

		public bool DrawPoints
		{
			get;
			set;
		}

		public bool DrawLegend
		{
			get;
			set;
		}

		public bool DrawCurveMousePoint
		{
			get;
			set;
		}

		public bool OnlyPositiveValues
		{
			get;
			set;
		}

		public bool UseFixedSection
		{
			get;
			set;
		}

		public bool UseFixedScale
		{
			get;
			set;
		}

		public bool UseAntiAliasedLines
		{
			get;
			set;
		}

		public bool PointsRemoveOptimization
		{
			get;
			set;
		}

		public int MeasureLabelsXCount
		{
			get;
			set;
		}

		public int MeasureLabelsYCount
		{
			get;
			set;
		}

		public string LabelX
		{
			get;
			set;
		}

		public string LabelY
		{
			get;
			set;
		}

		public Vector2 FixedSection
		{
			get;
			set;
		}

		public Vector2 FixedScale
		{
			get;
			set;
		}

		public SimpleCurveDrawerStyle()
		{
			this.DrawBackground = false;
			this.DrawBackgroundLines = true;
			this.DrawMeasures = false;
			this.DrawPoints = true;
			this.DrawLegend = false;
			this.DrawCurveMousePoint = false;
			this.OnlyPositiveValues = false;
			this.UseFixedSection = false;
			this.UseFixedScale = false;
			this.UseAntiAliasedLines = false;
			this.PointsRemoveOptimization = false;
			this.MeasureLabelsXCount = 5;
			this.MeasureLabelsYCount = 5;
			this.LabelX = "x";
			this.LabelY = "y";
		}
	}
}
