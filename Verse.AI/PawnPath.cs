using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse.AI
{
	public class PawnPath : IDisposable
	{
		private List<IntVec3> nodes = new List<IntVec3>(128);

		private float totalCostInt;

		private int curNodeIndex;

		public bool inUse;

		public bool Found
		{
			get
			{
				return this.totalCostInt >= 0f;
			}
		}

		public float TotalCost
		{
			get
			{
				return this.totalCostInt;
			}
		}

		public int NodesLeftCount
		{
			get
			{
				return this.curNodeIndex + 1;
			}
		}

		public List<IntVec3> NodesReversed
		{
			get
			{
				return this.nodes;
			}
		}

		public IntVec3 FirstNode
		{
			get
			{
				return this.nodes[this.nodes.Count - 1];
			}
		}

		public IntVec3 LastNode
		{
			get
			{
				return this.nodes[0];
			}
		}

		public static PawnPath NotFound
		{
			get
			{
				return PawnPathPool.NotFoundPath;
			}
		}

		public void AddNode(IntVec3 nodePosition)
		{
			this.nodes.Add(nodePosition);
		}

		public void SetupFound(float totalCost)
		{
			if (this == PawnPath.NotFound)
			{
				Log.Warning("Calling SetupFound with totalCost=" + totalCost + " on PawnPath.NotFound");
				return;
			}
			this.totalCostInt = totalCost;
			this.curNodeIndex = this.nodes.Count - 1;
		}

		public void Dispose()
		{
			this.ReleaseToPool();
		}

		public void ReleaseToPool()
		{
			if (this != PawnPath.NotFound)
			{
				this.totalCostInt = 0f;
				this.nodes.Clear();
				this.inUse = false;
			}
		}

		public static PawnPath NewNotFound()
		{
			return new PawnPath
			{
				totalCostInt = -1f
			};
		}

		public IntVec3 ConsumeNextNode()
		{
			IntVec3 result = this.Peek(1);
			this.curNodeIndex--;
			return result;
		}

		public IntVec3 Peek(int nodesAhead)
		{
			return this.nodes[this.curNodeIndex - nodesAhead];
		}

		public override string ToString()
		{
			if (!this.Found)
			{
				return "PawnPath(not found)";
			}
			if (!this.inUse)
			{
				return "PawnPath(not in use)";
			}
			return string.Concat(new object[]
			{
				"PawnPath(nodeCount= ",
				this.nodes.Count,
				(this.nodes.Count <= 0) ? string.Empty : string.Concat(new object[]
				{
					" first=",
					this.FirstNode,
					" last=",
					this.LastNode
				}),
				" cost=",
				this.totalCostInt,
				" )"
			});
		}

		public void DrawPath(Pawn pathingPawn)
		{
			if (!this.Found)
			{
				return;
			}
			float y = Altitudes.AltitudeFor(AltitudeLayer.Item);
			if (this.NodesLeftCount > 0)
			{
				for (int i = 0; i < this.NodesLeftCount - 1; i++)
				{
					Vector3 a = this.Peek(i).ToVector3Shifted();
					a.y = y;
					Vector3 b = this.Peek(i + 1).ToVector3Shifted();
					b.y = y;
					GenDraw.DrawLineBetween(a, b);
				}
				if (pathingPawn != null)
				{
					Vector3 drawPos = pathingPawn.DrawPos;
					drawPos.y = y;
					Vector3 b2 = this.Peek(0).ToVector3Shifted();
					b2.y = y;
					if ((drawPos - b2).sqrMagnitude > 0.01f)
					{
						GenDraw.DrawLineBetween(drawPos, b2);
					}
				}
			}
		}
	}
}
