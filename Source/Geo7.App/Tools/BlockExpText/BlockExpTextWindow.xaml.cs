using System;
using System.Collections.Generic;
using System.Windows;
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
