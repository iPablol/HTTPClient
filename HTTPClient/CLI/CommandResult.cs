using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTTPClient.CLI
{
	internal record struct CommandResult(string keyword = "", object? result = null)
	{
	}
}
