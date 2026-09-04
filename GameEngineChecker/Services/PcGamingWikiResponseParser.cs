using GameEngineChecker.Interfaces;
using GameEngineChecker.Models.Exceptions;
using GameEngineChecker.Models.PcGamingWiki;
using Newtonsoft.Json;

namespace GameEngineChecker.Services
{
	public class PcGamingWikiResponseParser : IPcGamingWikiResponseParser
	{
		public PcGamingWikiEngineResponse Parse(string responseBody)
		{
			var errorResponse = JsonConvert.DeserializeObject<PcGamingWikiApiErrorResponse>(responseBody);
			var error = errorResponse.Error;
			if (error != null)
			{
				throw new ApiErrorException(error.Code, error.Info);
			}

			var importResponse = JsonConvert.DeserializeObject<PcGamingWikiEngineResponse>(responseBody);
			return importResponse;
		}
	}
}