using System;
using System.Threading;
using System.Threading.Tasks;
using Syringe.Core.Runner;
using Syringe.Core.Tasks;

namespace Syringe.Service.Parallel
{
	internal class SessionRunnerTaskInfo
	{
		public SessionRunnerTaskInfo(int id)
		{
			Id = id;
		}

		public int Id { get; private set; }
		public string Username { get; set; }
		public string TeamName { get; set; }
		public DateTime StartTime { get; set; }
		public TaskRequest Request { get; set; }
		public TestFileRunner Runner { get; set; }

		public CancellationTokenSource CancelTokenSource { get; set; }
		public Task CurrentTask { get; set; }

		public string Errors { get; set; }

        public int?  Position { get; set; }
	}
}