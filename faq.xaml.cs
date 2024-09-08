using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FreeNet
{
    /// <summary>
    /// Interaction logic for faq.xaml
    /// </summary>
    public partial class faq : Window
    {
        public faq()
        {
            InitializeComponent();
        }

        private void RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Hyperlink hp = sender as Hyperlink;
            System.Diagnostics.Process.Start("explorer.exe", hp.NavigateUri.ToString());
        }
    }
}
