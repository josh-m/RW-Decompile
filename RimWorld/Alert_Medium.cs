using System;
using UnityEngine;

namespace RimWorld
{
	public abstract class Alert_Medium : Alert
	{
		public override AlertPriority Priority
		{
			get
			{
				return AlertPriority.Medium;
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
