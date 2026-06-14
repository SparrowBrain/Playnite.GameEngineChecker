using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using GameEngineChecker.Services;
using GameEngineChecker.Tests;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using Xunit;

namespace GameEngineChecker.UnitTests.Services
{
	public class GamesFilterTests
	{
		private readonly Fixture _fixture;
		private readonly Tag _engineTag;
		private readonly TestableItemCollection<Platform> _platforms;

		public GamesFilterTests()
		{
			_fixture = new Fixture();
			_fixture.Customize(new AutoFakeItEasyCustomization());

			var api = _fixture.Freeze<IPlayniteAPI>();

			var tags = new TestableItemCollection<Tag>(new List<Tag>());

			_engineTag = _fixture.Create<Tag>();
			_engineTag.Name = "[Engine] Unity";
			tags.Add(_engineTag);

			A.CallTo(() => api.Database.Tags).Returns(tags);

			_platforms = new TestableItemCollection<Platform>(new List<Platform>());

			A.CallTo(() => api.Database.Platforms).ReturnsLazily(() => _platforms);
		}

		[Fact]
		public void ShouldTheGameBeProcessed_ReturnsTrue_WhenGameHasNoEngineTags()
		{
			// Arrange
			var game = CreatePcGame();
			var sut = _fixture.Create<GamesFilter>();

			// Act
			var result = sut.ShouldTheGameBeProcessed(game);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void ShouldTheGameBeProcessed_ReturnsTrue_WhenGameHasNoTags()
		{
			// Arrange
			var game = CreatePcGame();
			game.TagIds = null;
			var sut = _fixture.Create<GamesFilter>();

			// Act
			var result = sut.ShouldTheGameBeProcessed(game);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void ShouldTheGameBeProcessed_ReturnsFalse_WhenGameHasAnyEngineTag()
		{
			// Arrange
			var game = CreatePcGame();
			game.TagIds.Add(_engineTag.Id);
			var sut = _fixture.Create<GamesFilter>();

			// Act
			var result = sut.ShouldTheGameBeProcessed(game);

			// Assert
			Assert.False(result);
		}

		[Theory]
		[InlineData("PC")]
		[InlineData("PC (Windows)")]
		[InlineData("PC (Linux)")]
		[InlineData("PC (DOS)")]
		public void ShouldTheGameBeProcessed_ReturnsTrue_WhenGameHasPcPlatformName(string platformName)
		{
			// Arrange
			var pcPlatform = new Platform(platformName);
			_platforms.Clear();
			_platforms.Add(pcPlatform);
			var game = _fixture.Create<Game>();
			game.PlatformIds.Add(pcPlatform.Id);
			var sut = _fixture.Create<GamesFilter>();

			// Act
			var result = sut.ShouldTheGameBeProcessed(game);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void ShouldTheGameBeProcessed_ReturnsTrue_WhenGameHasPcPlatformSpecificationId()
		{
			// Arrange
			var pcPlatform = new Platform("Random") { SpecificationId = "pc_windows" };
			_platforms.Clear();
			_platforms.Add(pcPlatform);
			var game = _fixture.Create<Game>();
			game.PlatformIds.Add(pcPlatform.Id);
			var sut = _fixture.Create<GamesFilter>();

			// Act
			var result = sut.ShouldTheGameBeProcessed(game);

			// Assert
			Assert.True(result);
		}

		[Theory]
		[InlineData("PlayStation", null)]
		[InlineData("Random", "random")]
		public void ShouldTheGameBeProcessed_ReturnsFalse_WhenGamePlatformIsNotPc(string name, string specificationId)
		{
			// Arrange
			var platform = new Platform(name) { SpecificationId = specificationId };
			_platforms.Clear();
			_platforms.Add(platform);
			var game = _fixture.Create<Game>();
			game.PlatformIds.Add(platform.Id);
			var sut = _fixture.Create<GamesFilter>();

			// Act
			var result = sut.ShouldTheGameBeProcessed(game);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void ShouldTheGameBeProcessed_ReturnsFalse_WhenGamePlatformsIsNull()
		{
			// Arrange
			var game = _fixture.Create<Game>();
			game.PlatformIds = null;
			var sut = _fixture.Create<GamesFilter>();

			// Act
			var result = sut.ShouldTheGameBeProcessed(game);

			// Assert
			Assert.False(result);
		}

		private Game CreatePcGame()
		{
			var game = _fixture.Create<Game>();

			var pcPlatform = new Platform("PC");
			_platforms.Add(pcPlatform);

			game.PlatformIds.Add(pcPlatform.Id);

			return game;
		}
	}
}