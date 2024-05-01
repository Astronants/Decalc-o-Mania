using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Decalco
{
    [KSPAddon(KSPAddon.Startup.Instantly, once:true)]
    class Loader : MonoBehaviour
    {
        private static TextureLoader textures;

        // The patch needs to be created before ModuleManager attempts to apply it.
        // It's not the most reliable way to do this. A custom PartModule updating the part's material on load is preferable.
        public void Awake()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            textures = new TextureLoader();
            textures.Load();

            if (!File.Exists(DirUtils.patch_file) && textures.all.Count() == 0)
            {
                TryDeleteCache();
                Destroy(gameObject);
                return;
            }
            else if (ValidateCache())
            {
                Logger.Log("Patch will be loaded from cache");
                Destroy(gameObject);
                return;
            }
            // Because cache also validates mod version, the contents of the patch could still be the same, even if it returns false.
            // In which case Delcalc'o'mania treats it as being different while ModuleManager would see no changes and load from cache
            // this is normal and expected, and should only happen right after updating the mod

            CreatePatchFile();

            stopwatch.Stop();
            Logger.Log($"Patch created in {stopwatch.ElapsedMilliseconds / 1000.0:F3}s.");
        }

        // Saving the cache is not timing sensitive, it can wait until Start() is called
        public void Start()
        {
            CreateCache();
        }

        private void CreatePatchFile()
        {
            textures.Sort();
            Logger.Log("Creating variants...\n" +
                "Long:\n" +
                $"\t{string.Join("\n\t", textures.lng)}\n" +
                "Wide:\n" +
                $"\t{string.Join("\n\t", textures.wide)}"
                );

            ConfigNode patch = new ConfigNode();
            if (textures.wide.Count() > 0)
                patch.AddNode(NewPatchNode("wide", textures.wide));
            if (textures.lng.Count() > 0)
                patch.AddNode(NewPatchNode("long", textures.lng));

            try
            {
                Logger.Log("Saving patch...");
                patch.Save(DirUtils.patch_file);
                return;
            }
            catch (Exception e)
            {
                Logger.Error("An error occured while saving the patch", e);
            }

            try
            {
                if (File.Exists(DirUtils.patch_file)) File.Delete(DirUtils.patch_file);
                if (File.Exists(DirUtils.cache_file)) File.Delete(DirUtils.cache_file);
            }
            catch (Exception e)
            {
                Logger.Error("An error occured while trying to delete erroneous files", e);
            }
        }

        private ConfigNode NewPatchNode(string typeName, IEnumerable<string> textureList)
        {
            ConfigNode config = new ConfigNode($"@PART[*]:HAS[#tags[cck_decal,{typeName},*]]:Final");
            ConfigNode module = config.AddNode("@MODULE[ModulePartVariants]");
            foreach (string texture in textureList)
            {
                ConfigNode variant = module.AddNode("VARIANT");
                string name = Path.GetFileNameWithoutExtension(texture);
                variant.AddValue("name", name);
                variant.AddValue("displayName", name);
                variant.AddValue("themeName", "Decalcomania");
                variant.AddValue("primaryColor", "#cc0e0e");
                variant.AddValue("secondaryColor", "#000000");
                variant.AddNode("TEXTURE").AddValue("mainTextureURL", UrlDir.StripExtension(texture, ".png"));
            }
            return config;
        }

        // Saves time and prevents writing unnecessarily
        private bool ValidateCache()
        {
            using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
            {
                bool isValid = false;
                if (File.Exists(DirUtils.cache_file) && File.Exists(DirUtils.patch_file))
                {
                    // hash patch file contents
                    byte[] data = File.ReadAllBytes(DirUtils.patch_file);
                    sha.ComputeHash(data);
                    string patchSHA = BitConverter.ToString(sha.Hash);
                    // read cache file
                    ConfigNode cacheConfig = ConfigNode.Load(DirUtils.cache_file);
                    if (cacheConfig != null && cacheConfig.HasValue("version") && cacheConfig.HasValue("patchSHA"))
                    {
                        string version = cacheConfig.GetValue("version");
                        string cachedSHA = cacheConfig.GetValue("patchSHA");
                        string[] cachedTextures = cacheConfig.GetNodes("TEXTURE")
                            .Select(node => node.GetValue("mainTextureURL"))
                            .ToArray();

                        isValid = version.Equals(Logger.Version);
                        isValid &= cachedSHA.Equals(patchSHA);
                        isValid &= cachedTextures.Except(textures.all).Count() == 0;
                        isValid &= textures.all.Except(cachedTextures).Count() == 0;
                    }
                    else Logger.Warn("Failed to read the cache");
                }
                return isValid;
            }
        }

        private void CreateCache()
        {
            using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
            {
                ConfigNode cache = new ConfigNode();

                byte[] data = File.ReadAllBytes(DirUtils.patch_file);
                sha.ComputeHash(data);
                string patchSHA = BitConverter.ToString(sha.Hash);

                cache.AddValue("version", Logger.Version);
                cache.AddValue("patchSHA", patchSHA);
                foreach (string texture in textures.all)
                {
                    cache.AddNode("TEXTURE").AddValue("mainTextureURL", texture);
                }

                try
                {
                    Logger.Log("Saving cache...");
                    cache.Save(DirUtils.cache_file);
                }
                catch (Exception e)
                {
                    Logger.Error("An error occured while saving the cache", e);
                    TryDeleteCache();
                }

            }
        }

        private void TryDeleteCache()
        {
            try
            {
                if (File.Exists(DirUtils.cache_file)) File.Delete(DirUtils.cache_file);
            }
            catch (Exception e)
            {
                Logger.Error("An error occured while trying to delete cache file", e);
            }
        }
    }
}
