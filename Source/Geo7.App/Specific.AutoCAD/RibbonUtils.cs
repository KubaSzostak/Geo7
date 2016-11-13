using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Windows;
using Autodesk.AutoCAD.Runtime;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using Autodesk.AutoCAD.ApplicationServices;
using System.Windows.Input;
using System.Windows.Controls;
using Geo7.Tools;
using Geo7;
using Geo7.Properties;


[assembly: CommandClass(typeof(RibbonUtils))]

namespace Geo7
{


    public class RibbonUtils
    {
        public RibbonCombo pan1ribcombo1 = new RibbonCombo();
        public RibbonCombo pan3ribcombo = new RibbonCombo();

        public static void Init()
        {
            // Załaduj Ribbon przy starcie
            acadApp.Idle += new EventHandler(App_Idle);

            // Oraz przy każdej zmianie Workspace
            Application.SystemVariableChanged += new SystemVariableChangedEventHandler(Application_SystemVariableChanged);
        }

        static void Application_SystemVariableChanged(object sender, SystemVariableChangedEventArgs e)
        {
            if (e.Name.ToLower() == "wscurrent")
                try
                {
                    InitGeo7Ribbon();
                }
                catch (System.Exception ex)
                {
                    Ac.WriteError(ex, "RibbonUtils.Application_SystemVariableChanged()", null);
                }
        }

        static void App_Idle(object sender, EventArgs e)
        {
            // Trzeba poczekać aż Ribbon zostanie załadowany. Może się zdarzyć
            // że aplikacja będzie wczytana wcześniej niż Ribbon.
            if (ComponentManager.Ribbon == null)
                return;

            acadApp.Idle -= new EventHandler(App_Idle);
            Ac.Write("Loading Geo7 ribbon tab... ");
            AppLog.Add("Loading Geo7 ribbon tab... ");

            try
            {
                InitGeo7Ribbon();
                Ac.WriteLn("Loaded.");
            }
            catch (System.Exception ex)
            {
                Ac.WriteError(ex, "RibbonUtils.App_Idle()", null);
            }
            AppLog.Add("Geo7 ribbon loaded.");
        }


        public RibbonUtils()
        {
            pan3ribcombo.CurrentChanged += new EventHandler<RibbonPropertyChangedEventArgs>(pan3ribcombo_CurrentChanged);
        }

        public static void InitGeo7Ribbon()
        {
            AppLog.Add("InitGeo7Ribbon()...");
            if (Geo7ToolsTab == null)
            {
                Geo7ToolsTab = new RibbonTab();
                Geo7ToolsTab.Title = Geo7ToolsApp.AppName;
                Geo7ToolsTab.Id = Geo7ToolsApp.AppName.Replace(" ", "_");

                if (Ac.Licence.IsValid || Ac.RunWithoutLicense)
                {
                    AppLog.Add("InitGeo7Ribbon():AddBlocksPanel()...");
                    AddBlocksPanel();
                    AppLog.Add("InitGeo7Ribbon():AddDrawPanel()...");
                    AddDrawPanel();
                    AppLog.Add("InitGeo7Ribbon():AddToolsPanel()...");
                    AddToolsPanel();
                }
                AppLog.Add("InitGeo7Ribbon():AddConfigurationPanel()...");
                AddConfigurationPanel();
                AppLog.Add("InitGeo7Ribbon():Done.");
            }

            Autodesk.Windows.RibbonControl ribbon = ComponentManager.Ribbon;
            if (ribbon == null)
            {
                AppLog.Add(AcConsts.RibbonNotFound);
                Ac.WriteLn(AcConsts.RibbonNotFound);
                return;
            }

            if (ribbon.FindTab(Geo7ToolsTab.Id) == null)
            {
                AppLog.Add("InitGeo7Ribbon():ribbon.Tabs.Add(Geo7ToolsTab)...");
                ribbon.Tabs.Add(Geo7ToolsTab);
            }
            AppLog.Add("InitGeo7Ribbon():Done.");
        }

        public static void ShowGeo7Ribbon()
        {
            try
            {
                AppLog.Add("ShowGeo7Ribbon()...");
                //Variable MENUBAR=1
                Ac.ExecuteCommand("_ribbon "); // Show Ribbon
                InitGeo7Ribbon();
                ComponentManager.Ribbon.ActiveTab = Geo7ToolsTab;
                AppLog.Add("ShowGeo7Ribbon():Done.");
            }
            catch (System.Exception ex)
            {
                Ac.WriteError(ex, "RibbonUtils.ShowGeo7Ribbon()", null);
            }
        }

        private void pan3ribcombo_CurrentChanged(object sender, RibbonPropertyChangedEventArgs e)
        {
            RibbonButton but = pan3ribcombo.Current as RibbonButton;
            acadApp.ShowAlertDialog(but.Text);
        }

        [CommandMethod("r1")]
        public void RibbonSplitButtontest()
        {
            RibbonButton commandlinebutton = new RibbonButton();
            commandlinebutton.Text = "Newly Added Button with command: MyRibbonTestCombo";
            commandlinebutton.ShowText = true;
            commandlinebutton.ShowImage = true;
            commandlinebutton.Image = Images.getBitmap(Resources.Small);
            commandlinebutton.LargeImage = Images.getBitmap(Resources.Find);
            commandlinebutton.CommandHandler = new RibbonCommandHandler();
            pan1ribcombo1.Items.Add(commandlinebutton);
            pan1ribcombo1.Current = commandlinebutton;
        }

        public static RibbonTab Geo7ToolsTab;

        private static void AddBlocksPanel()
        {

            var blocksPnl = Geo7ToolsTab.AddPanel(AcConsts.Blocks);

            // Insert Point
            var blockAddButton = new RibbonSplitButton();
            blockAddButton.ShowText = false;
            blockAddButton.Size = RibbonItemSize.Large;
            blockAddButton.Text = "Insert Point"; //Required not to crash AutoCAD when using cmd locators

            var insertPointCommands = DwgLib.GetInsertPointCommands(Ac.Geo7Dwg);
            foreach (var cmd in insertPointCommands)
            {
                blockAddButton.Items.AddButton(cmd);
            }


            blocksPnl.Items.Add(blockAddButton);

            // Import / Export
            var blocksInOutRow = new RibbonRowPanel();
            blocksInOutRow.Items.AddButton(new ImportBlocksCommand());
            blocksInOutRow.Items.AddRowBreak();
            blocksInOutRow.Items.AddButton(new ExportBlocksCommand());
            blocksInOutRow.Items.AddRowBreak();
            blocksInOutRow.Items.AddButton(new ExportTextsCommand());

            blocksPnl.Items.Add(blocksInOutRow);

            // Block Settings
            var blocksSettingsRow = new RibbonRowPanel();
            blocksSettingsRow.Items.AddButton(new BlockSettingsCommand());
            blocksSettingsRow.Items.AddRowBreak();
            blocksSettingsRow.Items.AddButton(new FindBlockCommand());
            blocksSettingsRow.Items.AddRowBreak();

            blocksPnl.Items.Add(blocksSettingsRow);

            //var blocksFindRow = new RibbonRowPanel();
            //blocksFindRow.Items.Add(new RibbonTextBox());
            //blocksFindRow.Items.Add(new RibbonButton());
            //blocksFindRow.Items.Add(new RibbonButton());
            //blocksFindRow.Items.AddRowBreak();

            //var findResults = new RibbonGallery();
            //findResults.DisplayMode = GalleryDisplayMode.Window;
            //findResults.Size = RibbonItemSize.Standard;
            //findResults.ShowImage = false;


            //var b1 = new RibbonButton();
            //b1.Size = RibbonItemSize.Standard;
            //b1.ShowImage = false;
            //b1.Text = "aaaa";
            //b1.Orientation = Orientation.Horizontal;

            //var b2 = new RibbonButton();
            //b2.Size = RibbonItemSize.Standard;
            //b2.Text = "2222";
            //b1.ShowImage = false;
            //b2.Orientation = Orientation.Horizontal;


            //findResults.MenuItems.Add(b1);
            //findResults.MenuItems.Add(b2);
            //findResults.MenuItems.Add(b1);
            //findResults.MenuItems.Add(b2);
            //findResults.Items.Add(b1);

            //blocksFindRow.Items.AddButton("Block Settings", null, null);
        }

        private static void AddDrawPanel()
        {
            // Draw
            var drawPnl = Geo7ToolsTab.AddPanel("Rysuj");

            drawPnl.Items.AddButton( new SlopeCommand());
            drawPnl.Items.AddRowBreak();
            drawPnl.Items.AddButton(new OrtoCommand());
            drawPnl.Items.AddRowBreak();
            drawPnl.Items.Add(new RibbonSeparator());
        }

        private static void AddToolsPanel()
        {
            // Tools
            var toolsPnl = Geo7ToolsTab.AddPanel(AcConsts.Tools);
            var toolsRow = new RibbonRowPanel();
            toolsPnl.Items.Add(toolsRow);

            toolsRow.Items.AddItem(new RibbonButton(), new ConvertDgnTextsCommand());
            toolsRow.Items.AddRowBreak();
#if AC2010
            toolsRow.Items.AddItem(new RibbonButton(), new RegisterCommand());
            toolsRow.Items.AddRowBreak();
            toolsRow.Items.AddItem(new RibbonButton(), new UnregisterCommand());
#endif
        }

        private static void AddConfigurationPanel()
        {
            // Configuration
            var confPnl = Geo7ToolsTab.AddPanel(AcConsts.Configuration);

            confPnl.Items.AddButton(new InfoCommand());
            confPnl.Items.AddRowBreak();
            //confPnl.Items.AddButton(new LicenseCommand());
            //confPnl.Items.AddRowBreak();
            confPnl.Items.AddButton(new MakeSuggestionCommand());
        }

        private void AddSamplePanels()
        {

            // create Ribbon panels
            Autodesk.Windows.RibbonPanelSource panel1Panel = new RibbonPanelSource();
            panel1Panel.Title = "Panel1";
            RibbonPanel Panel1 = new RibbonPanel();
            Panel1.Source = panel1Panel;
            Geo7ToolsTab.Panels.Add(Panel1);

            RibbonButton pan1button1 = new RibbonButton();
            pan1button1.Text = "Button1";
            pan1button1.ShowText = true;
            pan1button1.ShowImage = true;
            pan1button1.Image = Images.getBitmap(Resources.Small);
            pan1button1.LargeImage = Images.getBitmap(Resources.Find);
            pan1button1.Orientation = System.Windows.Controls.Orientation.Vertical;
            pan1button1.Size = RibbonItemSize.Large;
            pan1button1.CommandHandler = new RibbonCommandHandler();

            RibbonButton pan1button2 = new RibbonButton();
            pan1button2.Text = "Button2";
            pan1button2.ShowText = true;
            pan1button2.ShowImage = true;
            pan1button2.Image = Images.getBitmap(Resources.Small);
            pan1button2.LargeImage = Images.getBitmap(Resources.Find);
            pan1button2.CommandHandler = new RibbonCommandHandler();

            RibbonButton pan1button3 = new RibbonButton();
            pan1button3.Text = "Button3";
            pan1button3.ShowText = true;
            pan1button3.ShowImage = true;
            pan1button3.Image = Images.getBitmap(Resources.Small);
            pan1button3.LargeImage = Images.getBitmap(Resources.Find);
            pan1button3.CommandHandler = new RibbonCommandHandler();

            // Set te propperties for the RibbonCombo
            // Te ribboncombo control does not listen to the command handler
            pan1ribcombo1.Text = " ";
            pan1ribcombo1.ShowText = true;
            pan1ribcombo1.MinWidth = 150;

            RibbonRowPanel pan1row1 = new RibbonRowPanel();
            pan1row1.Items.Add(pan1button2);
            pan1row1.Items.Add(new RibbonRowBreak());
            pan1row1.Items.Add(pan1button3);
            pan1row1.Items.Add(new RibbonRowBreak());
            pan1row1.Items.Add(pan1ribcombo1);

            panel1Panel.Items.Add(pan1button1);
            panel1Panel.Items.Add(new RibbonSeparator());
            panel1Panel.Items.Add(pan1row1);

            RibbonPanelSource pan4Panel = new RibbonPanelSource();
            pan4Panel.Title = "Panel4";
            RibbonPanel Panel4 = new RibbonPanel();
            Panel4.Source = pan4Panel;
            Geo7ToolsTab.Panels.Add(Panel4);

            RibbonButton pan4button1 = new RibbonButton();
            pan4button1.Text = "Button1";
            pan4button1.ShowText = true;
            pan4button1.ShowImage = true;
            pan4button1.Image = Images.getBitmap(Resources.Small);
            pan4button1.LargeImage = Images.getBitmap(Resources.Find);
            pan4button1.Size = RibbonItemSize.Large;
            pan4button1.Orientation = System.Windows.Controls.Orientation.Vertical;
            pan4button1.CommandHandler = new RibbonCommandHandler();

            RibbonButton pan4button2 = new RibbonButton();
            pan4button2.Text = "Button2";
            pan4button2.ShowText = true;
            pan4button2.ShowImage = true;
            pan4button2.Image = Images.getBitmap(Resources.Small);
            pan4button2.LargeImage = Images.getBitmap(Resources.Find);
            pan4button2.CommandHandler = new RibbonCommandHandler();

            RibbonButton pan4button3 = new RibbonButton();
            pan4button3.Text = "Button3";
            pan4button3.ShowText = true;
            pan4button3.ShowImage = true;
            pan4button3.Image = Images.getBitmap(Resources.Small);
            pan4button3.LargeImage = Images.getBitmap(Resources.Find);
            pan4button3.CommandHandler = new RibbonCommandHandler();

            RibbonButton pan4button4 = new RibbonButton();
            pan4button4.Text = "Button4";
            pan4button4.ShowText = true;
            pan4button4.ShowImage = true;
            pan4button4.Image = Images.getBitmap(Resources.Small);
            pan4button4.LargeImage = Images.getBitmap(Resources.Find);
            pan4button4.Size = RibbonItemSize.Large;
            pan4button4.Orientation = System.Windows.Controls.Orientation.Vertical;
            pan4button4.CommandHandler = new RibbonCommandHandler();

            RibbonButton pan4ribcombobutton1 = new RibbonButton();
            pan4ribcombobutton1.Text = "Button1";
            pan4ribcombobutton1.ShowText = true;
            pan4ribcombobutton1.ShowImage = true;
            pan4ribcombobutton1.Image = Images.getBitmap(Resources.Small);
            pan4ribcombobutton1.LargeImage = Images.getBitmap(Resources.Find);
            pan4ribcombobutton1.CommandHandler = new RibbonCommandHandler();

            RibbonButton pan4ribcombobutton2 = new RibbonButton();
            pan4ribcombobutton2.Text = "Button2";
            pan4ribcombobutton2.ShowText = true;
            pan4ribcombobutton2.ShowImage = true;
            pan4ribcombobutton2.Image = Images.getBitmap(Resources.Small);
            pan4ribcombobutton2.LargeImage = Images.getBitmap(Resources.Find);
            pan4ribcombobutton2.CommandHandler = new RibbonCommandHandler();

            pan3ribcombo.Width = 150;
            pan3ribcombo.Items.Add(pan4ribcombobutton1);
            pan3ribcombo.Items.Add(pan4ribcombobutton2);
            pan3ribcombo.Current = pan4ribcombobutton1;

            RibbonRowPanel vvorow1 = new RibbonRowPanel();
            vvorow1.Items.Add(pan4button2);
            vvorow1.Items.Add(new RibbonRowBreak());
            vvorow1.Items.Add(pan4button3);
            vvorow1.Items.Add(new RibbonRowBreak());
            vvorow1.Items.Add(pan3ribcombo);

            pan4Panel.Items.Add(pan4button1);
            pan4Panel.Items.Add(vvorow1);
            pan4Panel.Items.Add(new RibbonSeparator());
            pan4Panel.Items.Add(pan4button4);
        }

        public class RibbonCommandHandler : System.Windows.Input.ICommand
        {
            public bool CanExecute(object parameter)
            {
                if (CanExecuteChanged != null) //Zapobiegaj wyświetlaniu ostrzeżenia:  Warning	2	The event 'GeoSoft.Geo7.RibbonUtils.RibbonCommandHandler.CanExecuteChanged' is never used	P:\Projects\NET.2011\GeoSoft.Geo7\Ribbon\RibbonUtils.cs	409	39	GeoSoft.Geo7
                    return true; 
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                Document doc = acadApp.DocumentManager.MdiActiveDocument;

                if (parameter is RibbonButton)
                {
                    RibbonButton button = parameter as RibbonButton;
                    doc.Editor.WriteMessage("\nRibbonButton Executed: " + button.Text + "\n");
                }
            }
        }

        public class Images
        {
            public static BitmapImage getBitmap(Bitmap image)
            {
                MemoryStream stream = new MemoryStream();
                image.Save(stream, ImageFormat.Png);
                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = stream;
                bmp.EndInit();


                return bmp;
            }
        }
    }

    public static class RibbonEx
    {
        public static RibbonPanelSource AddPanel(this RibbonTab tab, string title)
        {
            Autodesk.Windows.RibbonPanelSource panelSrc = new RibbonPanelSource();
            panelSrc.Title = title;
            RibbonPanel panel = new RibbonPanel();
            panel.Source = panelSrc;
            tab.Panels.Add(panel);

            return panelSrc;
        }

        public static void AddItem(this RibbonItemCollection items, RibbonCommandItem item, string text, ICommand command, Bitmap smallImage, Bitmap largeImage)
        {
            item.Text = text;
            item.ShowText = true;
            item.ShowImage = false;
            if ((smallImage != null) || (largeImage != null))
            {
                item.ShowImage = true;
                if (largeImage == null)
                    largeImage = smallImage;
                item.Image = GetImage(smallImage);
                item.LargeImage = GetImage(largeImage);
            }
            item.Size = RibbonItemSize.Standard;
            item.CommandHandler = command;

            items.Add(item);
        }

        public static void AddItem(this RibbonItemCollection items, RibbonCommandItem item, AcCommand command)
        {
            item.Description = command.Description;
            AddItem(items, item, command.DisplayName, command, command.SmallImage, command.LargeImage);
        }



        public static BitmapImage GetImage(Bitmap image)
        {
            if (image == null)
                return null;
            MemoryStream stream = new MemoryStream();
            image.Save(stream, ImageFormat.Png);
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = stream;
            bmp.EndInit();

            return bmp;
        }

        public static RibbonButton AddButton(this RibbonItemCollection items, AcCommand command)
        {
            RibbonButton btn = new RibbonButton();
            items.AddItem(btn, command);
            return btn;
        }

        public static void AddRowBreak(this RibbonItemCollection items)
        {
            items.Add(new RibbonRowBreak());
        }

        public static void AddSeparator(this RibbonItemCollection items)
        {
            items.Add(new RibbonSeparator());
        }
                

        public static RibbonSplitButton AddSplitButton(this RibbonItemCollection panelSrc, string text, ICommand command, Bitmap image)
        {
            RibbonSplitButton splitBtn = new RibbonSplitButton();
            panelSrc.AddItem(splitBtn, text, command, image, image);
            splitBtn.IsSplit = false;

            return splitBtn;
        }
        
    }
}
