using System;
using System.Collections.Generic;

namespace Verse
{
	public class LogMessageQueue
	{
		public int maxMessages = 200;

		private Queue<LogMessage> messages = new Queue<LogMessage>();

		private LogMessage lastMessage;

		public IEnumerable<LogMessage> Messages
		{
			get
			{
				return this.messages;
			}
		}

		public void Enqueue(LogMessage msg)
		{
			if (this.lastMessage != null && msg.CanCombineWith(this.lastMessage))
			{
				this.lastMessage.repeats++;
				return;
			}
			this.lastMessage = msg;
			this.messages.Enqueue(msg);
			if (this.messages.Count > this.maxMessages)
			{
				LogMessage oldMessage = this.messages.Dequeue();
				EditWindow_Log.Notify_MessageDequeued(oldMessage);
			}
		}

		internal void Clear()
		{
			this.messages.Clear();
			this.lastMessage = null;
		}
	}
}
