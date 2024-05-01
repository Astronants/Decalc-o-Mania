using System.IO;

namespace Decalco
{
    internal readonly struct DirUtils
    {
        internal static readonly string GameDataDir = UrlDir.CreateApplicationPath("GameData");
        internal static readonly string ModDir = Path.Combine(GameDataDir, "Decalc'o'mania");
        internal static readonly string cache_file = Path.Combine(ModDir, "Plugins", "decalco.cache");
        internal static readonly string patch_file = Path.Combine(ModDir, "Plugins", "patch.cfg");
    }
}
