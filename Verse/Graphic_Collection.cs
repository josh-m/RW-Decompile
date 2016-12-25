using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public abstract class Graphic_Collection : Graphic
	{
		protected Graphic[] subGraphics;

		public override void Init(GraphicRequest req)
		{
			this.data = req.graphicData;
			if (req.path.NullOrEmpty())
			{
				throw new ArgumentNullException("folderPath");
			}
			if (req.shader == null)
			{
				throw new ArgumentNullException("shader");
			}
			this.path = req.path;
			this.color = req.color;
			this.drawSize = req.drawSize;
			List<Texture2D> list = (from x in ContentFinder<Texture2D>.GetAllInFolder(req.path)
			where !x.name.EndsWith(Graphic_Single.MaskSuffix)
			select x).ToList<Texture2D>();
			if (list.NullOrEmpty<Texture2D>())
			{
				Log.Error("Collection cannot init: No textures found at path " + req.path);
				this.subGraphics = new Graphic[]
				{
					BaseContent.BadGraphic
				};
				return;
			}
			this.subGraphics = new Graphic[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				string path = req.path + "/" + list[i].name;
				this.subGraphics[i] = GraphicDatabase.Get<Graphic_Single>(path, req.shader, this.drawSize, this.color);
			}
		}
	}
}
