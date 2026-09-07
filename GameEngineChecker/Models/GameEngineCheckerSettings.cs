using System.Collections.Generic;

namespace GameEngineChecker.Models
{
	public class GameEngineCheckerSettings : ObservableObject
	{
		private string _botLogin;
		private string _botPassword;
		private bool _updateImportedGames;

		public string BotLogin
		{
			get => _botLogin;
			set => SetValue(ref _botLogin, value);
		}

		public string BotPassword
		{
			get => _botPassword;
			set => SetValue(ref _botPassword, value);
		}

		public bool UpdateImportedGames
		{
			get => _updateImportedGames;
			set => SetValue(ref _updateImportedGames, value);
		}

		public static GameEngineCheckerSettings Default => new GameEngineCheckerSettings();
	}
}