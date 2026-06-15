using AutoFixture.Xunit2;
using ReleaseTools.Package;
using Xunit;

namespace ReleaseTools.UnitTests.Package
{
    public class ExtensionPackageNameGuesserTests
    {
        [Theory]
        [InlineAutoData("1.1.0", "Game_Engine_Checker_7a21243e-c7cc-4ca7-85bd-f6f96f22e9db_1_1_0.pext")]
        [InlineAutoData("20.30.404", "Game_Engine_Checker_7a21243e-c7cc-4ca7-85bd-f6f96f22e9db_20_30_404.pext")]
        public void GetName_ProviderNameWithinConvention(
            string version,
            string expected,
            ExtensionPackageNameGuesser sut)
        {
            // Act
            var actual = sut.GetName(version);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}