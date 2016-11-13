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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Geo7;
using Geo7.Tools;

namespace Geo7.ToolPalettes
{
    /// <summary>
    /// Interaction logic for BlocksPalette.xaml
    /// </summary>
    public partial class BlockFindPage : Page
    {
        public BlockFindPage()
        {
            InitializeComponent();
            DataContext = new BlockFindPresenter();
        }

        private void BlockSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            //Ac.ShowModal(new BlockSettingsWindow());
        }

        private void Find_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.UpdateLayout();
                (DataContext as BlockFindPresenter).FindCommand.Execute(null);
            }
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
