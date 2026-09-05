using AutoFixture.Xunit2;
using GameEngineChecker.Models.Exceptions;
using GameEngineChecker.Services;
using Xunit;

namespace GameEngineChecker.UnitTests.Services
{
	public class PcGamingWikiResponseParserTests
	{
		[Theory]
		[AutoData]
		public void ParseCargo_ThrowsException_WhenErrorResponse(
			PcGamingWikiResponseParser sut)
		{
			// Arrange
			var response =
				"{\"error\":{\"code\":\"permissiondenied\",\"info\":\"You don't have permission to run arbitrary Cargo queries.\",\"*\":\"See https://www.pcgamingwiki.com/w/api.php for API usage. Subscribe to the mediawiki-api-announce mailing list at &lt;https://lists.wikimedia.org/postorius/lists/mediawiki-api-announce.lists.wikimedia.org/&gt; for notice of API deprecations and breaking changes.\"}}";

			// Act
			var exception = Record.Exception(() => sut.ParseCargo(response));

			// Assert
			Assert.NotNull(exception);
			var apiError = Assert.IsType<ApiErrorException>(exception);
			Assert.Equal("permissiondenied", apiError.Code);
			Assert.Equal("You don't have permission to run arbitrary Cargo queries.", apiError.Message);
		}

		[Theory]
		[AutoData]
		public void ParseCargo_ReturnsApiResponse_WhenNormalResponse(
			PcGamingWikiResponseParser sut)
		{
			// Arrange
			var response =
				"{\"cargoquery\":[{\"title\":{\"Engines\":\"Engine:Unreal Engine 5,Engine:Gamebryo (TES Engine)\",\"title\":\"The Elder Scrolls IV: Oblivion Remastered\"}}]}";

			// Act
			var parsedResponse = sut.ParseCargo(response);

			// Assert
			var cargo = Assert.Single(parsedResponse.CargoQuery);
			Assert.Equal("The Elder Scrolls IV: Oblivion Remastered", cargo.Title.Title);
			Assert.Equal("Engine:Unreal Engine 5,Engine:Gamebryo (TES Engine)", cargo.Title.Engines);
		}

		[Theory]
		[AutoData]
		public void ParseToken_ReturnsTokenResponse_WhenNormalResponse(
			PcGamingWikiResponseParser sut)
		{
			// Arrange
			var response =
				"{\"batchcomplete\":\"\",\"query\":{\"tokens\":{\"logintoken\":\"e521cee2c8e341a7f09725dd5d9f76406a9bdd1c+\\\\\"}}}";

			// Act
			var parsedResponse = sut.ParseToken(response);

			// Assert
			Assert.NotNull(parsedResponse);
			Assert.NotNull(parsedResponse.Query);
			Assert.NotNull(parsedResponse.Query.Tokens);

			Assert.Equal(@"e521cee2c8e341a7f09725dd5d9f76406a9bdd1c+\", parsedResponse.Query.Tokens.LoginToken);
		}

		[Theory]
		[InlineAutoData("{\"login\":{\"result\":\"WrongToken\"}}", "WrongToken", null)]
		[InlineAutoData("{\"login\":{\"result\":\"Failed\",\"reason\":\"The supplied credentials could not be authenticated.\"}}", "Failed", "The supplied credentials could not be authenticated.")]
		public void ParseToken_ThrowsException_WhenWrongToken(
			string response,
			string result,
			string reason,
			PcGamingWikiResponseParser sut)
		{
			// Act
			var exception = Record.Exception(() => sut.ParseLogin(response));

			// Assert
			Assert.NotNull(exception);
			var apiError = Assert.IsType<ApiErrorException>(exception);
			Assert.Equal(result, apiError.Code);
			if (reason != null)
			{
				Assert.Equal(reason, apiError.Message);
			}
		}

		[Theory]
		[AutoData]
		public void ParseToken_ReturnsLoginResponse_WhenSuccess(
			PcGamingWikiResponseParser sut)
		{
			// Arrange
			var response = "{\"login\":{\"result\":\"Success\",\"lguserid\":12345,\"lgusername\":\"Username\"}}";

			// Act
			var parsedResponse = sut.ParseLogin(response);

			// Assert
			Assert.NotNull(parsedResponse);
			Assert.NotNull(parsedResponse.Login);

			Assert.Equal("Success", parsedResponse.Login.Result);
			Assert.Equal(12345, parsedResponse.Login.LgUserId);
			Assert.Equal("Username", parsedResponse.Login.LgUsername);
		}
	}
}