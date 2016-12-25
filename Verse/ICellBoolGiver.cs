using System;
using UnityEngine;

namespace Verse
{
	public interface ICellBoolGiver
	{
		Color Color
		{
			get;
		}

		bool GetCellBool(int index);
	}
}
