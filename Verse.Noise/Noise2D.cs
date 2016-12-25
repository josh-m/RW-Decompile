using System;
using System.Xml.Serialization;
using UnityEngine;

namespace Verse.Noise
{
	public class Noise2D : IDisposable
	{
		public static readonly double South = -90.0;

		public static readonly double North = 90.0;

		public static readonly double West = -180.0;

		public static readonly double East = 180.0;

		public static readonly double AngleMin = -180.0;

		public static readonly double AngleMax = 180.0;

		public static readonly double Left = -1.0;

		public static readonly double Right = 1.0;

		public static readonly double Top = -1.0;

		public static readonly double Bottom = 1.0;

		private int m_width;

		private int m_height;

		private float[,] m_data;

		private int m_ucWidth;

		private int m_ucHeight;

		private int m_ucBorder = 1;

		private float[,] m_ucData;

		private float m_borderValue = float.NaN;

		private ModuleBase m_generator;

		[XmlIgnore]
		[NonSerialized]
		private bool m_disposed;

		public float this[int x, int y, bool isCropped = true]
		{
			get
			{
				if (isCropped)
				{
					if (x < 0 && x >= this.m_width)
					{
						throw new ArgumentOutOfRangeException("Invalid x position");
					}
					if (y < 0 && y >= this.m_height)
					{
						throw new ArgumentOutOfRangeException("Inavlid y position");
					}
					return this.m_data[x, y];
				}
				else
				{
					if (x < 0 && x >= this.m_ucWidth)
					{
						throw new ArgumentOutOfRangeException("Invalid x position");
					}
					if (y < 0 && y >= this.m_ucHeight)
					{
						throw new ArgumentOutOfRangeException("Inavlid y position");
					}
					return this.m_ucData[x, y];
				}
			}
			set
			{
				if (isCropped)
				{
					if (x < 0 && x >= this.m_width)
					{
						throw new ArgumentOutOfRangeException("Invalid x position");
					}
					if (y < 0 && y >= this.m_height)
					{
						throw new ArgumentOutOfRangeException("Invalid y position");
					}
					this.m_data[x, y] = value;
				}
				else
				{
					if (x < 0 && x >= this.m_ucWidth)
					{
						throw new ArgumentOutOfRangeException("Invalid x position");
					}
					if (y < 0 && y >= this.m_ucHeight)
					{
						throw new ArgumentOutOfRangeException("Inavlid y position");
					}
					this.m_ucData[x, y] = value;
				}
			}
		}

		public float Border
		{
			get
			{
				return this.m_borderValue;
			}
			set
			{
				this.m_borderValue = value;
			}
		}

		public ModuleBase Generator
		{
			get
			{
				return this.m_generator;
			}
			set
			{
				this.m_generator = value;
			}
		}

		public int Height
		{
			get
			{
				return this.m_height;
			}
		}

		public int Width
		{
			get
			{
				return this.m_width;
			}
		}

		public bool IsDisposed
		{
			get
			{
				return this.m_disposed;
			}
		}

		protected Noise2D()
		{
		}

		public Noise2D(int size) : this(size, size, null)
		{
		}

		public Noise2D(int size, ModuleBase generator) : this(size, size, generator)
		{
		}

		public Noise2D(int width, int height) : this(width, height, null)
		{
		}

		public Noise2D(int width, int height, ModuleBase generator)
		{
			this.m_generator = generator;
			this.m_width = width;
			this.m_height = height;
			this.m_data = new float[width, height];
			this.m_ucWidth = width + this.m_ucBorder * 2;
			this.m_ucHeight = height + this.m_ucBorder * 2;
			this.m_ucData = new float[width + this.m_ucBorder * 2, height + this.m_ucBorder * 2];
		}

		public float[,] GetNormalizedData(bool isCropped = true, int xCrop = 0, int yCrop = 0)
		{
			return this.GetData(isCropped, xCrop, yCrop, true);
		}

		public float[,] GetData(bool isCropped = true, int xCrop = 0, int yCrop = 0, bool isNormalized = false)
		{
			int num;
			int num2;
			float[,] array;
			if (isCropped)
			{
				num = this.m_width;
				num2 = this.m_height;
				array = this.m_data;
			}
			else
			{
				num = this.m_ucWidth;
				num2 = this.m_ucHeight;
				array = this.m_ucData;
			}
			num -= xCrop;
			num2 -= yCrop;
			float[,] array2 = new float[num, num2];
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					float num3;
					if (isNormalized)
					{
						num3 = (array[i, j] + 1f) / 2f;
					}
					else
					{
						num3 = array[i, j];
					}
					array2[i, j] = num3;
				}
			}
			return array2;
		}

		public void Clear()
		{
			this.Clear(0f);
		}

		public void Clear(float value)
		{
			for (int i = 0; i < this.m_width; i++)
			{
				for (int j = 0; j < this.m_height; j++)
				{
					this.m_data[i, j] = value;
				}
			}
		}

		private double GeneratePlanar(double x, double y)
		{
			return this.m_generator.GetValue(x, 0.0, y);
		}

		public void GeneratePlanar(double left, double right, double top, double bottom)
		{
			this.GeneratePlanar(left, right, top, bottom, true);
		}

		public void GeneratePlanar(double left, double right, double top, double bottom, bool isSeamless)
		{
			if (right <= left || bottom <= top)
			{
				throw new ArgumentException("Invalid right/left or bottom/top combination");
			}
			if (this.m_generator == null)
			{
				throw new ArgumentNullException("Generator is null");
			}
			double num = right - left;
			double num2 = bottom - top;
			double num3 = num / ((double)this.m_width - (double)this.m_ucBorder);
			double num4 = num2 / ((double)this.m_height - (double)this.m_ucBorder);
			double num5 = left;
			for (int i = 0; i < this.m_ucWidth; i++)
			{
				double num6 = top;
				for (int j = 0; j < this.m_ucHeight; j++)
				{
					float num7;
					if (isSeamless)
					{
						num7 = (float)this.GeneratePlanar(num5, num6);
					}
					else
					{
						double a = this.GeneratePlanar(num5, num6);
						double b = this.GeneratePlanar(num5 + num, num6);
						double a2 = this.GeneratePlanar(num5, num6 + num2);
						double b2 = this.GeneratePlanar(num5 + num, num6 + num2);
						double position = 1.0 - (num5 - left) / num;
						double position2 = 1.0 - (num6 - top) / num2;
						double a3 = Utils.InterpolateLinear(a, b, position);
						double b3 = Utils.InterpolateLinear(a2, b2, position);
						num7 = (float)Utils.InterpolateLinear(a3, b3, position2);
					}
					this.m_ucData[i, j] = num7;
					if (i >= this.m_ucBorder && j >= this.m_ucBorder && i < this.m_width + this.m_ucBorder && j < this.m_height + this.m_ucBorder)
					{
						this.m_data[i - this.m_ucBorder, j - this.m_ucBorder] = num7;
					}
					num6 += num4;
				}
				num5 += num3;
			}
		}

		private double GenerateCylindrical(double angle, double height)
		{
			double x = Math.Cos(angle * 0.017453292519943295);
			double z = Math.Sin(angle * 0.017453292519943295);
			return this.m_generator.GetValue(x, height, z);
		}

		public void GenerateCylindrical(double angleMin, double angleMax, double heightMin, double heightMax)
		{
			if (angleMax <= angleMin || heightMax <= heightMin)
			{
				throw new ArgumentException("Invalid angle or height parameters");
			}
			if (this.m_generator == null)
			{
				throw new ArgumentNullException("Generator is null");
			}
			double num = angleMax - angleMin;
			double num2 = heightMax - heightMin;
			double num3 = num / ((double)this.m_width - (double)this.m_ucBorder);
			double num4 = num2 / ((double)this.m_height - (double)this.m_ucBorder);
			double num5 = angleMin;
			for (int i = 0; i < this.m_ucWidth; i++)
			{
				double num6 = heightMin;
				for (int j = 0; j < this.m_ucHeight; j++)
				{
					this.m_ucData[i, j] = (float)this.GenerateCylindrical(num5, num6);
					if (i >= this.m_ucBorder && j >= this.m_ucBorder && i < this.m_width + this.m_ucBorder && j < this.m_height + this.m_ucBorder)
					{
						this.m_data[i - this.m_ucBorder, j - this.m_ucBorder] = (float)this.GenerateCylindrical(num5, num6);
					}
					num6 += num4;
				}
				num5 += num3;
			}
		}

		private double GenerateSpherical(double lat, double lon)
		{
			double num = Math.Cos(0.017453292519943295 * lat);
			return this.m_generator.GetValue(num * Math.Cos(0.017453292519943295 * lon), Math.Sin(0.017453292519943295 * lat), num * Math.Sin(0.017453292519943295 * lon));
		}

		public void GenerateSpherical(double south, double north, double west, double east)
		{
			if (east <= west || north <= south)
			{
				throw new ArgumentException("Invalid east/west or north/south combination");
			}
			if (this.m_generator == null)
			{
				throw new ArgumentNullException("Generator is null");
			}
			double num = east - west;
			double num2 = north - south;
			double num3 = num / ((double)this.m_width - (double)this.m_ucBorder);
			double num4 = num2 / ((double)this.m_height - (double)this.m_ucBorder);
			double num5 = west;
			for (int i = 0; i < this.m_ucWidth; i++)
			{
				double num6 = south;
				for (int j = 0; j < this.m_ucHeight; j++)
				{
					this.m_ucData[i, j] = (float)this.GenerateSpherical(num6, num5);
					if (i >= this.m_ucBorder && j >= this.m_ucBorder && i < this.m_width + this.m_ucBorder && j < this.m_height + this.m_ucBorder)
					{
						this.m_data[i - this.m_ucBorder, j - this.m_ucBorder] = (float)this.GenerateSpherical(num6, num5);
					}
					num6 += num4;
				}
				num5 += num3;
			}
		}

		public Texture2D GetTexture()
		{
			return this.GetTexture(GradientPresets.Grayscale);
		}

		public Texture2D GetTexture(Gradient gradient)
		{
			Texture2D texture2D = new Texture2D(this.m_width, this.m_height);
			texture2D.name = "Noise2DTex";
			Color[] array = new Color[this.m_width * this.m_height];
			for (int i = 0; i < this.m_width; i++)
			{
				for (int j = 0; j < this.m_height; j++)
				{
					float num;
					if (!float.IsNaN(this.m_borderValue) && (i == 0 || i == this.m_width - this.m_ucBorder || j == 0 || j == this.m_height - this.m_ucBorder))
					{
						num = this.m_borderValue;
					}
					else
					{
						num = this.m_data[i, j];
					}
					array[i + j * this.m_width] = gradient.Evaluate((num + 1f) / 2f);
				}
			}
			texture2D.SetPixels(array);
			texture2D.wrapMode = TextureWrapMode.Clamp;
			texture2D.Apply();
			return texture2D;
		}

		public Texture2D GetNormalMap(float intensity)
		{
			Texture2D texture2D = new Texture2D(this.m_width, this.m_height);
			texture2D.name = "Noise2DTex";
			Color[] array = new Color[this.m_width * this.m_height];
			for (int i = 0; i < this.m_ucWidth; i++)
			{
				for (int j = 0; j < this.m_ucHeight; j++)
				{
					float num = (this.m_ucData[Mathf.Max(0, i - this.m_ucBorder), j] - this.m_ucData[Mathf.Min(i + this.m_ucBorder, this.m_height + this.m_ucBorder), j]) / 2f;
					float num2 = (this.m_ucData[i, Mathf.Max(0, j - this.m_ucBorder)] - this.m_ucData[i, Mathf.Min(j + this.m_ucBorder, this.m_width + this.m_ucBorder)]) / 2f;
					Vector3 a = new Vector3(num * intensity, 0f, 1f);
					Vector3 b = new Vector3(0f, num2 * intensity, 1f);
					Vector3 vector = a + b;
					vector.Normalize();
					Vector3 zero = Vector3.zero;
					zero.x = (vector.x + 1f) / 2f;
					zero.y = (vector.y + 1f) / 2f;
					zero.z = (vector.z + 1f) / 2f;
					if (i >= this.m_ucBorder && j >= this.m_ucBorder && i < this.m_width + this.m_ucBorder && j < this.m_height + this.m_ucBorder)
					{
						array[i - this.m_ucBorder + (j - this.m_ucBorder) * this.m_width] = new Color(zero.x, zero.y, zero.z);
					}
				}
			}
			texture2D.SetPixels(array);
			texture2D.wrapMode = TextureWrapMode.Clamp;
			texture2D.Apply();
			return texture2D;
		}

		public void Dispose()
		{
			if (!this.m_disposed)
			{
				this.m_disposed = this.Disposing();
			}
			GC.SuppressFinalize(this);
		}

		protected virtual bool Disposing()
		{
			if (this.m_data != null)
			{
				this.m_data = null;
			}
			this.m_width = 0;
			this.m_height = 0;
			return true;
		}
	}
}
