namespace ReleaseTools.Package
{
    public class ExtensionPackageNameGuesser : IExtensionPackageNameGuesser
    {
        public string GetName(string version)
        {
            var versionNumbers = version.Split('.');
            return $"Game_Engine_Checker_7a21243e-c7cc-4ca7-85bd-f6f96f22e9db_{versionNumbers[0]}_{versionNumbers[1]}_{versionNumbers[2]}.pext";
        }
    }
}