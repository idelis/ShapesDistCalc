using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace DistTraveled_Calculator
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variables
        BackgroundWorker bg;
        bool stop = false;
        string fileStopsPath = null;
        string fileTransfersPath = null;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            InitializeBackgroundWorker();
        }

        #region FilesSelection
        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text file(*.txt) | *.txt";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (openFileDialog.ShowDialog() == true)
            {
                txtFileStops.Text = openFileDialog.FileName;
                if (txtFileTransfers.Text != "")
                    btnConvert.IsEnabled = true;
            }
        }
        private void btnOpenFileTransfers_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text file(*.txt) | *.txt";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (saveFileDialog.ShowDialog() == true)
            {
                txtFileTransfers.Text = saveFileDialog.FileName;
                if (txtFileStops.Text != "")
                    btnConvert.IsEnabled = true;
            }
        }

        #endregion

        #region Initialization
        private void InitializeProgressBar()
        {
            progressBarConvert.Value = 0;
            progressBarConvert.Maximum = 100;
        }
        private void InitializeBackgroundWorker()
        {
            bg = new BackgroundWorker();
            bg.DoWork += Bg_DoWork;
            bg.ProgressChanged += Bg_ProgressChanged;
            bg.RunWorkerCompleted += Bg_RunWorkerCompleted;
            bg.WorkerReportsProgress = true;
            bg.WorkerSupportsCancellation = true;
        }
        #endregion

        private void Bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                btnCancel.IsEnabled = false;
                btnConvert.IsEnabled = true;
                progressBarConvert.IsIndeterminate = false;
                MessageBoxResult result = MessageBox.Show("Opération annulée !", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                btnCancel.IsEnabled = false;
                btnConvert.IsEnabled = true;
                progressBarConvert.IsIndeterminate = false;
                progressBarConvert.Value = 100;
                MessageBoxResult result = MessageBox.Show("Le fichier à été compilé avec succès !\n\nVoulez-vous ouvrir le fichier ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    Process.Start(fileTransfersPath);
                }
            }
        }

        private void Bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void Bg_DoWork(object sender, DoWorkEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                progressBarConvert.Value = 0;
                fileStopsPath = txtFileStops.Text;
                fileTransfersPath = txtFileTransfers.Text;
                btnCancel.IsEnabled = true;
                progressBarConvert.IsIndeterminate = true;
            });

            #region Variables
            List<string> fileShapes1 = new List<string>();
            fileShapes1 = File.ReadAllLines(fileStopsPath).ToList();
            int colShape_Id = fileShapes1[0].Split(',').Select((c, ix) => new { col = c, index = ix }).FirstOrDefault(c => c.col.Contains("shape_id")).index;
            int colShape_lat = fileShapes1[0].Split(',').Select((c, ix) => new { col = c, index = ix }).FirstOrDefault(c => c.col.Contains("shape_pt_lat")).index;
            int colShape_lon = fileShapes1[0].Split(',').Select((c, ix) => new { col = c, index = ix }).FirstOrDefault(c => c.col.Contains("shape_pt_lon")).index;
            int colShape_sequence = fileShapes1[0].Split(',').Select((c, ix) => new { col = c, index = ix }).FirstOrDefault(c => c.col.Contains("shape_pt_sequence")).index;
            int colShape_distTraveled = fileShapes1[0].Split(',').Select((c, ix) => new { col = c, index = ix }).FirstOrDefault(c => c.col.Contains("shape_dist_traveled")).index;
            String[] lineValues1;
            String[] lineValues2;
            double lat1 = 0;
            double lat2 = 0;
            double lon1 = 0;
            double lon2 = 0;
            decimal total = 0;
            int j = 0;
            #endregion
            using (StreamWriter writer = new StreamWriter(fileTransfersPath))
            {
                writer.WriteLine("shape_id,shape_pt_lat,shape_pt_lon,shape_pt_sequence,shape_dist_traveled");
                for (int i = 1; i < fileShapes1.Count(); i++)
                {
                    j = i - 1;
                    lineValues1 = fileShapes1[i].Split(',');
                    if (lineValues1[colShape_sequence] == "1")
                    {
                        if (bg.CancellationPending)
                        {
                            e.Cancel = true;
                        }
                        else
                        {
                            total = 0;
                            writer.WriteLine(lineValues1[colShape_Id] + "," + lineValues1[colShape_lat] + "," + lineValues1[colShape_lon] + "," + lineValues1[colShape_sequence] + "," + total);
                        }
                    }
                    else
                    {
                        if (bg.CancellationPending)
                        {
                            e.Cancel = true;
                        }
                        else
                        {
                            lineValues2 = fileShapes1[j].Split(',');
                            if (lineValues2[colShape_Id] != null)
                            {
                                if (lineValues2[colShape_lat].Any(char.IsDigit) || lineValues2[colShape_lon].Any(char.IsDigit))
                                {
                                    lat1 = Convert.ToDouble(lineValues1[colShape_lat].Replace('.', ','));
                                    lon1 = Convert.ToDouble(lineValues1[colShape_lon].Replace('.', ','));
                                    lat2 = Convert.ToDouble(lineValues2[colShape_lat].Replace('.', ','));
                                    lon2 = Convert.ToDouble(lineValues2[colShape_lon].Replace('.', ','));
                                }
                                total = CalculateGeoDistance(lat1, lon1, lat2, lon2) + total;
                                string value = total.ToString().Replace(',', '.');
                                writer.WriteLine(lineValues1[colShape_Id] + "," + lineValues1[colShape_lat] + "," + lineValues1[colShape_lon] + "," + lineValues1[colShape_sequence] + "," + total.ToString().Replace(',', '.'));
                            }
                        }
                    }
                }
            }
        }


        #region Functions
        private decimal CalculateGeoDistance(double lat1, double lon1, double lat2, double lon2)
        {
            if ((lat1 == lat2) && (lon1 == lon2))
            {
                return 0;
            }
            else
            {
                decimal distance = 0;
                double dist = 0;
                double rlat1 = Math.PI * lat1 / 180;
                double rlat2 = Math.PI * lat2 / 180;
                double theta = lon1 - lon2;
                double rtheta = Math.PI * theta / 180;
                dist = Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) * Math.Cos(rlat2) * Math.Cos(rtheta);
                dist = Math.Acos(dist);
                dist = dist * 180 / Math.PI;
                dist = dist * 60 * 1.1515 * 1.609344;
                distance = (Decimal)dist;
                distance = TruncateDecimalPlace(distance, 9);
                return distance;
            }
        }

        private decimal TruncateDecimalPlace(decimal numberToTruncate, int decimalPlaces)
        {
            decimal power = (decimal)(Math.Pow(10.0, (double)decimalPlaces));
            return Math.Truncate((power * numberToTruncate)) / power;
        }
        #endregion

        #region Buttons
        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Show();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BtnConvert_Click(object sender, RoutedEventArgs e)
        {
            bg.RunWorkerAsync();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Voulez-vous vraiment arreter le processus en cours ?", "Attention", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                bg.CancelAsync();
            }
        }
        #endregion

    }
}