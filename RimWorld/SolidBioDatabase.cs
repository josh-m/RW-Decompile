using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class SolidBioDatabase
	{
		public static List<PawnBio> allBios = new List<PawnBio>();

		public static void Clear()
		{
			SolidBioDatabase.allBios.Clear();
		}

		public static void LoadAllBios()
		{
			foreach (PawnBio current in XmlLoader.LoadXmlDataInResourcesFolder<PawnBio>("Backstories/Solid"))
			{
				current.name.ResolveMissingPieces(null);
				if (current.childhood == null || current.adulthood == null)
				{
					PawnNameDatabaseSolid.AddPlayerContentName(current.name, current.gender);
				}
				else
				{
					current.PostLoad();
					current.ResolveReferences();
					foreach (string current2 in current.ConfigErrors())
					{
						Log.Error(current2);
					}
					SolidBioDatabase.allBios.Add(current);
					current.childhood.shuffleable = false;
					current.childhood.slot = BackstorySlot.Childhood;
					current.adulthood.shuffleable = false;
					current.adulthood.slot = BackstorySlot.Adulthood;
					BackstoryDatabase.AddBackstory(current.childhood);
					BackstoryDatabase.AddBackstory(current.adulthood);
				}
			}
		}
	}
}
