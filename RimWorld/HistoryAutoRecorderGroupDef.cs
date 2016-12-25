using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class HistoryAutoRecorderGroupDef : Def
	{
		public string graphLabelY = "Value";

		public bool useFixedScale;

		public Vector2 fixedScale = default(Vector2);

		public List<HistoryAutoRecorderDef> historyAutoRecorderDefs = new List<HistoryAutoRecorderDef>();

		public static HistoryAutoRecorderGroupDef Named(string defName)
		{
			return DefDatabase<HistoryAutoRecorderGroupDef>.GetNamed(defName, true);
		}
	}
}
