using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTTPClient.CLI
{
	internal record struct Command
	{
		public string keyword { get; private set; }

		public string[] args { get; private set; }

		public string BuildArgs(int skip = 0) => String.Join(" ", args.Skip(skip));

		public Command(string command)
		{
			string[] args = command.Split(' ');
			keyword = args[0].ToLower();
			this.args = args.Skip(1).ToArray();
		}
	}
}
