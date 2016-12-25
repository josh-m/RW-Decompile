using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public static class GraphicGetter_NakedHumanlike
	{
		private const string NakedBodyTextureFolderPath = "Things/Pawn/Humanlike/Bodies/";

		public static Graphic GetNakedBodyGraphic(BodyType bodyType, Shader shader, Color skinColor)
		{
			if (bodyType == BodyType.Undefined)
			{
				Log.Error("Getting naked body graphic with undefined body type.");
				bodyType = BodyType.Male;
			}
			string str = "Naked_" + bodyType.ToString();
			string path = "Things/Pawn/Humanlike/Bodies/" + str;
			return GraphicDatabase.Get<Graphic_Multi>(path, shader, Vector2.one, skinColor);
		}
	}
}
