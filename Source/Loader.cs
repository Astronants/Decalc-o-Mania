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
        public readonly string modName = "Decalc'o'mania";
        private string assembly_loc;
        private string assembly_dir;
        private string textures_dir;
        private string GD_dir;
        private string patch_path;

        public void Awake()
        {
            assembly_loc = Assembly.GetExecutingAssembly().Location.Replace("\\", "/");
            assembly_dir = Path.GetDirectoryName(assembly_loc).Replace("\\", "/");
            GD_dir = assembly_dir.Replace(modName + "/Plugin", "");
            textures_dir = assembly_dir.Replace("Plugin", "Textures");
            patch_path = assembly_dir + "/patch.cfg";
            string[] files = Directory.GetFiles(textures_dir);

            // removing the files that are not png files
            files = files.Where(file => Path.GetExtension(file) == ".png").ToArray();
            if (files.Length == 0) return;
            //--------------------------------------

            // writing the patch
            StreamWriter patch = new StreamWriter(patch_path);
            string[] wides = files.Where(tex => !isLong(tex)).ToArray();
            string[] longs = files.Where(tex => isLong(tex)).ToArray();
            try
            {
                Log("writing patch...");
                // loading the wide decals
                if (wides.Length > 0)
                {
                    patch.WriteLine("@PART[*]:HAS[#tags[cck_decal,*],~tags[*,long,*],@MODULE[ModulePartVariants]]:FINAL{\n@MODULE[ModulePartVariants]{");
                    foreach (string tex in wides)
                    {
                        if (!isLong(tex))
                        {
                            LoadTexture(patch, tex);
                        }
                    }
                    patch.WriteLine("}}");
                }

                // loading the long decals
                if (longs.Length > 0)
                {
                    patch.WriteLine("@PART[*]:HAS[#tags[cck_decal,long,*],@MODULE[ModulePartVariants]]:FINAL{\n@MODULE[ModulePartVariants]{");
                    foreach (string tex in longs)
                    {
                        if (isLong(tex))
                        {
                            LoadTexture(patch, tex);
                        }
                    }
                    patch.WriteLine("}}");
                }

                // closing the streamwriter
                patch.Close();
                Log("patch writed!");

            }
            catch (Exception e)
            {
                ScreenMessages.PostScreenMessage($"[{modName}]: {Localizer.Format("#autoLOC_Decalco_Patch_Err")}", 60, ScreenMessageStyle.UPPER_CENTER, Color.red);
                Log("there was an error while writing the patch!");
                Debug.LogError(e.Message);

                // closing the streamwriter and destroying the Loader
                patch.Close();
                Destroy(this);
            }

        }

        public void OnDestroy()
        {
            File.Delete(patch_path);
        }

        private bool isLong(string texture)
        {
            Texture2D img = new Texture2D(1, 1);
            img.LoadImage(File.ReadAllBytes(texture));

            return img.height > img.width;
        }

        private void LoadTexture(StreamWriter patch, string texture)
        {
            Log("Load(Texture): " + texture.Replace("\\", "/").Replace(GD_dir, ""));
            patch.WriteLine("VARIANT{");
            patch.WriteLine(string.Format("name = {0}\ndisplayName = {0}\nthemeName = {0}\nprimaryColor = #cc0e0e\nsecondaryColor = #000000", Path.GetFileName(texture).Replace(Path.GetExtension(texture), "")));
            patch.WriteLine("TEXTURE{");
            patch.WriteLine(string.Format("mainTextureURL = {0}", texture.Replace("\\", "/").Replace(GD_dir, "").Replace(Path.GetExtension(texture), "")));
            patch.WriteLine("}\n}");
        }

        private void Log(string text)
        {
            Debug.Log($"{modName} => {text}");
        }
    }
}
