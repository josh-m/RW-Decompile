using System;

namespace Verse
{
	public class Graphic_Terrain : Graphic_Single
	{
		public override void Init(GraphicRequest req)
		{
			base.Init(req);
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"Terrain(path=",
				this.path,
				", shader=",
				base.Shader,
				", color=",
				this.color,
				")"
			});
		}
	}
}
