using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using System.Drawing;


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
using Geo7.Tools;
#endif

namespace Geo7.Windows
{
    /// <summary>
    /// Interaction logic for BlocksImportWindow.xaml
    /// </summary>
    public partial class BlockImpTextWindow : System.Windows.Window
    {
        public BlockImpTextWindow()
        {
            InitializeComponent();           
        }
        
        
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            //this.Close(); this.IsCancel = true
        }
                

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            //AppServices.Dialog.ShowInfo("BtnOK_Click");
            this.DialogResult = true;
            this.Close();
        }
    }
}
