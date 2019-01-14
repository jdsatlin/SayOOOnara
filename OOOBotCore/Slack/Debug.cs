using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SayOOOnara.Slack
{
	public class Debug
	{
		public static void AddToDebugLog(string message)
		{
			var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			var debugPath = Path.GetFullPath(baseDirectory + "\\debug");

			if (Directory.Exists(debugPath))
			{
				File.AppendAllText(debugPath + "\\log.txt", $"{DateTime.Now}: "+ message + " \n");
			}
		}
	}
}
