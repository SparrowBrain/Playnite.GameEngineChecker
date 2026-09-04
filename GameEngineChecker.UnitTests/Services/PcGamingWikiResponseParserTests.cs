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
		public void Parse_ThrowsException_WhenErrorResponse(
			PcGamingWikiResponseParser sut)
		{
			// Arrange
			var response =
				"{\"error\":{\"code\":\"permissiondenied\",\"info\":\"You don't have permission to run arbitrary Cargo queries.\",\"*\":\"See https://www.pcgamingwiki.com/w/api.php for API usage. Subscribe to the mediawiki-api-announce mailing list at &lt;https://lists.wikimedia.org/postorius/lists/mediawiki-api-announce.lists.wikimedia.org/&gt; for notice of API deprecations and breaking changes.\"}}";

			// Act
			var exception = Record.Exception(() => sut.Parse(response));

			// Assert
			Assert.NotNull(exception);
			var apiError = Assert.IsType<ApiErrorException>(exception);
			Assert.Equal("permissiondenied", apiError.Code);
			Assert.Equal("You don't have permission to run arbitrary Cargo queries.", apiError.Message);
		}

		[Theory]
		[AutoData]
		public void Parse_ReturnsApiResponse_WhenNormalResponse(
			PcGamingWikiResponseParser sut)
		{
			// Arrange
			var response =
				"{\"cargoquery\":[{\"title\":{\"Engines\":\"Engine:Unreal Engine 5,Engine:Gamebryo (TES Engine)\",\"title\":\"The Elder Scrolls IV: Oblivion Remastered\"}}]}";

			// Act
			var parsedResponse = sut.Parse(response);

			// Assert
			var cargo = Assert.Single(parsedResponse.CargoQuery);
			Assert.Equal("The Elder Scrolls IV: Oblivion Remastered", cargo.Title.Title);
			Assert.Equal("Engine:Unreal Engine 5,Engine:Gamebryo (TES Engine)", cargo.Title.Engines);
		}
	}
}