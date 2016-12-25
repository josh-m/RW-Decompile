using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class PowerOverlayMats
	{
		private const string TransmitterAtlasPath = "Things/Special/Power/TransmitterAtlas";

		private static readonly Shader TransmitterShader;

		public static readonly Graphic LinkedOverlayGraphic;

		public static readonly Material MatConnectorBase;

		public static readonly Material MatConnectorLine;

		public static readonly Material MatConnectorAnticipated;

		public static readonly Material MatConnectorBaseAnticipated;

		static PowerOverlayMats()
		{
			PowerOverlayMats.TransmitterShader = ShaderDatabase.MetaOverlay;
			PowerOverlayMats.MatConnectorBase = MaterialPool.MatFrom("Things/Special/Power/OverlayBase", ShaderDatabase.MetaOverlay);
			PowerOverlayMats.MatConnectorLine = MaterialPool.MatFrom("Things/Special/Power/OverlayWire", ShaderDatabase.MetaOverlay);
			PowerOverlayMats.MatConnectorAnticipated = MaterialPool.MatFrom("Things/Special/Power/OverlayWireAnticipated", ShaderDatabase.MetaOverlay);
			PowerOverlayMats.MatConnectorBaseAnticipated = MaterialPool.MatFrom("Things/Special/Power/OverlayBaseAnticipated", ShaderDatabase.MetaOverlay);
			Graphic graphic = GraphicDatabase.Get<Graphic_Single>("Things/Special/Power/TransmitterAtlas", PowerOverlayMats.TransmitterShader);
			PowerOverlayMats.LinkedOverlayGraphic = GraphicUtility.WrapLinked(graphic, LinkDrawerType.TransmitterOverlay);
			graphic.MatSingle.renderQueue = 3600;
			PowerOverlayMats.MatConnectorBase.renderQueue = 3600;
			PowerOverlayMats.MatConnectorLine.renderQueue = 3600;
		}
	}
}
