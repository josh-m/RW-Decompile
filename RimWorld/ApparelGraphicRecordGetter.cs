using System;
using Verse;

namespace RimWorld
{
	public static class ApparelGraphicRecordGetter
	{
		public static bool TryGetGraphicApparel(Apparel apparel, BodyType bodyType, out ApparelGraphicRecord rec)
		{
			if (bodyType == BodyType.Undefined)
			{
				Log.Error("Getting apparel graphic with undefined body type.");
				bodyType = BodyType.Male;
			}
			if (apparel.def.apparel.wornGraphicPath.NullOrEmpty())
			{
				rec = new ApparelGraphicRecord(null, null);
				return false;
			}
			string path;
			if (apparel.def.apparel.LastLayer == ApparelLayer.Overhead)
			{
				path = apparel.def.apparel.wornGraphicPath;
			}
			else
			{
				path = apparel.def.apparel.wornGraphicPath + "_" + bodyType.ToString();
			}
			Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.Cutout, apparel.def.graphicData.drawSize, apparel.DrawColor);
			rec = new ApparelGraphicRecord(graphic, apparel);
			return true;
		}
	}
}
