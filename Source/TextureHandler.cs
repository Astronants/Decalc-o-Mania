using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Decalco
{
    internal class TextureHandler
    {
        private static TextureHandler instance = null;
        public static TextureHandler Instance => instance = instance ?? new TextureHandler();

        internal List<string> tex_all = new List<string>();
        internal List<string> tex_long = new List<string>();
        internal List<string> tex_wide = new List<string>();

        internal void LoadTextures()
        {
            tex_all = Directory.EnumerateFiles(Path.Combine(DirUtils.ModDir, "Textures"), "*.png", SearchOption.AllDirectories)
                .Where(f => new DirectoryInfo(Path.GetDirectoryName(f)).Name != "agencies" && new DirectoryInfo(Path.GetDirectoryName(f)).Name != "templates")
                .Select(path => path.Replace('\\', '/').Replace(DirUtils.GameDataDir + '/', "")).ToList();

            Logger.Log($"{tex_all.Count()} Textures found");

            if (tex_all.Count() == 0) return;

            tex_long = tex_all.Where(tex => IsLong(tex)).ToList();
            tex_wide = tex_all.Except(tex_long).ToList();
        }

        private bool IsLong(string texture)
        {
            Logger.Log("Load(Texture): " + texture);
            //Texture2D img = GameDatabase.Instance.GetTexture(texture, false); // <- I'd rather use KSP's database, but as long as i rely on ModuleManager to apply the variants I can't afford to wait for the game to load them first
            byte[] img_data = File.ReadAllBytes(Path.Combine(DirUtils.GameDataDir, texture));
            Texture2D img = new Texture2D(2, 2);
            img.LoadImage(img_data);

            return img.height > img.width;
        }
    }
}
