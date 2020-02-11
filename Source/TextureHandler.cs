using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Decalco
{
    class TextureHandler
    {
        private static TextureHandler instance;
        public static TextureHandler Instance => instance;
        private static readonly string texture_dir = Path.Combine(KSPUtil.ApplicationRootPath.Replace('\\', '/'), "GameData/Decalc'o'mania/Textures");
        /// <summary>
        /// The cache file is here to prevent to re-write the patch if no changes have been detected.
        /// </summary>
        private static readonly string cache_file = Path.Combine(Path.GetDirectoryName(PatchWriter.patch_path), "decalco.cache");
        public List<string> textures_all = new List<string>();
        private List<string> textures_long = new List<string>();
        private List<string> textures_wide = new List<string>();

        public enum TextureType
        {
            Wide,
            Long
        }

        public TextureHandler()
        {
            LoadTextures();
            instance = this;
        }

        /// <summary>
        /// Returns whether or not the textures list and the cache are identical.
        /// </summary>
        internal bool CompareToCache()
        {
            //If the cache file exists but not the config file => delete the cache file
            if (!File.Exists(PatchWriter.patch_path) && File.Exists(cache_file))
                File.Delete(cache_file);

            //If the cache file doesn't exists => create a new one and return false;
            if (!File.Exists(cache_file))
            {
                CreateCache();
                return false;
            }

            using (var sr = new StreamReader(cache_file))
            {
                string[] files = sr.ReadToEnd().Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
                string[] alltex = textures_all.Select(elem => elem.Replace(Path.Combine(KSPUtil.ApplicationRootPath.Replace('\\', '/'), "GameData/"), "")).ToArray();
                
                if (files.Except(alltex).Count() > 0 || alltex.Except(files).Count() > 0)
                    return false;
            }

            return true;
        }

        public void CreateCache()
        {
            if (File.Exists(cache_file))
                File.Delete(cache_file);

            using (StreamWriter sw = new StreamWriter(cache_file))
            {
                string[] alltex = textures_all.Select(elem => elem.Replace(Path.Combine(KSPUtil.ApplicationRootPath.Replace('\\', '/'), "GameData/"), "")).ToArray();
                sw.Write(string.Join("\n", alltex));
            }
        }

        internal void LoadTextures()
        {
            textures_all = Directory.EnumerateFiles(texture_dir, "*.png", SearchOption.AllDirectories).Where(f => new DirectoryInfo(Path.GetDirectoryName(f)).Name != "agencies" && new DirectoryInfo(Path.GetDirectoryName(f)).Name != "templates").Select(path => path.Replace('\\', '/')).ToList(); ;

            if (textures_all.Count() == 0)
                return;

            textures_long = textures_all.Where(tex => IsLong(tex)).ToList();
            textures_wide = textures_all.Except(textures_long).ToList();
        }

        /// <exception cref="ArgumentOutOfRangeException"/>
        public List<string> GetList(TextureType type)
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
            Texture2D img = new Texture2D(1, 1);
            img.LoadImage(File.ReadAllBytes(texture));
            return img.height > img.width;
        }
    }
}
