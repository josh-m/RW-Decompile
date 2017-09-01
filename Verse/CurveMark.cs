using System;
using UnityEngine;

namespace Verse
{
	public struct CurveMark
	{
		private float x;

		private string message;

		private Color color;

		public float X
		{
			get
			{
				return this.x;
			}
		}

		public string Message
		{
			get
			{
				return this.message;
			}
		}

		public Color Color
		{
			get
			{
				return this.color;
			}
		}

		public CurveMark(float x, string message, Color color)
		{
			this.x = x;
			this.message = message;
			this.color = color;
		}
	}
}
