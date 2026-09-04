using System.Collections.Generic;

namespace GameEngineChecker.Models
{
	public class GameEngineCheckerSettings : ObservableObject
	{
		private string _botLogin;
		private string _botPassword;

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

		public static GameEngineCheckerSettings Default => new GameEngineCheckerSettings();
	}
}