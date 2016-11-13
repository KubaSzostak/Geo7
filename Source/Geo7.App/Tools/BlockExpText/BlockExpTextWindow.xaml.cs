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
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Interop;


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
    /// Interaction logic for BlockExportWindow.xaml
    /// </summary>
    public partial class BlockExpTextWindow : System.Windows.Window
    {
        public BlockExpTextWindow()
        {
            InitializeComponent();
            //imgSelectBlocks.Source = Resources.
            
            DataContext = new BlockExpTextPresenter(this); 
        }

        

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnSelectBlocks_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
