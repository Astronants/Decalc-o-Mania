using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Reflection;

namespace Decalco
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    internal class InstallCheck : MonoBehaviour
    {
        internal const string modName = "Decalc'o'mania";
        internal const string directoryName = "Decalc'o'mania";
        internal const string expectedPath = directoryName + "/Plugins";
        private List<string[]> missingDeps = new List<string[]>();
        //GUI
        private GUIStyle green_label, white_label, yellow_label, red_label;
        private bool show_window;
        private Rect window_pos;

        protected void Start()
        {
            #region Check the mod's dll installation.
            var badAssemblies = AssemblyLoader.loadedAssemblies.Where(a => a.assembly.GetName().Name == Assembly.GetExecutingAssembly().GetName().Name && a.url != expectedPath).Select(a => a.path.Replace(Path.GetFullPath(KSPUtil.ApplicationRootPath), ""));
            if (badAssemblies.Any())
            {
                PopupDialog.SpawnPopupDialog
                    (
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    "Installation Check",
                    $"{modName}: Incorrect Installation",
                    $"{modName} has been installed incorrectly and may not work properly.\n\nIncorrect path(s):\n- {String.Join("\n- ", badAssemblies.ToArray())}\n\nAll files should be located in GameData{Path.DirectorySeparatorChar}{directoryName}.",
                    "OK",
                    false,
                    HighLogic.UISkin
                    );
                Logger.Warn($"The mod has been installed incorrectly and may not work properly. Incorrect path(s): \"{String.Join("\", \"", badAssemblies.ToArray())}\". All files should be located in GameData{Path.DirectorySeparatorChar}{directoryName}.");
            }
            #endregion
            #region Check for missing dependencies
            CheckDependency("Module Manager", "ModuleManager", "");
            CheckDependency("Community Category Kit", "CCK", "CommunityCategoryKit");

            if (missingDeps.Count > 0)
            {
                show_window = true;
                Logger.Warn($"One or more dependencies are missing or incorrectly installed: \"{String.Join("\", \"", missingDeps.Select(e => e[0]).ToArray())}\". {modName} requires these mods in order to work properly.");
            }
            #endregion
        }

        private void CheckDependency(string name, string assemblyName, string expectedPath)
        {
            expectedPath = expectedPath.Replace('/', Path.DirectorySeparatorChar);
            var assemblies = AssemblyLoader.loadedAssemblies.Where(a => a.assembly.GetName().Name == assemblyName);
            if (assemblies.Any())
            {
                var wrongPaths = assemblies.Where(a => a.url != expectedPath).Select(a => Path.GetDirectoryName(a.path.Replace(Path.GetFullPath(KSPUtil.ApplicationRootPath), "")));
                if (wrongPaths.Any())
                {
                    missingDeps.Add(new string[] { name, String.Join(", ", wrongPaths), ("GameData" + Path.DirectorySeparatorChar + expectedPath).TrimEnd(Path.DirectorySeparatorChar) });
                }
            }
            else
            {
                missingDeps.Add(new string[] { name });
            }

        }

        public void OnGUI()
        {
            LoadStyles();
            if (show_window)
            {
                window_pos = GUILayout.Window(this.GetInstanceID(), this.window_pos, this.Window, modName, HighLogic.Skin.window);
            }
        }

        private void LoadStyles()
        {
            green_label = new GUIStyle(HighLogic.Skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = true
            };
            white_label = new GUIStyle(HighLogic.Skin.label)
            {
                normal =
                {
                    textColor = Color.white
                },
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = true
            };
            yellow_label = new GUIStyle(HighLogic.Skin.label)
            {
                normal =
                {
                    textColor = Color.yellow
                },
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = true
            };
            red_label = new GUIStyle(HighLogic.Skin.label)
            {
                normal =
                {
                    textColor = Color.red
                },
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = true
            };
        }

        private void Window(int id)
        {
            GUILayout.BeginVertical(GUILayout.Width(400f));
            GUILayout.Label("One or more dependencies are missing or installed incorrectly.", new GUIStyle(white_label) { fontStyle= FontStyle.Bold });
            #region legend
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Legend:", white_label);
            GUILayout.FlexibleSpace();
            GUILayout.Label("missing addon", red_label);
            GUILayout.FlexibleSpace();
            GUILayout.Label("incorrectly installed addon", yellow_label);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            #endregion
            #region addon list
            GUILayout.BeginVertical(HighLogic.Skin.box);
            foreach (var addon in missingDeps)
            {
                if (addon.Count() == 3)
                {
                    GUILayout.Label(addon[0], new GUIStyle(yellow_label) { fontStyle = FontStyle.Bold });
                    GUILayout.BeginVertical(HighLogic.Skin.box);
                    GUILayout.Label("Incorrect path(s):", green_label);
                    GUILayout.Label(addon[1], white_label);
                    GUILayout.Label("Expected path:", green_label);
                    GUILayout.Label(addon[2], white_label);
                    GUILayout.EndVertical();
                }
                else
                {
                    GUILayout.Label(addon[0], new GUIStyle(red_label) { fontStyle = FontStyle.Bold });
                }
            }
            GUILayout.EndVertical();
            #endregion
            GUILayout.Label($"{modName} requires these addons in order to work properly.", new GUIStyle(white_label) { fontStyle = FontStyle.Bold });
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("OK", HighLogic.Skin.button))
            {
                show_window = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}
