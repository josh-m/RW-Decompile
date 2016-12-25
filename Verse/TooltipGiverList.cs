using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public sealed class TooltipGiverList
	{
		private List<Thing> givers = new List<Thing>();

		public void RegisterTooltipGiver(Thing t)
		{
			if (!t.def.hasTooltip)
			{
				Log.Warning("Tried to register non tooltip giver " + t + " to give tooltips.");
				return;
			}
			this.givers.Add(t);
		}

		public void DeregisterTooltipGiver(Thing t)
		{
			if (this.givers.Contains(t))
			{
				this.givers.Remove(t);
			}
			else
			{
				Log.Warning("Tried to remove non-registered tooltip giver " + t);
			}
		}

		public void DispenseAllThingTooltips()
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			if (Find.WindowStack.FloatMenu != null)
			{
				return;
			}
			CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
			float cellSizePixels = Find.CameraDriver.CellSizePixels;
			Vector2 vector = new Vector2(cellSizePixels, cellSizePixels);
			Rect rect = new Rect(0f, 0f, vector.x, vector.y);
			int num = 0;
			for (int i = 0; i < this.givers.Count; i++)
			{
				Thing thing = this.givers[i];
				if (currentViewRect.Contains(thing.Position) && !thing.Position.Fogged(thing.Map))
				{
					Vector2 vector2 = thing.DrawPos.MapToUIPosition();
					rect.x = vector2.x - vector.x / 2f;
					rect.y = vector2.y - vector.y / 2f;
					if (rect.Contains(Event.current.mousePosition))
					{
						TipSignal tooltip = thing.GetTooltip();
						tooltip.text += TooltipUtility.ShotCalculationTipString(thing);
						TooltipHandler.TipRegion(rect, tooltip);
					}
					num++;
				}
			}
		}
	}
}
