using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Reflection;

namespace APW
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    internal class InstallCheck : MonoBehaviour
    {
        static bool isRunning;
        internal const string modName = "Auto Physics Warp";
        internal const string directoryName = "AutoPhysicsWarp";
        internal const string expectedPath = directoryName;
        private List<Dependency> missingDeps = new List<Dependency>();
        //GUI
        private GUIStyle green_label, white_label, yellow_label, red_label;
        private bool show_window;
        private Rect window_pos;

        public void Awake()
        {
            if (isRunning)
            {
                Destroy(this);
            }
            isRunning = true;
        }

        protected void Start()
        {
            if (Assembly.GetExecutingAssembly() == null) return;
            Logger.Log($"InstallCheck: {Assembly.GetExecutingAssembly().GetName().Name} v{Assembly.GetExecutingAssembly().GetName().Version}");
            #region Check the mod's dll installation.
            IEnumerable<AssemblyLoader.LoadedAssembly> assemblies = AssemblyLoader.loadedAssemblies.Where(a => a.assembly.GetName().Name == Assembly.GetExecutingAssembly().GetName().Name);
            IEnumerable<string> wrongPaths = assemblies.Where(a => a.url != expectedPath).Select(a => a.path.Replace(Path.GetFullPath(KSPUtil.ApplicationRootPath), ""));
            if (wrongPaths.Any())
            {
                
                PopupDialog.SpawnPopupDialog
                    (
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    "Installation Check",
                    $"{modName}: Incorrect Installation",
                    $"{modName} has been installed incorrectly and may not work properly.\n\nIncorrect path(s):\n- {String.Join("\n- ", wrongPaths.ToArray())}\n\nAll files should be located in GameData{Path.DirectorySeparatorChar}{directoryName}.",
                    "OK",
                    false,
                    HighLogic.UISkin
                    );
                Logger.Warn($"The mod has been installed incorrectly and may not work properly.\nIncorrect path(s):\n- {String.Join("\n- ", wrongPaths.ToArray())}.\nAll files should be located in GameData{Path.DirectorySeparatorChar}{directoryName}.");
            }
            #endregion
            #region Check for missing dependencies
            //No dependencies

            if (missingDeps.Any())
            {
                show_window = true;
                Logger.Warn($"One or more dependencies are missing or incorrectly installed:\n- {String.Join("\n- ", missingDeps.Select(e => e.InfoString).ToArray())}.\n{modName} requires these mods in order to work properly.");
            }
            #endregion
        }

        private void CheckDependency(string name, string assembly, string expectedPath)
        {
            expectedPath = expectedPath.Replace('/', Path.DirectorySeparatorChar);
            IEnumerable<AssemblyLoader.LoadedAssembly> assemblies = AssemblyLoader.loadedAssemblies.Where(a => a.assembly.GetName().Name == assembly);
            if (assemblies.Any())
            {
                IEnumerable<string> wrongPaths = assemblies.Where(a => a.url != expectedPath).Select(a => Path.GetDirectoryName(a.path.Replace(Path.GetFullPath(KSPUtil.ApplicationRootPath), "")));
                if (wrongPaths.Any())
                {
                    Dependency dependency = new Dependency(name);
                    dependency.SetPaths(wrongPaths, ("GameData" + Path.DirectorySeparatorChar + expectedPath).TrimEnd(Path.DirectorySeparatorChar));
                    missingDeps.Add(dependency);
                }
            }
            else
            {
                Dependency dependency = new Dependency(name);
                missingDeps.Add(dependency);
            }
        }

        public void OnGUI()
        {
            if (show_window)
            {
                LoadStyles();
                window_pos = GUILayout.Window(this.GetInstanceID(), this.window_pos, this.Window, modName, HighLogic.Skin.window, GUILayout.Height(0f));
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
                fontStyle = FontStyle.Bold,
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
                fontStyle = FontStyle.Bold,
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
                GUILayout.BeginHorizontal();
                GUILayout.Label(addon.Name, new GUIStyle(addon.IsIncorrectlyInstalled ? yellow_label : red_label));
                if (addon.IsIncorrectlyInstalled)
                {
                    addon.Details_button();
                }
                GUILayout.EndHorizontal();
                if (addon.IsIncorrectlyInstalled)
                {
                    if (addon.ShowDetails)
                    {
                        GUILayout.BeginVertical(HighLogic.Skin.box);
                        GUILayout.Label("Incorrect path(s):", green_label);
                        GUILayout.Label(addon.GetWrongPaths(), white_label);
                        GUILayout.Label("Expected path:", green_label);
                        GUILayout.Label(addon.GetExpectedPath(), white_label);
                        GUILayout.EndVertical();
                    }
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

    class Dependency
    {
        public Dependency(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; } = "";
        public bool ShowDetails { get; set; } = false;
        public bool IsIncorrectlyInstalled { get; private set; } = false;
        private IEnumerable<string> wrongPaths;
        private string expectedPath;

        public void SetPaths(IEnumerable<string> wp, string ep)
        {
            this.IsIncorrectlyInstalled = true;
            this.wrongPaths = wp;
            this.expectedPath = ep;
        }

        public string GetWrongPaths()
        {
            return string.Join(", ", this.wrongPaths);
        }

        public string GetExpectedPath()
        {
            return this.expectedPath;
        }

        public string InfoString
        {
            get
            {
                if (this.IsIncorrectlyInstalled)
                {
                    return this.Name + ": " + this.GetWrongPaths() + ". Expected path: " + this.GetExpectedPath();
                }
                else
                {
                    return this.Name;
                }
            }
        }

        public void Details_button()
        {
            if (!this.ShowDetails)
            {
                if (GUILayout.Button("+", new GUIStyle(HighLogic.Skin.button) { padding = new RectOffset(10, 10, HighLogic.Skin.button.padding.top, HighLogic.Skin.button.padding.bottom) }, GUILayout.ExpandWidth(false)))
                {
                    this.ShowDetails = true;
                }
            }
            else
            {
                if (GUILayout.Button("-", new GUIStyle(HighLogic.Skin.button) { padding = new RectOffset(12, 12, HighLogic.Skin.button.padding.top, HighLogic.Skin.button.padding.bottom) }, GUILayout.ExpandWidth(false)))
                {
                    this.ShowDetails = false;
                }
            }
        }
    }
}
