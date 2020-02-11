using System;
using System.Linq;
using UnityEngine;

namespace Decalco
{
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    class Loader : MonoBehaviour
    {
        PatchWriter patchWriter = new PatchWriter();
        TextureHandler textureHandler = new TextureHandler();

        public void Start()
        {
            textureHandler.LoadTextures();

            if (textureHandler.CompareToCache() == true || (!System.IO.File.Exists(PatchWriter.patch_path) && textureHandler.textures_all.Count() == 0))
            {
                Destroy(this);
                return;
            }
            textureHandler.CreateCache();
            patchWriter.Initialize();

            foreach (var type in Enum.GetValues(typeof(TextureHandler.TextureType)).Cast<TextureHandler.TextureType>())
            {
                foreach (string texture in textureHandler.GetList(type))
                {
                    string textureWithoutExt = texture.Replace(System.IO.Path.GetExtension(texture), "");

                    Logger.Log("Load(Texture): " + texture.Replace(System.IO.Path.Combine(KSPUtil.ApplicationRootPath.Replace('\\', '/'), "GameData/"), ""));

                    patchWriter.AddLinesToType(type, new string[] {
                        "VARIANT{",
                        string.Format("name = {0}\ndisplayName = {0}\nthemeName = {0}\nprimaryColor = #cc0e0e\nsecondaryColor = #000000", System.IO.Path.GetFileName(textureWithoutExt)),
                        "TEXTURE{",
                        "mainTextureURL = " + textureWithoutExt.Replace(System.IO.Path.Combine(KSPUtil.ApplicationRootPath.Replace('\\', '/'), "GameData/"), ""),
                        "}}"
                    });
                }
            }

            patchWriter.EndPatch();
            patchWriter.WritePatch();
        }

    }
}
