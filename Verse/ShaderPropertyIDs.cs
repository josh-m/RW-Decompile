using System;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class ShaderPropertyIDs
	{
		private static readonly string PlanetSunLightDirectionName = "_PlanetSunLightDirection";

		private static readonly string PlanetSunLightEnabledName = "_PlanetSunLightEnabled";

		private static readonly string PlanetRadiusName = "_PlanetRadius";

		private static readonly string MapSunLightDirectionName = "_CastVect";

		private static readonly string GlowRadiusName = "_GlowRadius";

		private static readonly string GameSecondsName = "_GameSeconds";

		private static readonly string ColorName = "_Color";

		private static readonly string ColorTwoName = "_ColorTwo";

		private static readonly string MaskTexName = "_MaskTex";

		private static readonly string SwayHeadName = "_SwayHead";

		private static readonly string ShockwaveSpanName = "_ShockwaveSpan";

		public static int PlanetSunLightDirection = Shader.PropertyToID(ShaderPropertyIDs.PlanetSunLightDirectionName);

		public static int PlanetSunLightEnabled = Shader.PropertyToID(ShaderPropertyIDs.PlanetSunLightEnabledName);

		public static int PlanetRadius = Shader.PropertyToID(ShaderPropertyIDs.PlanetRadiusName);

		public static int MapSunLightDirection = Shader.PropertyToID(ShaderPropertyIDs.MapSunLightDirectionName);

		public static int GlowRadius = Shader.PropertyToID(ShaderPropertyIDs.GlowRadiusName);

		public static int GameSeconds = Shader.PropertyToID(ShaderPropertyIDs.GameSecondsName);

		public static int Color = Shader.PropertyToID(ShaderPropertyIDs.ColorName);

		public static int ColorTwo = Shader.PropertyToID(ShaderPropertyIDs.ColorTwoName);

		public static int MaskTex = Shader.PropertyToID(ShaderPropertyIDs.MaskTexName);

		public static int SwayHead = Shader.PropertyToID(ShaderPropertyIDs.SwayHeadName);

		public static int ShockwaveSpan = Shader.PropertyToID(ShaderPropertyIDs.ShockwaveSpanName);
	}
}
