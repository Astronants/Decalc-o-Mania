using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Decalco
{
    internal class TextureLoader
    {
        internal IEnumerable<string> all;
        internal IEnumerable<string> lng;
        internal IEnumerable<string> wide;

        internal void Load()
        {
            all = Directory.EnumerateFiles(Path.Combine(DirUtils.ModDir, "Textures"), "*.png", SearchOption.AllDirectories)
                .Where(f => new DirectoryInfo(Path.GetDirectoryName(f)).Name != "agencies" && new DirectoryInfo(Path.GetDirectoryName(f)).Name != "templates")
                .Select(path => KSPUtil.GetRelativePath(path, DirUtils.GameDataDir));

            Logger.Log($"{all.Count()} Textures found");
        }

        internal void Sort()
        {
            lng = all.Where(tex => IsLong(tex));
            wide = all.Except(lng);
        }

        private bool IsLong(string texture)
        {
            //Texture2D img = GameDatabase.Instance.GetTexture(texture, false);
            byte[] img_data = File.ReadAllBytes(Path.Combine(DirUtils.GameDataDir, texture));
            Texture2D img = new Texture2D(2, 2);
            img.LoadImage(img_data);

            return img.height > img.width;
        }
    }
}
