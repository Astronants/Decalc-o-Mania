using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Decalco
{
    internal class TextureHandler
    {
        private static TextureHandler instance = null;
        public static TextureHandler Instance => instance = instance ?? new TextureHandler();

        internal List<string> textures_all = new List<string>();
        private List<string> textures_long = new List<string>();
        private List<string> textures_wide = new List<string>();
        /// <summary>
        /// The cache file is here to prevent to re-write the patch if no changes have been detected.
        /// </summary>
        internal static readonly string cache_file = Path.Combine(Utils.ModDir, "Plugins", "decalco.cache");

        internal enum TextureType
        {
            Wide,
            Long
        }

        internal bool ValidateCache()
        {
            if (!File.Exists(cache_file)) return false;

            using (var sr = new StreamReader(cache_file))
            {
                string header = sr.ReadLine();
                string modVersion = typeof(Loader).Assembly.GetName().Version.ToString();
                if (!string.Equals(header, modVersion)) return false;

                return true;
            }
        }

        /// <summary>
        /// Returns whether or not the textures list and the cache are identical.
        /// </summary>
        internal bool CompareToCache()
        {
            //If the cache file doesn't exist or is from a different version => return false
            if (ValidateCache() == false)
            {
                if (File.Exists(cache_file)) File.Delete(cache_file);
                return false;
            }

            //If the cache file exists but not the config file => delete the cache file
            if (!File.Exists(PatchWriter.patch_path)) File.Delete(cache_file);

            using (var sr = new StreamReader(cache_file))
            {
                List<string> cache_content = sr.ReadToEnd().Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)).Skip(1).ToList();
                //If there is any difference between both lists => return false
                if (cache_content.Except(textures_all).Count() > 0 || textures_all.Except(cache_content).Count() > 0)
                    return false;
            }

            Logger.Log("Cache matches textures list");
            return true;
        }

        internal void CreateCache()
        {
            if (File.Exists(cache_file))
                File.Delete(cache_file);

            using (StreamWriter sw = new StreamWriter(cache_file))
            {
                sw.WriteLine(typeof(Loader).Assembly.GetName().Version.ToString());
                sw.Write(string.Join("\n", textures_all));
            }
        }

        internal void LoadTextures()
        {
            textures_all = Directory.EnumerateFiles(Path.Combine(Utils.ModDir, "Textures"), "*.png", SearchOption.AllDirectories)
                .Where(f => new DirectoryInfo(Path.GetDirectoryName(f)).Name != "agencies" && new DirectoryInfo(Path.GetDirectoryName(f)).Name != "templates")
                .Select(path => path.Replace('\\', '/').Replace(Utils.GameDataDir + '/', ""))
                .ToList();
            
            if (textures_all.Count() == 0)
                return;

            textures_long = textures_all.Where(tex => IsLong(tex)).ToList();
            textures_wide = textures_all.Except(textures_long).ToList();
        }

        /// <exception cref="ArgumentOutOfRangeException"/>
        internal List<string> GetList(TextureType type)
        {
            switch (type)
            {
                case TextureType.Wide:
                    return textures_wide;
                case TextureType.Long:
                    return textures_long;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool IsLong(string texture)
        {
            //Texture2D img = GameDatabase.Instance.GetTexture(texture, false);
            byte[] img_data = File.ReadAllBytes(Path.Combine(Utils.GameDataDir, texture));
            Texture2D img = new Texture2D(2, 2);
            img.LoadImage(img_data);

            return img.height > img.width;
        }
    }
}
