using System;
using System.Xml;
using UnityEngine;

namespace Verse
{
	public class CurvePoint
	{
		public Vector2 loc;

		public float x
		{
			get
			{
				return this.loc.x;
			}
			set
			{
				this.loc.x = value;
			}
		}

		public float y
		{
			get
			{
				return this.loc.y;
			}
			set
			{
				this.loc.y = value;
			}
		}

		public CurvePoint()
		{
			this.loc = new Vector2(0f, 0f);
		}

		public CurvePoint(float x, float y)
		{
			this.loc = new Vector2(x, y);
		}

		public CurvePoint(Vector2 loc)
		{
			this.loc = loc;
		}

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			XmlNode firstChild = xmlRoot.FirstChild;
			if (firstChild.Name == "loc")
			{
				if (firstChild.ChildNodes.Count == 2)
				{
					this.loc.x = Convert.ToSingle(firstChild["x"].FirstChild.Value);
					this.loc.y = Convert.ToSingle(firstChild["y"].FirstChild.Value);
				}
				else
				{
					this.loc = (Vector2)ParseHelper.FromString(firstChild.FirstChild.Value, typeof(Vector2));
				}
			}
			else
			{
				this.loc = (Vector2)ParseHelper.FromString(firstChild.Value, typeof(Vector2));
			}
		}

		public override string ToString()
		{
			return this.loc.ToStringTwoDigits();
		}

		public static implicit operator Vector2(CurvePoint pt)
		{
			return pt.loc;
		}
	}
}
