using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Reflection;
using System.Text;

namespace Decalco
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    internal class InstallCheck : MonoBehaviour
    {
        internal const string expectedPath = "Decalc'o'mania/Plugins";
        internal const string fileName = "Decalco";
        private List<Dependency> missingDeps = new List<Dependency>();
        
        private GUIStyle green_label, white_label, yellow_label, red_label;
        private bool dp_window;
        private Rect dp_window_pos = new Rect();
        string tooltip = "";
        private Rect tooltipDialog = new Rect();
        Vector2 tooltipSize = new Vector2();

        int NamesColWidth, ExpPathColWidth, StateColWidth;

        protected void Start()
        {
            Debug.Log($"\nInstallCheck: Checking \"{Logger.modName}\" installation...");
            CheckInstall();

            #region Look for missing or incorrectly installed dependencies
            CheckDependency("Module Manager", "ModuleManager", "", "https://forum.kerbalspaceprogram.com/index.php?/topic/50533-18x-module-manager-413-november-30th-2019-right-to-ludicrous-speed/");
            CheckDependency("Community Category Kit", "CCK", "CommunityCategoryKit", "https://forum.kerbalspaceprogram.com/index.php?/topic/149840-discussion-community-category-kit/");
            
            if (missingDeps.Any())
            {
                dp_window = true;
                LoadStyles();
                StringBuilder modsTable = new StringBuilder();
                modsTable.Append("One or more dependencies are missing or incorrectly installed:\n");

                CalcColsWidth();
                string format = "  {0,-" + (NamesColWidth + 8) + "}{1,-" + (StateColWidth + 8) + "}{2,-" + (ExpPathColWidth + 8) + "}{3}\n";
                modsTable.AppendFormat(
                    format,
                    "Name:",
                    "State:",
                    "Expected Path:",
                    "Wrong Path(s):"
                    );
                foreach (Dependency addon in missingDeps)
                {
                    modsTable.AppendFormat(
                        format,
                        addon.Name,
                        addon.StateString,
                        addon.State == Dependency.States.IncorrectInstall ? addon.GetExpectedPath() : "",
                        addon.State == Dependency.States.IncorrectInstall ? addon.GetWrongPaths() : ""
                        );
                }
                Debug.Log(modsTable.ToString());
            }
            else
            {
                Debug.Log("No missing dependency found.\n");
            }
            #endregion
            
            Debug.Log($"InstallCheck: \"{Logger.modName}\" installation checked succesfully.");
        }

        private void CheckInstall()
        {
            IEnumerable<string> wrongPaths = AssemblyLoader.loadedAssemblies.Where(a => a.assembly.GetName().Name == Assembly.GetExecutingAssembly().GetName().Name && a.url != expectedPath).Select(a => a.path.Replace(Path.GetFullPath(KSPUtil.ApplicationRootPath), "").Replace('\\', '/'));
            if (wrongPaths.Any())
            {
                PopupDialog popup = PopupDialog.SpawnPopupDialog(
                    new MultiOptionDialog(
                    "InstallCheck",
                    $"{Logger.modName} has been installed incorrectly and may not work properly.\n\nIncorrect path(s):\n- {String.Join("\n- ", wrongPaths.ToArray())}\n\nExpected path:\n- GameData/{expectedPath}/{fileName}.dll",
                    $"{Logger.modName}: Incorrect Installation",
                    HighLogic.UISkin,
                    new DialogGUIButton("OK", () => popup = null)
                    ),
                    false,
                    HighLogic.UISkin
                    );
                Debug.Log($"The mod has been installed incorrectly and may not work properly.\nIncorrect path(s):\n- {String.Join("\n- ", wrongPaths.ToArray())}.\nExpected path:\n- GameData/{expectedPath}/{fileName}.dll\n");
            }
            else
            {
                Debug.Log("Addon installed correctly.\n");
            }
        }

        private void CheckDependency(string name, string assembly, string expectedPath, string homePage = null)
        {
            IEnumerable<AssemblyLoader.LoadedAssembly> assemblies = AssemblyLoader.loadedAssemblies.Where(a => a.assembly.GetName().Name == assembly);
            if (assemblies.Any())
            {
                IEnumerable<string> wrongPaths = assemblies.Where(a => a.url != expectedPath).Select(a => Path.GetDirectoryName(a.path.Replace(Path.GetFullPath(KSPUtil.ApplicationRootPath), "")));
                if (wrongPaths.Any())
                {
                    Dependency dependency = new Dependency(name);
                    dependency.SetPaths(wrongPaths.Select(p => p.Replace('\\', '/')), ("GameData/" + expectedPath).TrimEnd('/'));
                    if (homePage != null) dependency.SetUrl(homePage);
                    missingDeps.Add(dependency);
                }
            }
            else
            {
                Dependency dependency = new Dependency(name);
                if (homePage != null) dependency.SetUrl(homePage);
                missingDeps.Add(dependency);
            }
        }

        private void CalcColsWidth()
        {
            NamesColWidth = 5;
            ExpPathColWidth = 14;
            StateColWidth = 6;
            foreach (Dependency addon in missingDeps)
            {
                if (addon.Name.Length > NamesColWidth)
                {
                    NamesColWidth = addon.Name.Length;
                }
                if (addon.State == Dependency.States.IncorrectInstall && addon.GetExpectedPath().Length > ExpPathColWidth)
                {
                    ExpPathColWidth = addon.GetExpectedPath().Length;
                }
                if (addon.StateString.Length > StateColWidth)
                {
                    StateColWidth = addon.StateString.Length;
                }
            }
        }

        public void OnGUI()
        {
            if (dp_window)
            {
                dp_window_pos = GUILayout.Window(this.GetInstanceID(), this.dp_window_pos, this.DPWindow, Logger.modName, HighLogic.Skin.window, GUILayout.Height(0));
            }

            if (tooltip != "")
            {
                tooltipSize = green_label.CalcSize(new GUIContent(tooltip));
                tooltipDialog.position = new Vector2(Input.mousePosition.x + 10f, (Screen.height - Input.mousePosition.y) + 10f);
                tooltipDialog = GUILayout.Window(this.GetInstanceID() + 12565, this.tooltipDialog, this.ShowTooltip, (string)null, new GUIStyle(HighLogic.Skin.window) { padding = new RectOffset(5, 5, 5, 5) });
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

        private void DPWindow(int id)
        {
            GUILayout.BeginVertical(GUILayout.Width(400));
            GUILayout.Label("One or more dependencies are missing or installed incorrectly.", new GUIStyle(white_label) { fontStyle = FontStyle.Bold });
            #region legend
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Legend:", white_label);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Missing", red_label);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Incorrectly Installed", yellow_label);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            #endregion
            #region addon list
            GUILayout.BeginVertical(HighLogic.Skin.box);
            foreach (var addon in missingDeps)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(addon.Name, new GUIStyle(addon.State == Dependency.States.Missing ? red_label : yellow_label));
                if (addon.State == Dependency.States.Missing && addon.Url != null)
                {
                    bool HomePageButton = GUILayout.Button("HP", HighLogic.Skin.button, GUILayout.Width(28f));
                    if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                    {
                        tooltip = addon.Url;
                    }
                    else
                    {
                        tooltip = "";
                    }

                    if (HomePageButton)
                    {
                        Application.OpenURL(addon.Url); //Opens the dependency homepage
                    }
                }
                else if (addon.State == Dependency.States.IncorrectInstall)
                {
                    if (GUILayout.Button(addon.showDetails ? "-" : "+", HighLogic.Skin.button, GUILayout.Width(28f)))
                    {
                        addon.showDetails = !addon.showDetails;
                    }
                }
                GUILayout.EndHorizontal();
                if (addon.State == Dependency.States.IncorrectInstall && addon.showDetails)
                {
                    GUILayout.BeginVertical(HighLogic.Skin.box);
                    GUILayout.Label("Incorrect path(s):", green_label);
                    GUILayout.Label(addon.GetWrongPaths(), new GUIStyle(white_label) { alignment = TextAnchor.MiddleCenter });
                    GUILayout.Label("Expected path:", green_label);
                    GUILayout.Label(addon.GetExpectedPath(), new GUIStyle(white_label) { alignment = TextAnchor.MiddleCenter });
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndVertical();
            #endregion
            GUILayout.Label($"{Logger.modName} requires these addons in order to work properly.", new GUIStyle(white_label) { fontStyle = FontStyle.Bold });
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("OK", HighLogic.Skin.button))
            {
                dp_window = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void ShowTooltip(int id)
        {
            GUILayout.Label(tooltip, green_label, GUILayout.Width(tooltipSize.x));
        }
    }

    class Dependency
    {
        public Dependency(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
        public string Url { get; private set; } = null;
        public bool showDetails = false;
        public States State { get; private set; } = States.Missing;
        private IEnumerable<string> wrongPaths;
        private string expectedPath;
        public enum States
        {
            Missing,
            IncorrectInstall
        };

        public void SetPaths(IEnumerable<string> wp, string ep)
        {
            this.State = States.IncorrectInstall;
            this.wrongPaths = wp;
            this.expectedPath = ep;
        }

        public void SetUrl(string url)
        {
            if (url != "") this.Url = url;
        }

        public string GetWrongPaths()
        {
            return string.Join(", ", this.wrongPaths);
        }

        public string GetExpectedPath()
        {
            return this.expectedPath;
        }

        public string StateString
        {
            get
            {
                string statestr = "";
                switch (this.State)
                {
                    case States.Missing:
                        statestr = "Missing";
                        break;
                    case States.IncorrectInstall:
                        statestr = "Incorrectly Installed";
                        break;
                }
                return statestr;
            }
        }
    }
}
