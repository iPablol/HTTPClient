
using System.Net;
using System.Reflection;


namespace HTTPClient.CLI
{
	internal static class CLI
	{
		public static CommandResult HandleCommand(Command command)
		{
			try
			{
				return new(command.keyword, (from x in typeof(CLI).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
											 where x.Name.ToLower() == command.keyword
											 select x)?.First().Invoke(null, [command]));
			}
			catch
			{
				if (aliasDictionary.TryGetValue(command.keyword, out Delegate? function))
				{
					return new(command.keyword, function.DynamicInvoke(command));
				}
				if (command.keyword == "") return default;
				Console.WriteLine("Invalid command");
				return default;
			}
		}

		private static void Quit(Command command) => Environment.Exit(0);

		private static (string method, Uri) Get(Command command)
		{
			Uri? uri = command.args.First()?.ToUri();
			return uri is null ? default : ("GET", uri);
		}

		private static (string method, Uri) Head(Command command)
		{
			Uri? uri = command.args.First()?.ToUri();
			return uri is null ? default : ("HEAD", uri);
		}

		private static (string method, Uri, string body) Post(Command command)
		{
			Uri? uri = command.args.First()?.ToUri();
			return uri is null ? default : ("POST", uri, command.BuildArgs(skip: 1));
		}

		private static (string method, Uri, string body) Put(Command command)
		{
			Uri? uri = command.args.First()?.ToUri();
			return uri is null ? default : ("PUT", uri, command.BuildArgs(skip: 1));
		}

		private static (string method, Uri, string body) Delete(Command command)
		{
			Uri? uri = command.args.First()?.ToUri();
			return uri is null ? default : ("DELETE", uri, command.BuildArgs(skip: 1));
		}

		private static void ChangeKey(Command command) => HTTPClient.key = command.args.First();

		private static readonly Dictionary<string, Delegate> aliasDictionary = new()
		{
			{ "bye", Quit },
			{ "key", ChangeKey }
		};
	}
}
