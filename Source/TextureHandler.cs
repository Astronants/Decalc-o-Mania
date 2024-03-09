using System;
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
        private List<string> tex_long = new List<string>();
        private List<string> tex_wide = new List<string>();

        internal enum TextureType
        {
            Wide,
            Long
        }

        internal void LoadTextures()
        {
            tex_all = Directory.EnumerateFiles(Path.Combine(DirUtils.ModDir, "Textures"), "*.png", SearchOption.AllDirectories)
                .Where(f => new DirectoryInfo(Path.GetDirectoryName(f)).Name != "agencies" && new DirectoryInfo(Path.GetDirectoryName(f)).Name != "templates")
                .Select(path => path.Replace(DirUtils.GameDataDir + Path.DirectorySeparatorChar, ""))
                .ToList();
            
            if (tex_all.Count() == 0)
            {
                Logger.Log("No textures detected.");
                return;
            }

            tex_long = tex_all.Where(tex => IsLong(tex)).ToList();
            tex_wide = tex_all.Except(tex_long).ToList();
        }

        /// <exception cref="ArgumentOutOfRangeException"/>
        internal List<string> GetList(TextureType type)
        {
            switch (type)
            {
                case TextureType.Wide:
                    return tex_wide;
                case TextureType.Long:
                    return tex_long;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
