﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Decalco
{
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    class Loader : MonoBehaviour
    {
        internal static readonly string cache_file = Path.Combine(DirUtils.ModDir, "Plugins", "decalco.cache");
        internal static readonly string patch_file = Path.Combine(DirUtils.ModDir, "Plugins", "patch.cfg");

        public void Awake()
        {
            TextureHandler.Instance.LoadTextures();
            bool useCache = ValidateCache();

            if ((!File.Exists(patch_file) && TextureHandler.Instance.tex_all.Count() == 0) || useCache)
            {
                Destroy(this);
                if (useCache)
                    Logger.Log("Patch will be loaded from cache");
                else
                    TryDeleteCache();

                return;
            }

            TextureHandler.Instance.SortTextures();
            CreatePatchFile();
        }

        private void CreatePatchFile()
        {
            ConfigNode patch = new ConfigNode();
            if (TextureHandler.Instance.tex_wide.Count() > 0)
                patch.AddNode(CreatePatchNode("wide", TextureHandler.Instance.tex_wide));
            if (TextureHandler.Instance.tex_long.Count() > 0)
                patch.AddNode(CreatePatchNode("long", TextureHandler.Instance.tex_long));
            try
            {
                Logger.Log("Saving patch...");
                patch.Save(patch_file);
                CreateCache();
                /*PopupDialog dialog = PopupDialog.SpawnPopupDialog(
                new MultiOptionDialog("DecalcoPatchSuccess",
                    "The patch was successfully updated! Restart the game to apply the changes.",
                    Logger.modName, HighLogic.UISkin,
                    new DialogGUIButton("OK", () => dialog = null)),
                true,
                HighLogic.UISkin);*/
                return;
            }
            catch (Exception e)
            {
                Logger.Error("An error occured while creating the patch", e);
            }

            try
            {
                if (File.Exists(patch_file)) File.Delete(patch_file);
                if (File.Exists(cache_file)) File.Delete(cache_file);
            }
            catch (Exception e)
            {
                Logger.Error("An error occured while trying to delete erroneous files", e);
            }
        }

        private ConfigNode CreatePatchNode(string typeName, List<string> textureList)
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

        private bool ValidateCache()
        {
            using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
            {
                bool isValid = false;
                if (File.Exists(cache_file) && File.Exists(patch_file))
                {
                    // hash patch file contents
                    byte[] contentBytes = File.ReadAllBytes(patch_file);
                    sha.ComputeHash(contentBytes);
                    string patchSHA = BitConverter.ToString(sha.Hash);
                    // read cache file
                    ConfigNode cacheConfig = ConfigNode.Load(cache_file);
                    if (cacheConfig != null && cacheConfig.HasValue("version") && cacheConfig.HasValue("patchSHA"))
                    {
                        string version = cacheConfig.GetValue("version");
                        string cachedSHA = cacheConfig.GetValue("patchSHA");
                        List<string> textures = new List<string>();
                        ConfigNode[] textureNodes = cacheConfig.GetNodes("TEXTURE");
                        foreach (ConfigNode node in textureNodes)
                        {
                            textures.Add(node.GetValue("mainTextureURL"));
                        }

                        isValid = version.Equals(Logger.modVersion);
                        isValid &= cachedSHA.Equals(patchSHA);
                        isValid &= textures.Except(TextureHandler.Instance.tex_all).Count() == 0;
                        isValid &= TextureHandler.Instance.tex_all.Except(textures).Count() == 0;
                    }
                    else Logger.Warn("Unable to read the cache");
                }
                return isValid;
            }
        }

        private void CreateCache()
        {
            using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
            {
                ConfigNode cache = new ConfigNode();

                byte[] contentBytes = File.ReadAllBytes(patch_file);
                sha.ComputeHash(contentBytes);
                string patchSHA = BitConverter.ToString(sha.Hash);

                cache.AddValue("version", Logger.modVersion);
                cache.AddValue("patchSHA", patchSHA);
                foreach (string texture in TextureHandler.Instance.tex_all)
                {
                    cache.AddNode("TEXTURE").AddValue("mainTextureURL", texture);
                }

                try
                {
                    Logger.Log("Saving cache...");
                    cache.Save(cache_file);
                    return;
                }
                catch (Exception e)
                {
                    Logger.Error("An error occured while saving the cache", e);
                }

                TryDeleteCache();
            }
        }

        private void TryDeleteCache()
        {
            try
            {
                if (File.Exists(cache_file)) File.Delete(cache_file);
            }
            catch (Exception e)
            {
                Logger.Error("An error occured while trying to delete cache file", e);
            }
        }
    }
}
