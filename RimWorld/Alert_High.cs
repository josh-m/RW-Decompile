using System;
using UnityEngine;

namespace RimWorld
{
	public abstract class Alert_High : Alert
	{
		public override AlertPriority Priority
		{
			get
			{
				return AlertPriority.High;
			}
		}

		public override Color ArrowColor
		{
			get
			{
				return Color.white;
			}
		}
	}
}
