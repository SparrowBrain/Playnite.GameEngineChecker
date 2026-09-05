using GameEngineChecker.Models.PcGamingWiki.Cargo;
using GameEngineChecker.Models.PcGamingWiki.Login;
using GameEngineChecker.Models.PcGamingWiki.Token;

namespace GameEngineChecker.Interfaces
{
	public interface IPcGamingWikiResponseParser
	{
		PcGamingWikiEngineResponse ParseCargo(string responseBody);
		PcGamingWikiTokenResponse ParseToken(string responseBody);
		PcGamingWikiLoginResponse ParseLogin(string responseBody);
	}
}