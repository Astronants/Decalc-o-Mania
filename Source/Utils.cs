using System.IO;

namespace Decalco
{
    internal class Utils
    {
        internal static readonly string GameDataDir = UrlDir.CreateApplicationPath("GameData").Replace('\\', '/');
        internal static readonly string ModDir = Path.Combine(GameDataDir, "Decalc'o'mania");
    }
}
