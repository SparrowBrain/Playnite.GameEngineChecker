using GameEngineChecker.Interfaces;
using GameEngineChecker.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GameEngineChecker.Services
{
	public class PcGamingWikiClient : IPcGamingWikiClient, IDisposable
	{
		public const string BaseAddress = "https://www.pcgamingwiki.com";
		private static readonly string UserAgent = $"Playnite.GameEngineChecker Extension {UserAgentConstants.Version} (https://github.com/SparrowBrain/Playnite.GameEngineChecker)";
		private readonly IPlayniteAPI _api;
		private readonly GameEngineCheckerSettings _settings;
		private readonly IPcGamingWikiResponseParser _responseParser;
		private readonly ILogger _logger = LogManager.GetLogger();
		private readonly CookieContainer _cookieContainer;
		private readonly HttpClient _httpClient;
		private readonly HttpClientHandler _httpClientHandler;

		public PcGamingWikiClient(IPlayniteAPI api, GameEngineCheckerSettings settings, IPcGamingWikiResponseParser responseParser)
		{
			_api = api;
			_settings = settings;
			_responseParser = responseParser;
			_cookieContainer = new CookieContainer();
			_httpClientHandler = new HttpClientHandler() { CookieContainer = _cookieContainer };
			_httpClient = new HttpClient(_httpClientHandler) { BaseAddress = new Uri(BaseAddress) };
		}

		public async Task<string> GetEngines(Uri link, Game game, CancellationToken cancellationToken)
		{
			await EnsureLoggedIn(cancellationToken);

			_logger.Debug($"Request to PC Gaming Wiki: {link}");
			var request = new HttpRequestMessage(HttpMethod.Get, link);
			request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);

			var stopwatch = new Stopwatch();
			stopwatch.Start();
			var response = await _httpClient.SendAsync(request, cancellationToken);
			stopwatch.Stop();

			var responseString = await response.Content.ReadAsStringAsync();
			_logger.Debug($"Response from PC Gaming Wiki: Status: {response.StatusCode}; Body {responseString}; Elapsed milliseconds: {stopwatch.ElapsedMilliseconds:N}");

			response.EnsureSuccessStatusCode();
			var parsedResponse = _responseParser.ParseCargo(responseString);

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

		private async Task EnsureLoggedIn(CancellationToken cancellationToken)
		{
			var cookies = _cookieContainer.GetCookies(new Uri(BaseAddress));
			if (cookies["pcgamingwiki_session"] != null
				&& cookies["pcgamingwiki_BPsession"] != null)
			{
				return;
			}

			var token = await GetToken(cancellationToken);
			await Login(token, cancellationToken);
		}

		private async Task<string> GetToken(CancellationToken cancellationToken)
		{
			var link = "/w/api.php?action=query&meta=tokens&type=login&format=json";
			var request = new HttpRequestMessage(HttpMethod.Get, link);
			request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);

			var response = await _httpClient.SendAsync(request, cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync();

			var tokenResponse = _responseParser.ParseToken(responseString);
			return tokenResponse.Query.Tokens.LoginToken;
		}

		private async Task Login(string token, CancellationToken cancellationToken)
		{
			var link = "/w/api.php?action=login&format=json";
			var request = new HttpRequestMessage(HttpMethod.Post, link);
			request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);
			request.Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>( "lgname", _settings.BotLogin ),
				new KeyValuePair<string, string>( "lgpassword", _settings.BotPassword ),
				new KeyValuePair<string, string>( "lgtoken", token ),
			});

			var response = await _httpClient.SendAsync(request, cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync();

			var loginResponse = _responseParser.ParseLogin(responseString);
			_logger.Debug($"User {loginResponse.Login.LgUsername} logged in.");
		}

		public void Dispose()
		{
			_httpClient.Dispose();
			_httpClientHandler.Dispose();
		}
	}
}