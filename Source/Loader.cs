using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Decalco
{
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    class Loader : MonoBehaviour
    {
        /// <summary>
        /// The cache file is here to prevent to re-write the patch if no changes have been detected.
        /// </summary>
        internal static readonly string cache_file = Path.Combine(DirUtils.ModDir, "Plugins", "decalco.cache");

        public void Start()
        {
            TextureHandler.Instance.LoadTextures();

            if ((!System.IO.File.Exists(PatchWriter.patch_path) && TextureHandler.Instance.tex_all.Count() == 0) || CompareToCache() == true)
            {
                Destroy(this);
                return;
            }

            PatchWriter.Instance.Initialize();

            foreach (var type in Enum.GetValues(typeof(TextureHandler.TextureType)).Cast<TextureHandler.TextureType>())
            {
                foreach (string texture in TextureHandler.Instance.GetList(type))
                {
                    Logger.Log("Load(Texture): " + texture);
                    PatchWriter.Instance.AddTextureToType(type, texture.Replace(System.IO.Path.GetExtension(texture), ""));
                }
            }

            PatchWriter.Instance.EndPatch();
            PatchWriter.Instance.WritePatch();
            CreateCache();
        }

        internal bool ValidateCache()
        {
            if (!File.Exists(cache_file)) return false;
            // validate cache's header
            using (var sr = new StreamReader(cache_file))
            {
                string[] header = sr.ReadLine().Split(' ');
                if (header.Length < 2) return false;

                // compare mod versions to ensure patches are up to date with every mod update
                if (!string.Equals(header[0], Logger.modVersion)) return false;

                // compare patch's hash with cached value
                using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
                {
                    byte[] contentBytes = File.ReadAllBytes(PatchWriter.patch_path);
                    sha.ComputeHash(contentBytes);
                    string filesha = BitConverter.ToString(sha.Hash);
                    if (!string.Equals(header[1], filesha)) return false;
                }
            }

            return true;
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
                if (cache_content.Except(TextureHandler.Instance.tex_all).Count() > 0 || TextureHandler.Instance.tex_all.Except(cache_content).Count() > 0)
                    return false;
            }

            Logger.Log("Cache matches textures list and patch file.");
            return true;
        }
        internal void CreateCache()
        {
            if (File.Exists(cache_file))
                File.Delete(cache_file);

            using (StreamWriter sw = new StreamWriter(cache_file))
            {
                // write cache header
                using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
                {
                    byte[] contentBytes = File.ReadAllBytes(PatchWriter.patch_path);
                    sha.ComputeHash(contentBytes);
                    string filesha = BitConverter.ToString(sha.Hash);
                    sw.WriteLine($"{Logger.modVersion} {filesha}");
                }
                // write texture list
                sw.Write(string.Join("\n", TextureHandler.Instance.tex_all));
            }
        }

    }
}
