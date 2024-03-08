using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Decalco
{
    internal class PatchWriter
    {
        private static PatchWriter instance = null;
        public static PatchWriter Instance => instance = instance ?? new PatchWriter();

        internal static readonly string patch_path = Path.Combine(Utils.ModDir, "Plugins", "patch.cfg");
        private readonly Dictionary<string, List<string>> patchContent = new Dictionary<string, List<string>>();
        
        /// <summary>
        /// Initialize the PatchWriter with the first lines of each type
        /// </summary>
        internal void Initialize()
        {
            foreach (var type in Enum.GetValues(typeof(TextureHandler.TextureType)).Cast<TextureHandler.TextureType>())
            {
                string typeName = Enum.GetName(typeof(TextureHandler.TextureType), type).ToLower();
                if (TextureHandler.Instance.GetList(type).Count() > 0)
                {
                    patchContent[typeName] = new List<string>()
                    {
                        $"@PART[*]:HAS[#tags[cck_decal,{typeName},*]]:Final",
                        "{\n@MODULE[ModulePartVariants]{"
                    };
                }
            }
        }

        /// <exception cref="ArgumentOutOfRangeException"/>
        internal void AddLinesToType(TextureHandler.TextureType type, string[] lines)
        {
            string typeName = Enum.GetName(typeof(TextureHandler.TextureType), type).ToLower();

            if (!patchContent.ContainsKey(typeName))
                throw new ArgumentOutOfRangeException();

            foreach (string line in lines)
            {
                patchContent[typeName].Add(line);
            }
        }

        /// <summary>
        /// Add the final lines for each decal type to the PatchWriter
        /// </summary>
        internal void EndPatch()
        {
            foreach (var type in Enum.GetValues(typeof(TextureHandler.TextureType)).Cast<TextureHandler.TextureType>())
            {
                string typeName = Enum.GetName(typeof(TextureHandler.TextureType), type).ToLower();
                if (patchContent.ContainsKey(typeName))
                {
                    patchContent[typeName].Add("}}");
                }
            }
        }

        internal void WritePatch()
        {
            try
            {
                if (File.Exists(patch_path))
                    File.Delete(patch_path);

                using (StreamWriter patch = new StreamWriter(patch_path))
                {
                    List<string> lines = new List<string>();
                    foreach (KeyValuePair<string, List<string>> pair in patchContent)
                    {
                        lines.Add(string.Join("\n", pair.Value));
                    }
                    patch.Write(string.Join("\n", lines));
                }

                PopupDialog dialog = PopupDialog.SpawnPopupDialog(
                    new MultiOptionDialog("DecalcoPatchSuccess",
                        "The patch was successfully updated! Restart the game to apply the changes.",
                        Logger.modName, HighLogic.UISkin,
                        new DialogGUIButton("OK", () => dialog = null)),
                    true,
                    HighLogic.UISkin);
                Logger.Log("Patch written successfully!");
            }
            catch (Exception e)
            {
                ScreenMessages.PostScreenMessage(Logger.logPrefix + KSP.Localization.Localizer.Format("#autoLOC_Decalco_patch_Err"), 5f, ScreenMessageStyle.UPPER_CENTER, Color.red);
                throw new Exception(KSP.Localization.Localizer.Format(Logger.logPrefix + "#autoLOC_Decalco_patch_Err"), e);
            }
        }

    }
}
