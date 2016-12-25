using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class SimpleCurveView
	{
		private const float ResetZoomBuffer = 0.1f;

		public Rect rect;

		private Dictionary<object, float> debugInputValues = new Dictionary<object, float>();

		private static Rect identityRect = new Rect(0f, 0f, 1f, 1f);

		public IEnumerable<float> DebugInputValues
		{
			get
			{
				if (this.debugInputValues != null)
				{
					foreach (float val in this.debugInputValues.Values)
					{
						yield return val;
					}
				}
			}
		}

		public void SetDebugInput(object key, float value)
		{
			this.debugInputValues[key] = value;
		}

		public void ClearDebugInputFrom(object key)
		{
			if (this.debugInputValues.ContainsKey(key))
			{
				this.debugInputValues.Remove(key);
			}
		}

		public void SetViewRectAround(SimpleCurve curve)
		{
			if (!curve.AllPoints.Any<CurvePoint>())
			{
				this.rect = SimpleCurveView.identityRect;
				return;
			}
			this.rect.xMin = (from pt in curve.AllPoints
			select pt.loc.x).Min();
			this.rect.xMax = (from pt in curve.AllPoints
			select pt.loc.x).Max();
			this.rect.yMin = (from pt in curve.AllPoints
			select pt.loc.y).Min();
			this.rect.yMax = (from pt in curve.AllPoints
			select pt.loc.y).Max();
			if (Mathf.Approximately(this.rect.width, 0f))
			{
				this.rect.width = this.rect.xMin * 2f;
			}
			if (Mathf.Approximately(this.rect.height, 0f))
			{
				this.rect.height = this.rect.yMin * 2f;
			}
			if (Mathf.Approximately(this.rect.width, 0f))
			{
				this.rect.width = 1f;
			}
			if (Mathf.Approximately(this.rect.height, 0f))
			{
				this.rect.height = 1f;
			}
			float width = this.rect.width;
			float height = this.rect.height;
			this.rect.xMin = this.rect.xMin - width * 0.1f;
			this.rect.xMax = this.rect.xMax + width * 0.1f;
			this.rect.yMin = this.rect.yMin - height * 0.1f;
			this.rect.yMax = this.rect.yMax + height * 0.1f;
		}
	}
}
