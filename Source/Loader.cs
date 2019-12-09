using System;
using System.IO;
using System.Linq;
using KSP.Localization;
using System.Reflection;
using UnityEngine;

namespace Decalco
{
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    class Loader : MonoBehaviour
    {
        public static readonly string assembly_dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace(Path.DirectorySeparatorChar, '/');
        private static readonly string GameData = assembly_dir.Replace("Decalc'o'mania" + "/Plugins", "");
        private static readonly string textures_dir = assembly_dir.Replace("Plugins", "Textures");
        private static readonly string patch_path = assembly_dir + "/patch.cfg";
        private static bool isRunning = false;

        public void Awake()
        {
            if (isRunning)
            {
                Destroy(this);
                return;
            }
            isRunning = true;
        }

        public void Start()
        {
            if (File.Exists(patch_path))
            {
                File.Delete(patch_path);
            }
            string[] files = new string[0];
            try
            {
                files = Directory.EnumerateFiles(textures_dir, "*.png", SearchOption.AllDirectories).Where(f => new DirectoryInfo(Path.GetDirectoryName(f)).Name != "agencies" && new DirectoryInfo(Path.GetDirectoryName(f)).Name != "templates").ToArray();
            }
            catch (Exception e)
            {
                Exception(e);
            }
            if (files.Length == 0) return;

            #region patch writing
            StreamWriter patch = new StreamWriter(patch_path);
            string[] wides = files.Where(tex => !IsLong(tex)).ToArray();
            string[] longs = files.Where(tex => IsLong(tex)).ToArray();
            try
            {
                // loading the wide decals
                if (wides.Length > 0)
                {
                    patch.WriteLine("@PART[*]:HAS[#tags[cck_decal,wide,*]]:Final{\n@MODULE[ModulePartVariants]{");
                    foreach (string tex in wides)
                    {
                        try
                        {
                            patch.WriteLine(LoadTexture(tex));
                        }
                        catch (Exception e)
                        {
                            Exception(e);
                            patch.Close();
                        }
                    }
                    patch.WriteLine("}}");
                }
                // loading the long decals
                if (longs.Length > 0)
                {
                    patch.WriteLine("@PART[*]:HAS[#tags[cck_decal,long,*]]:Final{\n@MODULE[ModulePartVariants]{");
                    foreach (string tex in longs)
                    {
                        try
                        {
                            patch.WriteLine(LoadTexture(tex));
                        }
                        catch (Exception e)
                        {
                            Exception(e);
                            patch.Close();
                        }
                    }
                    patch.WriteLine("}}");
                }
                patch.Close();
            }
            catch (Exception e)
            {
                Exception(e);
                patch.Close();
            }
            #endregion
        }

        public void OnDestroy()
        {
            if (File.Exists(patch_path))
            {
                File.Delete(patch_path);
            }
        }

        private bool IsLong(string texture)
        {
            Texture2D img = new Texture2D(1, 1);
            img.LoadImage(File.ReadAllBytes(texture));

            return img.height > img.width;
        }

        /// <summary>
        /// Add the content to load an image file in the patch.
        /// </summary>
        /// <param name="texture">The image file path.</param>
        private string[] LoadTexture(string texture)
        {
            Logger.Log("Load(Texture): " + texture.Replace(GameData, "").Replace(Path.GetExtension(texture), "").Replace(Path.DirectorySeparatorChar, '/'));
            string[] lines = new string[] { };
            lines = new string[]
            {
                "VARIANT{",
                string.Format("name = {0}\ndisplayName = {0}\nthemeName = {0}\nprimaryColor = #cc0e0e\nsecondaryColor = #000000", Path.GetFileName(texture).Replace(Path.GetExtension(texture), "")),
                "TEXTURE{",
                string.Format("mainTextureURL = {0}", texture.Replace(GameData, "").Replace(Path.GetExtension(texture), "").Replace(Path.DirectorySeparatorChar, '/')),
                "}}"
            };
            return lines;
        }

        private void Exception(Exception e)
        {
            ScreenMessages.PostScreenMessage($"[{Logger.modName}]: {Localizer.Format("#autoLOC_Decalco_Patch_Err")}", 30, ScreenMessageStyle.UPPER_CENTER, Color.red);
            Logger.Error("An error has occured while writing the patch.", e);
            Destroy(this);
        }
    }
}
