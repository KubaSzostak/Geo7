using System.Windows;

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
