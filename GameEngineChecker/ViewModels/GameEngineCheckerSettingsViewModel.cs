using GameEngineChecker.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;

namespace GameEngineChecker.ViewModels
{
	public class GameEngineCheckerSettingsViewModel : ObservableObject, ISettings
	{
		private readonly GameEngineChecker _plugin;
		private GameEngineCheckerSettings _settings;
		private GameEngineCheckerSettings _editingClone;

		public GameEngineCheckerSettingsViewModel(GameEngineChecker plugin)
		{
			_plugin = plugin;

			var savedSettings = plugin.LoadPluginSettings<GameEngineCheckerSettings>();
			Settings = savedSettings ?? GameEngineCheckerSettings.Default;
		}

		public GameEngineCheckerSettings Settings
		{
			get => _settings;
			set
			{
				_settings = value;
				OnPropertyChanged();
			}
		}

		public void BeginEdit()
		{
			_editingClone = Serialization.GetClone(Settings);
		}

		public void EndEdit()
		{
			_plugin.SavePluginSettings(Settings);
		}

		public void CancelEdit()
		{
			Settings = _editingClone;
		}

		public bool VerifySettings(out List<string> errors)
		{
			// Code execute when user decides to confirm changes made since BeginEdit was called.
			// Executed before EndEdit is called and EndEdit is not called if false is returned.
			// List of errors is presented to user if verification fails.

			errors = new List<string>();
			return true;
		}

		public ICommand CreateBotPassword => new RelayCommand(() =>
		{
			Process.Start("https://www.pcgamingwiki.com/wiki/Special:BotPasswords");
		});
	}
}