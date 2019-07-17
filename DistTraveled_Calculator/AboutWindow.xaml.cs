using System.Windows;

namespace DistTraveled_Calculator
{
    /// <summary>
    /// Logique d'interaction pour AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            txtAbout.Text = "Cette application permet le calcul des shapes_dist_traveled dans un fichier shapes.txt.\n\nVersion 1.0.0\n\nIDELIS © 2019";
        }
    }
}