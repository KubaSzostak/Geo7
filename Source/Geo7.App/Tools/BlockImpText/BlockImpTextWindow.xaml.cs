using System.Windows;



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
