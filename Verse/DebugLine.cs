using System;
using UnityEngine;

namespace Verse
{
	internal struct DebugLine
	{
		public Vector3 a;

		public Vector3 b;

		private int ticksLeft;

		public int TicksLeft
		{
			get
			{
				return this.ticksLeft;
			}
			set
			{
				this.ticksLeft = value;
			}
		}

		public DebugLine(Vector3 a, Vector3 b)
		{
			this.a = a;
			this.b = b;
			this.ticksLeft = 100;
		}

		public DebugLine(Vector3 a, Vector3 b, int ticksLeft)
		{
			this.a = a;
			this.b = b;
			this.ticksLeft = ticksLeft;
		}

		public void Draw()
		{
			GenDraw.DrawLineBetween(this.a, this.b);
		}
	}
}
