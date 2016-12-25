using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class LightningBoltMeshPool
	{
		private const int NumBoltMeshesMax = 20;

		private static List<Mesh> boltMeshes = new List<Mesh>();

		public static Mesh RandomBoltMesh
		{
			get
			{
				if (LightningBoltMeshPool.boltMeshes.Count < 20)
				{
					Mesh mesh = LightningBoltMeshMaker.NewBoltMesh();
					LightningBoltMeshPool.boltMeshes.Add(mesh);
					return mesh;
				}
				return LightningBoltMeshPool.boltMeshes.RandomElement<Mesh>();
			}
		}
	}
}
