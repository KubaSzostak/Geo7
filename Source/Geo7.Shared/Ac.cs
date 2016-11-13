using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Windows.Forms.Integration;
using Geo7;


#if AutoCAD
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.EditorInput;

using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
#endif

#if BricsCAD
using Teigha.Runtime;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Teigha.Colors;
using Bricscad.ApplicationServices;
using Bricscad.Windows;
using Bricscad.EditorInput;

using AcApp = Bricscad.ApplicationServices.Application;
using System.Xml.Linq;
#endif


namespace System
{
    public class Ac
    {

        // WinForms bindings creates common CurrencyManager, so create new array for every SelectedItem property CurrencyManager
        public static string[] PrecisionList
        {
            get { return new string[] { "0", "0.0", "0.00", "0.000", "0.0000" }; }
        }
        public static string LinearPrecisionFormat { get { return XYCoordinate.DefaultPrecision; } }
        public static string[] IdAttributeTags = new string[] { "Id", "Name", "Number", "Nazwa", "Numer", "N", "Nr", "No" };
        public static string[] HeightAttributeTags = new string[] { "H", "Height", "Elevation", "Wys" };
        public static string[] CodeAttributeTags = new string[] { "Code", "Kod" };
              


        public static Document Doc
        {
            get
            {
                if (AcApp.DocumentManager == null)
                    return null;
                return AcApp.DocumentManager.MdiActiveDocument;
            }
        }

        public static Editor Editor
        {
            get
            {
                if (Doc == null)
                    return null;
                return Doc.Editor;
            }
        }

        public static Database Db
        {
            get
            {
                return HostApplicationServices.WorkingDatabase;
            }
        }

        public static string Version
        {
            get { return AcApp.Version; }
        }

        public static dynamic AcadApplication
        {
            // Assembly Interop.BricscadApp
            // using System.Runtime.InteropServices;
            get { return AcApp.AcadApplication; }
        }

        public static UserConfigurationManager UserConfigurationManager
        {
            get { return AcApp.UserConfigurationManager;  }
        }

        public static int LocaleId
        {
            get {
                //AutoCAD: SystemObjects.DynamicLinker.ProductLcid
                return AcadApplication.LocaleId;
            }
        }

        public static string FullName
        {
            get { return AcadApplication.FullName; }
        }

        public static string Path
        {
            get { return AcadApplication.Path; }
        }

        
        private static Assembly ExecAssembly = Assembly.GetExecutingAssembly();
        private static string ExtAssemblyLocation = ExecAssembly.Location;
        public static string AssemblyPath = IO.Path.GetDirectoryName(ExtAssemblyLocation);


        private static Version mAssemblyVersion = null;
        public static Version AssemblyVersion
        {
            get
            {
                if (mAssemblyVersion == null)
                {
                    mAssemblyVersion = ExecAssembly.Version(); 
                }
                return mAssemblyVersion;
            }
        }

        private static DateTime? mAssemblyDate;
        public static DateTime AssemblyDate
        {
            get
            {
                if (!mAssemblyDate.HasValue)
                {
                    mAssemblyDate = ExecAssembly.VersionDate();
                }
                return mAssemblyDate.Value;
            }
        }

        public static bool IsPolish
        {
            get { return true;}
        }

        public static bool IsModelSpaceActive
        {
            get
            {
                var modelSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(Db);
                return modelSpaceId == Db.CurrentSpaceId;
                //WriteLn(LayoutManager.Current.CurrentLayout);
            }
        }

        public static Window MainWindow
        {
            get { return AcApp.MainWindow; }
        }



        public static string ToString(double value)
        {
            var prec = GetSystemVariable("LUPREC");
            var fmt = "0." + new string('0', Convert.ToInt32(prec));

            return value.ToString(fmt);
        }

        public static object GetSystemVariable(string name)
        {
            return AcApp.GetSystemVariable(name);
        }
        public static void SetSystemVariable(string name, object value)
        {
            AcApp.SetSystemVariable(name, value);
        }


        public static AcTransaction StartTransaction()
        {
            return Db.StartAcTransaction();
        }

        public static EditorUserInteraction StartUserInteraction(Windows.Forms.Control modalForm)
        {
            return Ac.Editor.StartUserInteraction(modalForm);
        }

        public static EditorUserInteractionWrapper StartUserInteraction(System.Windows.Window wnd)
        {
            return new EditorUserInteractionWrapper(wnd);
        }

        public static void UpdateScreen()
        {
            AcApp.UpdateScreen();
        }

        private static string GetCUIMenuGroupName(string cuiFile)
        {
            if (!File.Exists(cuiFile))
                throw new FileNotFoundException("CUI file not found: " + cuiFile);
            
            try
            {
                var cuiXml = XDocument.Load(cuiFile);
                return cuiXml.Root.Element("MenuGroup").Attribute("Name").Value;
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid CUI file format", ex);
            }
        }

        public static string LoadCUI(string cuiFile)
        {
            AppServices.Log.Add("Loading CUI: " + cuiFile);
            var cuiMenuGroupName = GetCUIMenuGroupName(cuiFile);

            UnloadCUI(cuiMenuGroupName); // First unload any existing MenuGroup
            try
            {
                var menuGroups = Ac.AcadApplication.MenuGroups;
                menuGroups.Load(cuiFile);
            }
            catch (Exception ex)
            {
                throw new Exception("Loading CUI file failed", ex);
            }

            return cuiMenuGroupName;
        }

        public static void UnloadCUI(string menuGroupName)
        {
            try
            {
                menuGroupName = menuGroupName.ToLower();
                var menuGroups = Ac.AcadApplication.MenuGroups;

                for (int i = 0; i < menuGroups.Count; i++)
                {
                    var menuGroup = menuGroups.Item(i);
                    if (menuGroup.Name.ToLower() == menuGroupName)
                    {
                        menuGroup.Unload();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unloading CUI file failed. MenuGroup: " + menuGroupName, ex);
            }
        }

        public static void ZoomTo(Point3d point)
        {
            if (!IsModelSpaceActive)
                return;

            using (var trans = StartTransaction())
            using (var currView = Editor.GetCurrentView())
            {
                currView.CenterPoint = point.Convert2d();
                Editor.SetCurrentView(currView);
                trans.Commit();
            }            
        }

        public static void ZoomTo (Point3d min, Point3d max)
        {
            // http://help.autodesk.com/view/ACD/2015/ENU/?guid=GUID-FAC1A5EB-2D9E-497B-8FD9-E11D2FF87B93

            if (!IsModelSpaceActive)
                return;

            using (var trans = StartTransaction())
            using (var currView = Editor.GetCurrentView())
            {
                currView.CenterPoint = new Point2d((min.X + max.X) / 2.0, (min.Y + max.Y) / 2.0);
                currView.Height = (max.Y - min.Y) * 1.5;
                currView.Width = (max.X - min.X) * 1.5;
                Editor.SetCurrentView(currView);
                trans.Commit();
            }
        }

        public static void ZoomEx()
        {
            Doc.SendStringToExecute("_.ZOOM _E ", false, false, false);
        }

        public static void ExecuteCommand(string command)
        {
            Doc.SendStringToExecute(command, false, false, false);
        }

        public static void WriteLn(string s, params object[] args)
        {
            Write("\r\n" + s, args);
        }

        public static void Write(string s, params object[] args)
        {
            if (Editor == null)
                return;
            Editor.WriteMessage(string.Format(s, args));
            //DoEvents();           
            Editor.WriteMessage(""); 
        }

        internal static void WriteError(System.Exception ex, string methodName, string commandName)
        {
            Ac.WriteLn("");
            Ac.WriteLn("G7 Error: ");
            
            AppServices.Log.AddLn();
            if (!string.IsNullOrEmpty(commandName))
            {
                AppServices.Log.Add("CommanddName: " + commandName);
                Ac.WriteLn("CommanddName: " + commandName);
            }
            if (!string.IsNullOrEmpty(methodName))
            {
                AppServices.Log.Add("MethodName: " + methodName);
                Ac.WriteLn("MethodName: " + methodName);
            }

            Ac.WriteLn(ex.GetAllMessages());
            AppServices.Log.Add(ex);
        }

        internal static void WriteError(string errMsg, params string[] args)
        {
            errMsg = string.Format(errMsg, args);
            WriteError(new System.Exception(errMsg), null, null);
        }

        public static void ShowAlertDialog(string str)
        {
            AcApp.ShowAlertDialog(str);
        }

        public static void ShowMsg(string s, params object[] args)
        {
            s = string.Format(s, args);
            AcApp.ShowAlertDialog(s);            
        }

        public static void ShowErr(System.Exception ex)
        {
            AppServices.Log.Add(ex);
            AcApp.ShowAlertDialog(ex.GetExtendedMessage(null, null, false));

        }

        public static System.Windows.Forms.DialogResult ShowModalDialog(System.Windows.Forms.Form form)
        {
            return AcApp.ShowModalDialog(form);
        }
        public static void ShowModelessDialog(System.Windows.Forms.Form formToShow)
        {
            AcApp.ShowModelessDialog(formToShow);
        }

        public static System.Windows.Forms.DialogResult ShowModalForm(System.Windows.Forms.Form form)
        {
            form.FormBorderStyle = Windows.Forms.FormBorderStyle.FixedDialog;
            form.StartPosition = Windows.Forms.FormStartPosition.CenterParent;
            form.ShowInTaskbar = false;
            form.ShowIcon = false;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
            form.ControlBox = true;

            return AcApp.ShowModalDialog(MainWindow, form, false);
        }

        public static void ShowModelesForm(System.Windows.Forms.Form form)
        {
            form.ShowInTaskbar = false;
            form.StartPosition = Windows.Forms.FormStartPosition.CenterParent;
            AcApp.ShowModelessDialog(form);
        }

        public static bool ShowModal(System.Windows.Window wnd)
        {
            wnd.ShowInTaskbar = false;
            wnd.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            wnd.Topmost = true;
            wnd.ShowInTaskbar = false;

#if AutoCAD
            AcApp.ShowModalWindow(null, wnd, false);
#else
            return wnd.ShowDialog().GetValueOrDefault(false);
#endif
        }

        public static void ShowModeles(System.Windows.Window wnd)
        {
            wnd.ShowInTaskbar = false;
            wnd.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
#if AutoCAD
            AcApp.ShowModelessWindow(null,wnd, false);
#endif

#if BricsCAD
            throw new NotSupportedException();
#endif
        }

        private static int progressPos = 0;
        private static int progressPercent = 0;
        private static int progressSteps = 0;
        public static void InitProgress(string msg, int stepsCount)
        {
            progressPos = 0;
            progressPercent = 0;
            progressSteps = stepsCount;
#if AutoCAD
            Autodesk.AutoCAD.Internal.Utils.SetApplicationStatusBarProgressMeter(msg, 0, 100);
#else
            if (progressSteps > 99)
            {
                Ac.WriteLn(msg + "   0");
            }
#endif
        }

        public static void SetProgress(int pos)
        {
            progressPos = pos;
            int newPercent = (int)( (double)pos / (double)progressSteps * 100.0);
            if (newPercent > progressPercent)
            {
                progressPercent = newPercent;
#if AutoCAD
                Autodesk.AutoCAD.Internal.Utils.SetApplicationStatusBarProgressMeter(newPercent);
#else
                if (progressSteps > 99) {
                    if (newPercent % 10 == 0)
                    {
                        Ac.Write(newPercent.ToString());
                        Ac.Editor.UpdateScreen();
                    }
                    else if (newPercent % 2 == 0)
                    {
                        Ac.Write(".");
                    }
                }
                //Editor.UpdateScreen();
#endif
                //DoEvents();
            }
        }

        public static void ProgressStep()
        {
            SetProgress(progressPos + 1);
        }

        public static void ClearProgress()
        {
            try
            {
#if AutoCAD
                Autodesk.AutoCAD.Internal.Utils.RestoreApplicationStatusBar();
#else
                Ac.WriteLn("");
#endif
                DoEvents();
            }
            catch (System.Exception ex)
            {
                Ac.WriteError(ex, "Ac.ClearProgress()", null);
            }
        }

        private static void DoEvents()
        {
            //Db.TransactionManager.QueueForGraphicsFlush();

            Editor.UpdateScreen();
            AcApp.UpdateScreen();
            //System.Windows.Forms.Application.DoEvents();
        }

        public static string GetLastFileName(string filter)
        {
            var res = Ac.GetValue("FileDialog." + filter);
            if (string.IsNullOrEmpty(res))
            {                    
                res = System.IO.Path.ChangeExtension(Ac.Db.Filename, "");
            }
            return res;
        }

        public static void SetLastFileName(string filter, string value)
        {
            Ac.SetValue("FileDialog." + filter, value);
        }

        private static void InitFileDlgOptions(PromptFileOptions opt, string filter)
        {
            var filePath = GetLastFileName(filter);
            opt.InitialDirectory = IO.Path.GetDirectoryName(filePath);
            opt.InitialFileName = IO.Path.GetFileNameWithoutExtension(filePath);
            opt.Filter = filter;
        }

        public static string FileDlgCoordFilter = SysUtils.Strings.TxtFiles + "|*.txt" + "|" + SysUtils.Strings.AllFiles + "|*.*";

        public static System.Windows.Forms.OpenFileDialog ShowOpenFileDialog(string filter)
        {
            var filePath = GetLastFileName(filter);
            var dlg = new System.Windows.Forms.OpenFileDialog();

            dlg.InitialDirectory = IO.Path.GetDirectoryName(filePath);
            dlg.FileName = IO.Path.GetFileName(filePath);
            dlg.Filter = filter;
            if (dlg.ShowDialog() == Windows.Forms.DialogResult.OK)
                return dlg;
            else
                return null;
        }

        public static string ShowOpenFileDlg(string filter)
        {
            var opt = new PromptOpenFileOptions(" ");
            InitFileDlgOptions(opt, filter);
            var res = Ac.Editor.GetFileNameForOpen(opt);
            if (res.Status != PromptStatus.OK)
                return null;

            SetLastFileName(filter, res.StringResult);
            return res.StringResult;
        }

        public static string ShowSaveFileDlg(string filter)
        {
            var opt = new PromptSaveFileOptions(" ");
            InitFileDlgOptions(opt, filter);
            var res = Ac.Editor.GetFileNameForSave(opt);
            if (res.Status != PromptStatus.OK)
                return null;

            SetLastFileName(filter, res.StringResult);
            return res.StringResult;
        }


        public static SelectionFilter GetSelectBlocksFilter(params string[] blockNames)
        {
            List<string> blockList = new List<string>(blockNames);
            TypedValue[] values = new TypedValue[]
            {                
                GetTypedValue(DxfCode.Start, "INSERT" ),
                GetTypedValue(DxfCode.BlockName, blockList.Join(",") )
            };
            return new SelectionFilter(values);
        }

        public static ObjectId[] SelectBlocks(params string[]  blockNames)
        {
            //PromptEntityOptions peo = new PromptEntityOptions("Select blocks: ");
            //var res = Editor.GetEntity(peo);

            var filter = GetSelectBlocksFilter(blockNames);
            PromptSelectionOptions promptOpt = new PromptSelectionOptions();
            promptOpt.MessageForAdding = "Select blocks:";

            PromptSelectionResult selRes = Ac.Editor.GetSelection(promptOpt, filter);
            if (selRes.Status == PromptStatus.OK)
                return selRes.Value.GetObjectIds();
            return new ObjectId[0];
        }

        public static ObjectId[] SelectAllBlocks(string blockName)
        {
            var filter = GetSelectBlocksFilter(blockName);
            var selRes = Ac.Editor.SelectAll(filter);
            if (selRes.Status == PromptStatus.OK)
                return selRes.Value.GetObjectIds();
            return new ObjectId[0];
        }

        public static string GetValidName(string name)
        {
            string res = name;
            res = res.Replace("=", "");
            res = res.Replace("<", "");
            res = res.Replace(">", "");
            res = res.Replace("|", "");
            res = res.Replace("/", ".");
            res = res.Replace(@"\", ".");
            res = res.Replace(@"""", "");
            res = res.Replace("'", "");
            res = res.Replace("`", "");
            res = res.Replace(";", "");
            res = res.Replace(":", "");
            res = res.Replace(",", "");
            res = res.Replace("*", "");
            res = res.Replace("?", "");
            res = res.Trim();
            
            if (string.IsNullOrEmpty(res))
                res = "[empty]";
            return res.Trim();
        }

        


        public static void SetValue(string name, string value)
        {
            using (var docLoc = Ac.Doc.LockDocument())
            using (var trans = Ac.StartTransaction())
            {
                trans.SetValue(name, value);
                trans.Commit();
            }
        }


        /// <summary>
        /// If there is no value for 'name' empty string ("") is returned.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetValue(string name)
        {
            using (var docLoc = Ac.Doc.LockDocument())
            using (var trans = Ac.StartTransaction())
            {
                return trans.GetValue(name);
            }
        }

        public static TypedValue GetTypedValue(DxfCode code, object value)
        {
            return new TypedValue((int)code, value);
        }

        public static SortedSet<string> GetBlockNames(bool onlyWithReferences, bool onlyWithAttributes)
        {
            var res = new SortedSet<string>();

            using (var trans = Ac.StartTransaction())
            {
                var dbBlocks = trans.BlockDefs.AsEnumerable();
                if (onlyWithReferences)
                    dbBlocks = dbBlocks.Where(b => b.HasReferences);
                if (onlyWithAttributes)
                    dbBlocks = dbBlocks.Where(b => b.HasAttributes);

                res.AddItems(dbBlocks.Select(b => b.Name));
            }
            return res;            
        }
    }


    public class EditorUserInteractionWrapper : DisposableObject
    {
        public EditorUserInteractionWrapper(System.Windows.Window wnd)
        {
            window = wnd;
            window.Topmost = true;
            window.Hide();
        }

        public EditorUserInteractionWrapper(EditorUserInteraction userInteraction)
        {
            this.interaction = userInteraction;
        }

        private EditorUserInteraction interaction = null;
        private System.Windows.Window window = null;

        protected override void OnDisposing()
        {
            if (interaction != null)
                interaction.Dispose();

            if (window != null)
                window.Show();
        }

        public void End()
        {
            if (interaction != null)
                interaction.End();

            if (window != null)
                window.Show();
        }
    }
}
