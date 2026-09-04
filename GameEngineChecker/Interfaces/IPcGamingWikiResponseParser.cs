using GameEngineChecker.Models.PcGamingWiki;

namespace GameEngineChecker.Interfaces
{
	public interface IPcGamingWikiResponseParser
	{
		PcGamingWikiEngineResponse Parse(string responseBody);
	}
}