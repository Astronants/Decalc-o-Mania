using System.IO;

namespace Decalco
{
    internal class DirUtils
    {
        internal static readonly string GameDataDir = UrlDir.CreateApplicationPath("GameData");
        internal static readonly string ModDir = Path.Combine(GameDataDir, "Decalc'o'mania");
    }
}
