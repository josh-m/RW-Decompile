using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class GraphicDatabaseHeadRecords
	{
		private class HeadGraphicRecord
		{
			public Gender gender;

			public CrownType crownType;

			public string graphicPath;

			private List<KeyValuePair<Color, Graphic_Multi>> graphics = new List<KeyValuePair<Color, Graphic_Multi>>();

			public HeadGraphicRecord(string graphicPath)
			{
				this.graphicPath = graphicPath;
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(graphicPath);
				string[] array = fileNameWithoutExtension.Split(new char[]
				{
					'_'
				});
				try
				{
					this.crownType = (CrownType)((byte)ParseHelper.FromString(array[array.Length - 2], typeof(CrownType)));
					this.gender = (Gender)((byte)ParseHelper.FromString(array[array.Length - 3], typeof(Gender)));
				}
				catch (Exception ex)
				{
					Log.Error("Parse error with head graphic at " + graphicPath + ": " + ex.Message);
					this.crownType = CrownType.Undefined;
					this.gender = Gender.None;
				}
			}

			public Graphic_Multi GetGraphic(Color color)
			{
				for (int i = 0; i < this.graphics.Count; i++)
				{
					if (color.IndistinguishableFrom(this.graphics[i].Key))
					{
						return this.graphics[i].Value;
					}
				}
				Graphic_Multi graphic_Multi = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(this.graphicPath, ShaderDatabase.CutoutSkin, Vector2.one, color);
				this.graphics.Add(new KeyValuePair<Color, Graphic_Multi>(color, graphic_Multi));
				return graphic_Multi;
			}
		}

		private static List<GraphicDatabaseHeadRecords.HeadGraphicRecord> heads = new List<GraphicDatabaseHeadRecords.HeadGraphicRecord>();

		private static GraphicDatabaseHeadRecords.HeadGraphicRecord skull = null;

		private static readonly string[] HeadsFolderPaths = new string[]
		{
			"Things/Pawn/Humanlike/Heads/Male",
			"Things/Pawn/Humanlike/Heads/Female"
		};

		private static readonly string SkullPath = "Things/Pawn/Humanlike/Heads/None_Average_Skull";

		public static void Reset()
		{
			GraphicDatabaseHeadRecords.heads.Clear();
			GraphicDatabaseHeadRecords.skull = null;
		}

		private static void BuildDatabaseIfNecessary()
		{
			if (GraphicDatabaseHeadRecords.heads.Count > 0 && GraphicDatabaseHeadRecords.skull != null)
			{
				return;
			}
			string[] headsFolderPaths = GraphicDatabaseHeadRecords.HeadsFolderPaths;
			for (int i = 0; i < headsFolderPaths.Length; i++)
			{
				string text = headsFolderPaths[i];
				foreach (string current in GraphicDatabaseUtility.GraphicNamesInFolder(text))
				{
					GraphicDatabaseHeadRecords.heads.Add(new GraphicDatabaseHeadRecords.HeadGraphicRecord(text + "/" + current));
				}
			}
			GraphicDatabaseHeadRecords.skull = new GraphicDatabaseHeadRecords.HeadGraphicRecord(GraphicDatabaseHeadRecords.SkullPath);
		}

		public static Graphic_Multi GetHeadNamed(string graphicPath, Color skinColor)
		{
			GraphicDatabaseHeadRecords.BuildDatabaseIfNecessary();
			for (int i = 0; i < GraphicDatabaseHeadRecords.heads.Count; i++)
			{
				GraphicDatabaseHeadRecords.HeadGraphicRecord headGraphicRecord = GraphicDatabaseHeadRecords.heads[i];
				if (headGraphicRecord.graphicPath == graphicPath)
				{
					return headGraphicRecord.GetGraphic(skinColor);
				}
			}
			Log.Message("Tried to get pawn head at path " + graphicPath + " that was not found. Defaulting...");
			return GraphicDatabaseHeadRecords.heads.First<GraphicDatabaseHeadRecords.HeadGraphicRecord>().GetGraphic(skinColor);
		}

		public static Graphic_Multi GetSkull()
		{
			GraphicDatabaseHeadRecords.BuildDatabaseIfNecessary();
			return GraphicDatabaseHeadRecords.skull.GetGraphic(Color.white);
		}

		public static Graphic_Multi GetHeadRandom(Gender gender, Color skinColor, CrownType crownType)
		{
			GraphicDatabaseHeadRecords.BuildDatabaseIfNecessary();
			Predicate<GraphicDatabaseHeadRecords.HeadGraphicRecord> predicate = (GraphicDatabaseHeadRecords.HeadGraphicRecord head) => head.crownType == crownType && head.gender == gender;
			int num = 0;
			GraphicDatabaseHeadRecords.HeadGraphicRecord headGraphicRecord;
			while (true)
			{
				headGraphicRecord = GraphicDatabaseHeadRecords.heads.RandomElement<GraphicDatabaseHeadRecords.HeadGraphicRecord>();
				if (predicate(headGraphicRecord))
				{
					break;
				}
				num++;
				if (num > 40)
				{
					goto Block_2;
				}
			}
			return headGraphicRecord.GetGraphic(skinColor);
			Block_2:
			foreach (GraphicDatabaseHeadRecords.HeadGraphicRecord current in GraphicDatabaseHeadRecords.heads.InRandomOrder(null))
			{
				if (predicate(current))
				{
					return current.GetGraphic(skinColor);
				}
			}
			Log.Error("Failed to find head for gender=" + gender + ". Defaulting...");
			return GraphicDatabaseHeadRecords.heads.First<GraphicDatabaseHeadRecords.HeadGraphicRecord>().GetGraphic(skinColor);
		}
	}
}
