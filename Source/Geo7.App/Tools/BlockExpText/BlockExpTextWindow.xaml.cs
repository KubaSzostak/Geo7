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
using Geo7.Tools;

namespace Geo7.Windows
{
    /// <summary>
    /// Interaction logic for BlockExportWindow.xaml
    /// </summary>
    public partial class BlockExpTextWindow : System.Windows.Window
    {
        public BlockExpTextWindow(List<AcBlockRef> blockRefs)
        {
            InitializeComponent();
            DataContext = new BlockExpTextPresenter(blockRefs);
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
