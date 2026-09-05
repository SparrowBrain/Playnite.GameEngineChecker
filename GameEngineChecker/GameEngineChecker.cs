using GameEngineChecker.Services;
using GameEngineChecker.ViewModels;
using GameEngineChecker.Views;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GameEngineChecker
{
	public class GameEngineChecker : GenericPlugin
	{
		public const string PluginId = "7a21243e-c7cc-4ca7-85bd-f6f96f22e9db";

		private const string ExtensionName = "Game Engine Checker";
		private const int PcGamingWikiMaxRequestsPerWindow = 30;
		private static readonly TimeSpan PcGamingWikiRateLimitWindow = TimeSpan.FromSeconds(60);
		private static readonly ILogger Logger = LogManager.GetLogger();
		private readonly Tagger _tagger;
		private readonly RateLimiter _rateLimiter;

		private GameEngineCheckerSettingsViewModel _settings;

		public override Guid Id { get; } = Guid.Parse(PluginId);

		public GameEngineChecker(IPlayniteAPI api) : base(api)
		{
			Properties = new GenericPluginProperties
			{
				HasSettings = true
			};
			_tagger = new Tagger(PlayniteApi);
			_rateLimiter = new RateLimiter(PcGamingWikiRateLimitWindow, PcGamingWikiMaxRequestsPerWindow);
		}

		public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
		{
			yield return new MainMenuItem()
			{
				MenuSection = $"@{ExtensionName}",
				Description = ResourceProvider.GetString("LOCGame_Engine_Checker_MenuItemAddTagSelectedGamesDescription"),
				Action = x =>
				{
					try
					{
						Task.Run(() => AddTagsToGames(PlayniteApi.MainView.SelectedGames.ToList()));
					}
					catch (Exception ex)
					{
						Logger.Error(ex, "Failure running add task. Should not happen.");
					}
				}
			};
		}

		public override ISettings GetSettings(bool firstRunSettings)
		{
			return _settings ?? (_settings = new GameEngineCheckerSettingsViewModel(this));
		}

		public override UserControl GetSettingsView(bool firstRunSettings)
		{
			return new GameEngineCheckerSettingsView();
		}

		private async Task AddTagsToGames(IReadOnlyList<Game> games)
		{
			if (games.Count == 0)
			{
				return;
			}

			try
			{
				GetSettings(false);

				var gamesFilter = new GamesFilter(PlayniteApi);
				var pcGamingWikiLinkProvider = new PcGamingWikiLinkProvider();
				var pcGamingWikiResponseParser = new PcGamingWikiResponseParser();
				var pcGamingWikiClient = new PcGamingWikiClient(PlayniteApi, _settings.Settings, pcGamingWikiResponseParser);
				var enginesParser = new EnginesParser();

				var gameEngineCheckerService = new GameEngineCheckerService(
					PlayniteApi,
					_settings.Settings,
					gamesFilter,
					_rateLimiter,
					pcGamingWikiLinkProvider,
					pcGamingWikiClient,
					enginesParser,
					_tagger);

				using (var cancellationTokenSource = new CancellationTokenSource())
				using (var progressViewModel = ShowProgressDialog(cancellationTokenSource))
				{
					void ReportProgressAction(float progress)
					{
						PlayniteApi.MainView.UIDispatcher.Invoke(() => progressViewModel.ProgressValue = progress);
					}

					var addedCount = await gameEngineCheckerService.AddGameEngineTags(games, ReportProgressAction, cancellationTokenSource.Token);

					Logger.Info($"Successfully added game engine to {addedCount} out of {games.Count} games.");
					PlayniteApi.Notifications.Add(
						"game_engine_checker__added_count",
						string.Format(ResourceProvider.GetString("LOCGame_Engine_Checker_ResultsMessage"), addedCount),
						NotificationType.Info);
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "Failure while adding engines to games.");
				PlayniteApi.Notifications.Add(
					"game_engine_checker__add_error",
					string.Format(ResourceProvider.GetString("LOCGame_Engine_Checker_ResultsErrorMessage"), ex.Message, ex.StackTrace),
					NotificationType.Error);
			}
		}

		private ProgressViewModel ShowProgressDialog(CancellationTokenSource cts)
		{
			var progressViewModel = new ProgressViewModel(PlayniteApi, cts);
			PlayniteApi.MainView.UIDispatcher.Invoke(() =>
				{
					var window = ShowDialog(
						new ProgressView(progressViewModel),
						100,
						250,
						ResourceProvider.GetString("LOCGame_Engine_Checker_ProgressTitle"),
						false,
						false);

					progressViewModel.SetWindow(window);
				}
			);

			return progressViewModel;
		}

		private Window ShowDialog(UserControl view, double height, double width, string title, bool showMaximizeButton, bool waitToClose)
		{
			var window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions()
			{
				ShowCloseButton = true,
				ShowMaximizeButton = showMaximizeButton,
				ShowMinimizeButton = false,
			});

			window.Height = height;
			window.Width = width;
			window.Title = title;

			window.Content = view;
			window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
			window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

			if (waitToClose)
			{
				window.ShowDialog();
			}
			else
			{
				window.Show();
			}

			return window;
		}
	}
}