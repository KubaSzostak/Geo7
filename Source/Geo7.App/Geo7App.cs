using Geo7.Tools;
using System.Threading;
using System;
using System.Globalization;
using System.Linq;

#if AutoCAD
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.EditorInput;
#endif

#if BricsCAD
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Colors;
using Bricscad.ApplicationServices;
using Bricscad.Windows;
using Bricscad.EditorInput;
#endif



[assembly: ExtensionApplication(typeof(Geo7.Geo7App))]
[assembly: CommandClass(typeof(Geo7.Geo7App))]

namespace Geo7
{
    partial class Geo7App : IExtensionApplication
    {
        public const string AppName = "Geo7";
        public const string WebSite = "https://github.com/kubaszostak/geo7";
        private static PaletteSet ToolsPalettes = null;
        private string cuiMenuGroupName = "Geo7";

        public void Initialize()
        {
            try
            {
                StartupCheck();
                AppServices.Log.Add("Initialize() ...");
                InternalInit();
                ShowRibbon();
                cuiMenuGroupName = Ac.LoadCUI(Geo7App.Geo7CUI);
                LoadNonStaticCUI();
                AppServices.Log.Add("Initialize(): Done.");
                Ac.WriteLn(AppName + " loaded.");
            }
            catch (System.Exception ex)
            {
                Ac.WriteError(ex, this.GetType().Name + ".Initialize()", null);
            }
            AppServices.Log.Add("Initialize(): Done.");
        }


        public void Terminate()
        {
            Ac.UnloadCUI(cuiMenuGroupName);
            AppServices.Log.Add("Terminate(): Application closed.");
        }

        public static string Geo7Dwg
        {
            get
            {
                if (Ac.IsPolish)
                    return Ac.AssemblyPath + "\\Dwg\\Geo7.dwg";
                else
                    return Ac.AssemblyPath + "\\Dwg\\Geo7.dwg";
            }
        }
        public static string Geo7CUI
        {
            get
            {
                if (Ac.IsPolish)
                    return Ac.AssemblyPath + "\\cui\\Geo7.cui";
                else
                    return Ac.AssemblyPath + "\\cui\\Geo7.cui";
            }
        }


        private void LoadNonStaticCUI()
		{
			//TODO: Non static CUI items (insert point)
			// http://forums.autodesk.com/t5/net/accessing-toolbars-and-buttons-programatically/td-p/3182594
		}

		partial void InternalInit();
        partial void ShowRibbon();

        public void StartupCheck()
        {
            AppServices.Log.NewSection();
            AppServices.Log.Add("Starting application: {0}, Version {1}...", Geo7App.AppName, Ac.AssemblyVersion.ToString());
            AppServices.Log.Add("xCAD Version: " + Ac.Version);
            AppServices.Log.Add("Geo7 binary path: " + Ac.AssemblyPath);
            AppServices.Log.AddLn();

            if (!System.IO.File.Exists(Geo7App.Geo7Dwg))
            {
                Ac.WriteError(SysUtils.Strings.FileNotFound, Geo7App.Geo7Dwg);
            }

        }

        private static string[] mDefPointBlockNames = null;
        public static string[] DefPointBlockNames
        {
            get
            {
                if (mDefPointBlockNames == null)
                    mDefPointBlockNames = AcDwgDb.GetBlockNames(Geo7App.Geo7Dwg, true).OrderBy(n => n).ToArray();

                return mDefPointBlockNames;
            }
        }

        private void AddPalettes()
        {
            try
            {
                AppServices.Log.Add("AddPalettes()...");
                if (ToolsPalettes == null)
                {
                    // use constructor with Guid so that we can save/load user data
                    ToolsPalettes = new PaletteSet(AppName);
                    ToolsPalettes.MinimumSize = new System.Drawing.Size(300, 300);
                    //ToolsPalettes.Size = new System.Drawing.Size(300, 500);
                    //ToolsPalettes.Style = PaletteSetStyles.ShowTabForSingle | PaletteSetStyles.ShowAutoHideButton | PaletteSetStyles.ShowCloseButton;
                    ToolsPalettes.Visible = true;
                    //ToolsPalettes.KeepFocus = true;
                    //ToolsPalettes.Add("Tools", new ToolsPallete());
                    //AddPalette(new ToolPalettes.BlocksPalette());
                    //AddPalette(new ToolPalettes.PolygonsPalette());
                    //AddPalette(new ToolPalettes.InfoPalette()); // Tą zawsze wczytaj, żeby można było wczytać licencje
                }
                AppServices.Log.Add("AddPalettes():Done.");
            }
            catch (System.Exception ex)
            {
                Ac.WriteLn("Geo7: Error showing Tools Palette");
                Ac.WriteError(ex, this.GetType().Name + ".AddPalettes()", null);
            }            
        }
		

        [CommandMethod("G7Log")]
        public void ShowLogFile()
		{
			Ac.WriteLn("Opening " + AppServices.Log.LogFilePath + "...");
			System.Diagnostics.Process.Start(AppServices.Log.LogFilePath);
		}
		

        [CommandMethod("G7")]
        public void PrintInfo()
        {
            Ac.WriteLn(Geo7App.AppName);
            Ac.WriteLn("Version: " + Ac.AssemblyVersion.ToString());
            Ac.WriteLn("Geo7 binary path: " + Ac.AssemblyPath);
            Ac.WriteLn("Geo7 log file:    " + AppServices.Log.LogFilePath);

            ShowRibbon();
        }

		[CommandMethod("G7Info")]
		public void InfoDialog()
		{
			string s = Geo7App.AppName;
			s += "\r\nVersion: " + Ac.AssemblyVersion;
			s += "\r\n\r\n" + Geo7App.WebSite;

			Ac.ShowMsg(s);
		}

		[CommandMethod("G7MakeSuggestion")]
		public void MakeSuggestion()
		{
			System.Diagnostics.Process.Start("http://geo7.userecho.com/");
		}



		[CommandMethod("G7CreatePolygons")]
        public void CreatePolygonsCommand()
        {
            DocumentLock docLock = Ac.Doc.LockDocument();
            AcPolygonsCreator.Run();
            docLock.Dispose();
        }

        [CommandMethod("G7TextPolygons")]
        public void JoinPolygonsWithTextCommand()
        {
            new AcJoinPolygonsWithTexts().Run();
        }

        [CommandMethod("G7ConvertDgnTexts")]
        public void ConvertDgnTexts()
        {
            new ConvertDgnTextsCommand().Execute();
        }

        [CommandMethod("G7LANG")]
        public static void WhichLanguage()
        {
            var cId = SystemObjects.DynamicLinker.ProductLcid;
            var cId2 = Ac.LocaleId;
            var cult = new CultureInfo(cId2);
            var ed = Ac.Editor;
            var ver = Ac.Version;
            Ac.WriteLn("\nLanguage of your AutoCAD product is {0}.",cult.EnglishName);
            Ac.WriteLn(Thread.CurrentThread.CurrentCulture.EnglishName);
            Ac.WriteLn(Thread.CurrentThread.CurrentUICulture.EnglishName);
        }

        [CommandMethod("G7ImportPoints" )]
        public static void BlockImport()
        {
            new BlockImpTextCommand().Execute();
        }

        [CommandMethod("G7ExportPoints")]
        public static void BlockExport()
        {
            new BlockExpTextCommand().Execute();
        }

        [CommandMethod("G7ExportTexts")]
        public static void ExportTexts()
        {
            new TextExpTextCommand().Execute();
        }

        [CommandMethod("G7PointSettings")]
        public static void BlockSettings()
        {
            new BlockSettingsCommand().Execute();
		}

		[CommandMethod("G7InsertPoint")]
		public static void InsertPoint()
		{
			new BlockInsertCommand(null).Execute();
		}

		[CommandMethod("G7Slope", CommandFlags.Modal)]
		public static void Slope()
		{
			new SlopeCommand().Execute();
		}

		[CommandMethod("G7OrthoDist", CommandFlags.Modal)]
		public static void OrthoDist()
		{
			new OrthoDistCommand().Execute();
        }

        [CommandMethod("G7PtsLnDist", CommandFlags.Modal)]
        public static void PtLnDist()
        {
            new PtsLnDistCommand().Execute();
        }

        [CommandMethod("G7BugTest", CommandFlags.Modal)]
		public static void BugTest()
		{
		}


	}
}
