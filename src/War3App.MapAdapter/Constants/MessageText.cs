namespace War3App.MapAdapter.Constants
{
    public static class MessageText
    {
        public const string MissingFiles =  "Directory does not contain all files required for adapting. The following files could not be found:";
        public const string GetHelp = @"If you run into an error while trying to adapt a map, please create an issue at https://github.com/Drake53/War3App/issues and include a .zip file, which you can create by clicking the button below.
This .zip file will contain the map file, the target patch version, and the game files which belong to that patch version in order to reproduce your issue.

If the map belongs to you and you use map protection, try to reproduce the issue on the protected version first.
If the issue can only be reproduced on the unprotected version, you can enable the 'Encrypt map file' setting, which will encrypt the map file with RSA/AES, so only the author of this tool can open the map.";
    }
}