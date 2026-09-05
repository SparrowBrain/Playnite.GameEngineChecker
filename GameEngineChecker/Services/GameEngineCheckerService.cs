using GameEngineChecker.Interfaces;
using GameEngineChecker.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace GameEngineChecker.Services
{
	public class GameEngineCheckerService
	{
		private readonly ILogger _logger = LogManager.GetLogger();
		private readonly IPlayniteAPI _api;
		private readonly GameEngineCheckerSettings _settings;
		private readonly IGamesFilter _filter;
		private readonly IRateLimiter _rateLimiter;
		private readonly IPcGamingWikiLinkProvider _linkProvider;
		private readonly IPcGamingWikiClient _client;
		private readonly IEnginesParser _enginesParser;
		private readonly ITagger _tagger;

		public GameEngineCheckerService(
			IPlayniteAPI api,
			GameEngineCheckerSettings settings,
			IGamesFilter filter,
			IRateLimiter rateLimiter,
			IPcGamingWikiLinkProvider linkProvider,
			IPcGamingWikiClient client,
			IEnginesParser enginesParser,
			ITagger tagger)
		{
			_api = api;
			_settings = settings;
			_filter = filter;
			_rateLimiter = rateLimiter;
			_linkProvider = linkProvider;
			_client = client;
			_enginesParser = enginesParser;
			_tagger = tagger;
		}

		public async Task<int> AddGameEngineTags(
			IReadOnlyList<Game> games,
			Action<float> reportProgress,
			CancellationToken cancellationToken)
		{
			var addedCount = 0;
			var currentGameName = string.Empty;
			try
			{
				if (string.IsNullOrWhiteSpace(_settings.BotLogin)
					|| string.IsNullOrWhiteSpace(_settings.BotPassword))
				{
					throw new AuthenticationException("Missing authentication credentials.");
				}

				using (var _ = _api.Database.BufferedUpdate())
				{
					for (var i = 0; i < games.Count; i++)
					{
						var game = games[i];
						currentGameName = game.Name;
						if (cancellationToken.IsCancellationRequested)
						{
							return addedCount;
						}

						if (!_filter.ShouldTheGameBeProcessed(game))
						{
							ReportProgress(reportProgress, i, games);
							continue;
						}

						var link = await _linkProvider.GetLink(game, cancellationToken);
						if (link == null)
						{
							_logger.Info($"Could not create PC Gaming Wiki link for game {game.Id} - {game.Name}.");
							ReportProgress(reportProgress, i, games);
							continue;
						}

						await _rateLimiter.Limit(games.Count, cancellationToken);
						var engines = await _client.GetEngines(link, game, cancellationToken);
						if (engines == null)
						{
							_logger.Info($"No engines found for game {game.Id} - {game.Name}.");
							ReportProgress(reportProgress, i, games);
							continue;
						}

						var parsedEngines = _enginesParser.Parse(engines);
						_tagger.AddEngineTags(game, parsedEngines, cancellationToken);

						addedCount++;
						ReportProgress(reportProgress, i, games);
					}
				}

				return addedCount;
			}
			catch (OperationCanceledException)
			{
				return addedCount;
			}
			catch (AuthenticationException ex)
			{
				_logger.Info(ex, "Missing user credentials.");
				var message = new NotificationMessage(
					"game_engine_checker__pcgw_error_message",
					ResourceProvider.GetString("LOCGame_Engine_Checker_MissingAuthCredentials"),
					NotificationType.Error,
					() => _api.MainView.OpenPluginSettings(Guid.Parse(GameEngineChecker.PluginId))
				);
				_api.Notifications.Add(message);
				return addedCount;
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Error while getting engines");
				_api.Notifications.Add("game_engine_checker__pcgw_error_message",
					string.Format(
						ResourceProvider.GetString("LOCGame_Engine_Checker_PcgwDownloadErrorMessage"),
						currentGameName,
						ex.Message),
					NotificationType.Error);

				return addedCount;
			}
		}

		private static void ReportProgress(Action<float> reportProgress, int i, IReadOnlyList<Game> games)
		{
			reportProgress.Invoke(i * 100f / games.Count);
		}
	}
}