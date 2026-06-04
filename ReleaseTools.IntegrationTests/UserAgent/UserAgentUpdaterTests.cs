using AutoFixture.Xunit2;
using System;
using System.IO;
using ReleaseTools.UserAgent;
using Xunit;

namespace ReleaseTools.IntegrationTests.UserAgent
{
	public class UserAgentUpdaterTests : IDisposable
	{
		private const string ExpectedUserAgentConstantsClass = "UserAgent\\TestData\\After.cs";
		private const string UserAgentConstantsClassBefore = "..\\..\\..\\GameEngineChecker\\UserAgentConstants.cs";
		private readonly string _userAgentConstantsClass;

		public UserAgentUpdaterTests()
		{
			_userAgentConstantsClass = Path.GetTempFileName();
			File.Delete(_userAgentConstantsClass);
			File.Copy(UserAgentConstantsClassBefore, _userAgentConstantsClass);
		}

		[Theory, AutoData]
		public void Update_ReplacesTheVersionWithTheGivenOne(
			UserAgentUpdater sut)
		{
			// Arrange
			var expectedClass = File.ReadAllText(ExpectedUserAgentConstantsClass);

			// Act
			sut.Update(_userAgentConstantsClass, "33.0.120");

			// Assert
			var actual = File.ReadAllText(_userAgentConstantsClass);
			Assert.Equal(expectedClass, actual);
		}

		public void Dispose()
		{
			File.Delete(_userAgentConstantsClass);
		}
	}
}