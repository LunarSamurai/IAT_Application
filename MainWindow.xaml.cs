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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IAT_Application
{
    public partial class MainWindow : Window
    {
        private DateTime _startTime;
        private List<string> _results = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            LoadNextImage();
            _startTime = DateTime.Now;
        }

        private void LoadNextImage()
        {
            // Load your image here. For example:
            // DisplayedImage.Source = new BitmapImage(new Uri("path_to_your_image.jpg", UriKind.Relative));
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            RecordAnswer("Option 1");
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            RecordAnswer("Option 2");
        }

        private void RecordAnswer(string option)
        {
            TimeSpan timeTaken = DateTime.Now - _startTime;
            _results.Add($"Answered {option} in {timeTaken.TotalSeconds} seconds.");

            // Reset the timer
            _startTime = DateTime.Now;

            // Load the next image or finish the test
            LoadNextImage();
        }

        // Call this method when you want to save the results
        private void SaveResults()
        {
            string path = "path_to_your_desired_folder"; // Specify your path
            System.IO.File.WriteAllLines(System.IO.Path.Combine(path, "results.txt"), _results);
        }
    }
}
