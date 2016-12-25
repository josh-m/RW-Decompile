using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class DialogDatabase
	{
		private static List<DiaNodeMold> Nodes;

		private static List<DiaNodeList> NodeLists;

		static DialogDatabase()
		{
			DialogDatabase.Nodes = new List<DiaNodeMold>();
			DialogDatabase.NodeLists = new List<DiaNodeList>();
			DialogDatabase.LoadAllDialog();
		}

		private static void LoadAllDialog()
		{
			DialogDatabase.Nodes.Clear();
			UnityEngine.Object[] array = Resources.LoadAll("Dialog", typeof(TextAsset));
			UnityEngine.Object[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				UnityEngine.Object @object = array2[i];
				TextAsset ass = @object as TextAsset;
				if (@object.name == "BaseEncounters" || @object.name == "GeneratedDialogs")
				{
					LayerLoader.LoadFileIntoList(ass, DialogDatabase.Nodes, DialogDatabase.NodeLists, DiaNodeType.BaseEncounters);
				}
				if (@object.name == "InsanityBattles")
				{
					LayerLoader.LoadFileIntoList(ass, DialogDatabase.Nodes, DialogDatabase.NodeLists, DiaNodeType.InsanityBattles);
				}
				if (@object.name == "SpecialEncounters")
				{
					LayerLoader.LoadFileIntoList(ass, DialogDatabase.Nodes, DialogDatabase.NodeLists, DiaNodeType.Special);
				}
			}
			foreach (DiaNodeMold current in DialogDatabase.Nodes)
			{
				current.PostLoad();
			}
			LayerLoader.MarkNonRootNodes(DialogDatabase.Nodes);
		}

		public static DiaNodeMold GetRandomEncounterRootNode(DiaNodeType NType)
		{
			List<DiaNodeMold> list = new List<DiaNodeMold>();
			foreach (DiaNodeMold current in DialogDatabase.Nodes)
			{
				if (current.isRoot && (!current.unique || !current.used) && current.nodeType == NType)
				{
					list.Add(current);
				}
			}
			return list.RandomElement<DiaNodeMold>();
		}

		public static DiaNodeMold GetNodeNamed(string NodeName)
		{
			foreach (DiaNodeMold current in DialogDatabase.Nodes)
			{
				if (current.name == NodeName)
				{
					DiaNodeMold result = current;
					return result;
				}
			}
			foreach (DiaNodeList current2 in DialogDatabase.NodeLists)
			{
				if (current2.Name == NodeName)
				{
					DiaNodeMold result = current2.RandomNodeFromList();
					return result;
				}
			}
			Log.Error("Did not find node named '" + NodeName + "'.");
			return null;
		}
	}
}
