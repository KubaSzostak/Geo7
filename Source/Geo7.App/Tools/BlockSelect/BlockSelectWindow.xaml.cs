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

namespace Geo7.Tools
{
    /// <summary>
    /// Interaction logic for BlockSelectWindow.xaml
    /// </summary>
    public partial class BlockSelectWindow : Window
    {
        public BlockSelectWindow(BlockRefsAction action)
        {
            InitializeComponent();
            DataContext = new BlockSelectPresenter(this, action);
        }
        

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
