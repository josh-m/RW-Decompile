using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldView
	{
		private const float WorldRectWidthMin = 50f;

		public Rect screenRect;

		public Vector2 worldRectCenter;

		public float worldRectWidth;

		public float ViewedWorldXMax
		{
			get
			{
				return this.worldRectCenter.x + this.worldRectWidth / 2f;
			}
		}

		public float ViewedWorldXMin
		{
			get
			{
				return this.worldRectCenter.x - this.worldRectWidth / 2f;
			}
		}

		public float ViewedWorldZMax
		{
			get
			{
				return this.worldRectCenter.y + this.WorldRectHeight / 2f;
			}
		}

		public float ViewedWorldZMin
		{
			get
			{
				return this.worldRectCenter.y - this.WorldRectHeight / 2f;
			}
		}

		public float PixelsPerWorldSquare
		{
			get
			{
				return this.screenRect.width / this.worldRectWidth;
			}
		}

		public float WorldRectHeight
		{
			get
			{
				return this.worldRectWidth * (this.screenRect.height / this.screenRect.width);
			}
			set
			{
				this.worldRectWidth = value * (this.screenRect.width / this.screenRect.height);
			}
		}

		public Rect NormalizedTexViewCoords
		{
			get
			{
				Rect result = default(Rect);
				result.width = this.worldRectWidth / (float)this.WorldSize.x;
				result.height = this.screenRect.height / this.screenRect.width * result.width;
				float num = result.width / this.worldRectWidth;
				Vector2 vector = this.worldRectCenter * num;
				result.x = vector.x - this.worldRectWidth / 2f * num;
				result.y = vector.y - this.WorldRectHeight / 2f * num;
				return result;
			}
		}

		private IntVec2 WorldSize
		{
			get
			{
				return Find.World.Size;
			}
		}

		private Vector2 WholeWorldScreenSize
		{
			get
			{
				return new Vector2((float)this.WorldSize.x * this.PixelsPerWorldSquare, (float)this.WorldSize.z * this.PixelsPerWorldSquare);
			}
		}

		public WorldView(Rect outRect, Vector2 viewCenter, float viewWidth)
		{
			this.screenRect = outRect;
			this.worldRectCenter = viewCenter;
			this.worldRectWidth = viewWidth;
			this.ClampViewIntoWorldBounds();
		}

		public Vector2 ScreenLocOf(IntVec2 worldSquare)
		{
			float num = (float)worldSquare.x;
			float num2 = (float)worldSquare.z;
			num -= this.ViewedWorldXMin;
			num2 -= this.ViewedWorldZMin;
			num *= this.PixelsPerWorldSquare;
			num2 *= this.PixelsPerWorldSquare;
			num2 = this.screenRect.height - num2;
			num += this.screenRect.x;
			num2 += this.screenRect.y;
			num += this.PixelsPerWorldSquare * 0.5f;
			num2 -= this.PixelsPerWorldSquare * 0.5f;
			return new Vector2(num, num2);
		}

		public IntVec2 WorldSquareAt(Vector2 screenLoc)
		{
			float num = screenLoc.x;
			float num2 = screenLoc.y;
			num -= this.screenRect.x;
			num2 -= this.screenRect.y;
			num2 = this.screenRect.height - num2;
			num /= this.PixelsPerWorldSquare;
			num2 /= this.PixelsPerWorldSquare;
			num += this.ViewedWorldXMin;
			num2 += this.ViewedWorldZMin;
			return new IntVec2(Mathf.FloorToInt(num), Mathf.FloorToInt(num2));
		}

		public void TryDolly(Vector2 offset)
		{
			offset.x *= -1f;
			this.worldRectCenter += offset;
			this.ClampViewIntoWorldBounds();
		}

		public void TryZoom(float amount)
		{
			this.worldRectWidth += amount;
			this.ClampViewIntoWorldBounds();
		}

		private void ClampViewIntoWorldBounds()
		{
			if (this.worldRectWidth < 50f)
			{
				this.worldRectWidth = 50f;
			}
			if (this.worldRectWidth > (float)this.WorldSize.x)
			{
				this.worldRectWidth = (float)this.WorldSize.x;
			}
			if (this.WorldRectHeight > (float)this.WorldSize.z)
			{
				this.WorldRectHeight = (float)this.WorldSize.z;
			}
			float num = this.ViewedWorldXMax - (float)this.WorldSize.x;
			if (num > 0f)
			{
				this.worldRectCenter.x = this.worldRectCenter.x - num;
			}
			float num2 = -this.ViewedWorldXMin;
			if (num2 > 0f)
			{
				this.worldRectCenter.x = this.worldRectCenter.x + num2;
			}
			float num3 = this.ViewedWorldZMax - (float)this.WorldSize.z;
			if (num3 > 0f)
			{
				this.worldRectCenter.y = this.worldRectCenter.y - num3;
			}
			float num4 = -this.ViewedWorldZMin;
			if (num4 > 0f)
			{
				this.worldRectCenter.y = this.worldRectCenter.y + num4;
			}
		}
	}
}
