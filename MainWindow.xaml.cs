using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Threading;

namespace IAT_Application
{
    public partial class MainWindow : Window
    {
        private DateTime _startTime;
        private List<string> _results = new List<string>();
        private List<string> _imagePaths = new List<string>();
        private int _currentImageIndex = 0;
        private bool _isCongruentMapping = true; // This can be randomized
        private DateTime _rsptStartTime;
        private DispatcherTimer _timer;
        private AppState _currentState;
        private string _currentImageName;
        private enum AppState
        {
            Start,
            PlusSymbol,
            DisplayImage,
            AwaitingKeyPress,
            End
        }


        public MainWindow()
        {
            InitializeComponent(); // 1. Initialize UI components
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) }; // 2. Set up your timer and its event
            _timer.Tick += Timer_Tick;
            LoadImagesFromFolder("IAT_Resources", "Images"); // 3. Load the images from the folder

            // Set the initial state AFTER initializing components
            _currentState = AppState.Start;

            // Load the first image (plus symbol) and start the timer
            LoadNextImage();

            this.KeyDown += MainWindow_KeyDown; // 6. Attach the key down event handler
        }


        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Title.Visibility = Visibility.Collapsed;
            StartButton.Visibility = Visibility.Collapsed;

            // Set the initial state to PlusSymbol and display the plus symbol
            _currentState = AppState.PlusSymbol;
            PlusSymbol.Visibility = Visibility.Visible;

            // Start the timer after the plus symbol is displayed
            _timer.Start();

            // Load the first image
            LoadNextImage();

            MainContent.Visibility = Visibility.Visible;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (_currentState != AppState.PlusSymbol) // Check if the current state is not PlusSymbol
            {
                if (e.Key == Key.A || e.Key == Key.L)
                {
                    TimeSpan reactionTime = DateTime.Now - _rsptStartTime;
                    _results.Add($"Image: {_currentImageName}\n  Key {e.Key} pressed in {reactionTime.TotalMilliseconds} ms.");

                    if (_currentState == AppState.End)
                    {
                        // Save results and show a completion message
                        SaveResults();
                        MessageBox.Show("Test completed! Results saved.", "Completion", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                    }
                    else
                    {
                        // Continue with the test
                        LoadNextImage();
                    }
                }
            }
        }

        private void LoadImagesFromFolder(params string[] relativePathParts)
        {
            // Navigate up from the bin directory to the root of your project
            string rootProjectPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName).FullName;

            string folderPath = rootProjectPath;
            foreach (var part in relativePathParts)
            {
                folderPath = System.IO.Path.Combine(folderPath, part);
            }

            if (!Directory.Exists(folderPath))
            {
                throw new Exception($"Directory not found: {folderPath}");
            }

            _imagePaths = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                                   .Where(file => new string[] { ".jpeg", ".jpg", ".png", ".bmp" }.Any(ext => file.EndsWith(ext)))
                                   .ToList();
        }

        private void LoadNextImage()
        {
            if (_currentState == AppState.PlusSymbol)
            {
                // Show ImageBox and buttons
                ImageBox.Visibility = Visibility.Visible;
                Button1.Visibility = Visibility.Visible;
                Button2.Visibility = Visibility.Visible;

                PlusSymbol.Visibility = Visibility.Collapsed;
                if (_currentImageIndex < _imagePaths.Count)
                {
                    _currentImageName = System.IO.Path.GetFileNameWithoutExtension(_imagePaths[_currentImageIndex]); // Get the image file name without extension
                    DisplayedImage.Source = new BitmapImage(new Uri(_imagePaths[_currentImageIndex], UriKind.Absolute));
                    _currentImageIndex++;                    
                    _currentState = AppState.AwaitingKeyPress;
                    _rsptStartTime = DateTime.Now; // Set the _rsptStartTime when an image is shown.
                }
                else
                {
                    _currentState = AppState.End;
                    // No need to show the plus symbol at the end
                }
            }
            else if (_currentState == AppState.DisplayImage || _currentState == AppState.AwaitingKeyPress)
            {
                if (_currentImageIndex >= _imagePaths.Count)
                {
                    _currentState = AppState.End;
                    SaveResults();
                    MessageBox.Show("Test completed! Results saved.", "Completion", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    // Hide ImageBox and buttons
                    ImageBox.Visibility = Visibility.Collapsed;
                    Button1.Visibility = Visibility.Collapsed;
                    Button2.Visibility = Visibility.Collapsed;

                    PlusSymbol.Visibility = Visibility.Visible;
                    DisplayedImage.Source = null;
                    _timer.Start();            
                    _currentState = AppState.PlusSymbol;
                }
            }
        }




        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            LoadNextImage();
        }

        // Call this method when you want to save the results
        private void SaveResults()
        {
            // Navigate up from the bin directory to the root of your project
            string rootProjectPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName).FullName;

            string resultsFolderPath = System.IO.Path.Combine(rootProjectPath, "IAT_Resources", "Results");

            // Check if the directory exists, if not, create it
            if (!Directory.Exists(resultsFolderPath))
            {
                Directory.CreateDirectory(resultsFolderPath);
            }

            string resultsPath = System.IO.Path.Combine(resultsFolderPath, "results.txt");
            System.IO.File.WriteAllLines(resultsPath, _results);
        }


    }
}
