using System;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StatWorker_ShootingAccuracy : StatWorker
	{
		public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(base.GetExplanationFinalizePart(req, numberSense, finalVal));
			stringBuilder.AppendLine();
			for (int i = 5; i <= 45; i += 5)
			{
				stringBuilder.AppendLine(string.Concat(new string[]
				{
					"distance".Translate().CapitalizeFirst(),
					" ",
					i.ToString(),
					": ",
					Mathf.Pow(finalVal, (float)i).ToStringPercent()
				}));
			}
			return stringBuilder.ToString();
		}
	}
}
