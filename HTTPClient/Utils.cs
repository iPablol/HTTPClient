﻿using HTTPClient.CLI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HTTPClient
{
	internal static class Utils
	{
		public static IPAddress? ResolveHost(string host)
		{
			if (!IPAddress.TryParse(host, out IPAddress? address))
				address = Dns.GetHostEntry(host).AddressList.FirstOrDefault();
			return address;
		}

		public static Uri? ToUri(this string url)
		{
			try
			{
				Uri uri = new(url);
				return uri;
			}
			catch (UriFormatException ex)
			{
				Console.WriteLine("Invalid URL");
				return null;
			}
			catch (Exception ex)
			{
				return null;
			}
		}

		internal static bool IsValidJson(this string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return false;

			try
			{
				JsonDocument.Parse(text);
				return true;
			}
			catch (JsonException)
			{
				return false;
			}
		}
	}
}
