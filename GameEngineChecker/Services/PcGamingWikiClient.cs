using GameEngineChecker.Interfaces;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GameEngineChecker.Services
{
	public class PcGamingWikiClient : IPcGamingWikiClient, IDisposable
	{
		private static readonly string UserAgent = $"Playnite.GameEngineChecker Extension {UserAgentConstants.Version} (https://github.com/SparrowBrain/Playnite.GameEngineChecker)";
		private readonly IPlayniteAPI _api;
		private readonly IPcGamingWikiResponseParser _responseParser;
		private readonly ILogger _logger = LogManager.GetLogger();
		private readonly HttpClient _httpClient;

		public PcGamingWikiClient(IPlayniteAPI api, IPcGamingWikiResponseParser responseParser)
		{
			_api = api;
			_responseParser = responseParser;
			_httpClient = new HttpClient();
		}

		public async Task<string> GetEngines(Uri link, Game game, CancellationToken cancellationToken)
		{
			try
			{
				_logger.Debug($"Request to PC Gaming Wiki: {link}");
				var request = new HttpRequestMessage(HttpMethod.Get, link);
				request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);

				var stopwatch = new Stopwatch();
				stopwatch.Start();
				var response = await _httpClient.SendAsync(request, cancellationToken);
				stopwatch.Stop();

				var responseString = await response.Content.ReadAsStringAsync();
				_logger.Debug($"Response from PC Gaming Wiki: Status: {response.StatusCode}; Body {responseString}; Elapsed milliseconds: {stopwatch.ElapsedMilliseconds}");

				response.EnsureSuccessStatusCode();
				var parsedResponse = _responseParser.Parse(responseString);

				if (parsedResponse?.CargoQuery?.Count > 1)
				{
					var foundEntries = string.Join(", ", parsedResponse.CargoQuery.Select(x => $"\"{x.Title?.Title}\""));
					_logger.Info($"Multiple PC Gaming Wiki entries found for game {game.Id} - {game.Name}: {foundEntries}. Skipping.");
					return null;
				}

				var engines = parsedResponse?.CargoQuery?.FirstOrDefault()?.Title?.Engines;
				if (engines == null)
				{
					_logger.Debug($"No engines found in response: {responseString}");
				}

				return engines;
			}
			catch (Exception ex)
			{
				if (!(ex is OperationCanceledException))
				{
					_logger.Error(ex, $"Error while getting engines via {link}");
					_api.Notifications.Add("game_engine_checker__pcgw_error_message",
						string.Format(
							ResourceProvider.GetString("LOCGame_Engine_Checker_PcgwDownloadErrorMessage"),
							game.Name,
							ex.Message),
						NotificationType.Error);
				}

				return null;
			}
		}

		public void Dispose()
		{
			_httpClient.Dispose();
		}
	}
}