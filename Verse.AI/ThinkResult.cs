using System;

namespace Verse.AI
{
	public struct ThinkResult : IEquatable<ThinkResult>
	{
		private Job jobInt;

		private ThinkNode sourceNodeInt;

		private JobTag? tag;

		private bool fromQueue;

		public Job Job
		{
			get
			{
				return this.jobInt;
			}
		}

		public ThinkNode SourceNode
		{
			get
			{
				return this.sourceNodeInt;
			}
		}

		public JobTag? Tag
		{
			get
			{
				return this.tag;
			}
		}

		public bool FromQueue
		{
			get
			{
				return this.fromQueue;
			}
		}

		public bool IsValid
		{
			get
			{
				return this.Job != null;
			}
		}

		public static ThinkResult NoJob
		{
			get
			{
				return new ThinkResult(null, null, null, false);
			}
		}

		public ThinkResult(Job job, ThinkNode sourceNode, JobTag? tag = null, bool fromQueue = false)
		{
			this.jobInt = job;
			this.sourceNodeInt = sourceNode;
			this.tag = tag;
			this.fromQueue = fromQueue;
		}

		public override string ToString()
		{
			string text = (this.Job == null) ? "null" : this.Job.ToString();
			string text2 = (this.SourceNode == null) ? "null" : this.SourceNode.ToString();
			return string.Concat(new string[]
			{
				"(job=",
				text,
				" sourceNode=",
				text2,
				")"
			});
		}

		public override int GetHashCode()
		{
			int seed = 0;
			seed = Gen.HashCombine<Job>(seed, this.jobInt);
			seed = Gen.HashCombine<ThinkNode>(seed, this.sourceNodeInt);
			seed = Gen.HashCombine<JobTag?>(seed, this.tag);
			return Gen.HashCombineStruct<bool>(seed, this.fromQueue);
		}

		public override bool Equals(object obj)
		{
			return obj is ThinkResult && this.Equals((ThinkResult)obj);
		}

		public bool Equals(ThinkResult other)
		{
			int arg_6D_0;
			if (this.jobInt == other.jobInt && this.sourceNodeInt == other.sourceNodeInt)
			{
				JobTag? jobTag = this.tag;
				JobTag arg_41_0 = jobTag.GetValueOrDefault();
				JobTag? jobTag2 = other.tag;
				if (arg_41_0 == jobTag2.GetValueOrDefault() && jobTag.HasValue == jobTag2.HasValue)
				{
					arg_6D_0 = ((this.fromQueue == other.fromQueue) ? 1 : 0);
					return arg_6D_0 != 0;
				}
			}
			arg_6D_0 = 0;
			return arg_6D_0 != 0;
		}

		public static bool operator ==(ThinkResult lhs, ThinkResult rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(ThinkResult lhs, ThinkResult rhs)
		{
			return !(lhs == rhs);
		}
	}
}
