using System;
using System.Linq;
using UnityEngine;

namespace Decalco
{
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    class Loader : MonoBehaviour
    {
        public void Start()
        {
            TextureHandler.Instance.LoadTextures();

            if ((!System.IO.File.Exists(PatchWriter.patch_path) && TextureHandler.Instance.textures_all.Count() == 0) || TextureHandler.Instance.CompareToCache() == true)
            {
                Destroy(this);
                return;
            }

            TextureHandler.Instance.CreateCache();
            PatchWriter.Instance.Initialize();

            foreach (var type in Enum.GetValues(typeof(TextureHandler.TextureType)).Cast<TextureHandler.TextureType>())
            {
                foreach (string texture in TextureHandler.Instance.GetList(type))
                {
                    string textureWithoutExt = texture.Replace(System.IO.Path.GetExtension(texture), "");

                    Logger.Log("Load(Texture): " + texture);

                    PatchWriter.Instance.AddLinesToType(type, new string[] {
                        "VARIANT{",
                        string.Format("name = {0}\ndisplayName = {0}\nthemeName = {0}\nprimaryColor = #cc0e0e\nsecondaryColor = #000000", System.IO.Path.GetFileName(textureWithoutExt)),
                        "TEXTURE{",
                        "mainTextureURL = " + textureWithoutExt,
                        "}}"
                    });
                }
            }

            PatchWriter.Instance.EndPatch();
            PatchWriter.Instance.WritePatch();
        }

    }
}
