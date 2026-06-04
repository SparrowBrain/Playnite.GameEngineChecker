using System.IO;
using System.Text;

namespace ReleaseTools.UserAgent
{
	public class UserAgentUpdater
	{
		public void Update(string userAgentConstantsClass, string version)
		{
			var code = File.ReadAllLines(userAgentConstantsClass);
			var newCode = new StringBuilder();
			foreach (var line in code)
			{
				if (line.Contains("public const string Version"))
				{
					var equalsIndex = line.IndexOf('=');
					newCode.Append(line.Substring(0, equalsIndex));
					newCode.AppendLine($"= \"v{version}\";");

					continue;
				}

				newCode.AppendLine(line);
			}

			if (newCode.ToString().EndsWith("\r\n"))
			{
				newCode.Remove(newCode.Length - 2, 2);
			}

			File.WriteAllText(userAgentConstantsClass, newCode.ToString(), Encoding.UTF8);
		}
	}
}