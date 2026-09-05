using GameEngineChecker.Interfaces;
using GameEngineChecker.Models.Exceptions;
using GameEngineChecker.Models.PcGamingWiki;
using GameEngineChecker.Models.PcGamingWiki.Cargo;
using GameEngineChecker.Models.PcGamingWiki.Login;
using GameEngineChecker.Models.PcGamingWiki.Token;
using Newtonsoft.Json;
using System;

namespace GameEngineChecker.Services
{
	public class PcGamingWikiResponseParser : IPcGamingWikiResponseParser
	{
		public PcGamingWikiEngineResponse ParseCargo(string responseBody)
		{
			var errorResponse = JsonConvert.DeserializeObject<PcGamingWikiApiErrorResponse>(responseBody);
			var error = errorResponse.Error;
			if (error != null)
			{
				throw new ApiErrorException(error.Code, error.Info);
			}

			var engineResponse = JsonConvert.DeserializeObject<PcGamingWikiEngineResponse>(responseBody);
			return engineResponse;
		}

		public PcGamingWikiTokenResponse ParseToken(string responseBody)
		{
			var response = JsonConvert.DeserializeObject<PcGamingWikiTokenResponse>(responseBody);
			return response;
		}

		public PcGamingWikiLoginResponse ParseLogin(string responseBody)
		{
			var response = JsonConvert.DeserializeObject<PcGamingWikiLoginResponse>(responseBody);

			if (string.Equals(response.Login.Result, "Success", StringComparison.OrdinalIgnoreCase))
			{
				return response;
			}

			throw new ApiErrorException(response.Login.Result, response.Login.Reason);
		}
	}
}