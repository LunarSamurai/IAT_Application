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
            Description1,
            Modifier1,
            PlusSymbol,
            DisplayImage,
            AwaitingKeyPress,
            Description2,
            Modifier2,
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
            StartScreen.Visibility = Visibility.Collapsed; // Hide the start screen

            MainContent.Visibility = Visibility.Visible; // Make the main content grid visible

            DescriptionTextBlockGrid.Visibility = Visibility.Visible; // Show the description grid
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
        private string LoadContentFromFile(string relativePath)
        {
            // Get the base directory of the currently executing assembly
            string basePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            // Navigate up three directories from the bin directory to the root of your project
            string projectRootPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(basePath).FullName).FullName).FullName;

            // Combine the project root directory with the relative path to get the absolute path
            string fullPath = System.IO.Path.Combine(projectRootPath, relativePath);

            // Check if the file exists
            if (!System.IO.File.Exists(fullPath))
            {
                throw new Exception($"File not found: {fullPath}");
            }

            // Read and return the content of the file
            return System.IO.File.ReadAllText(fullPath);
        }


        private void DisplayContent(AppState state)
        {
            string content = string.Empty;
            switch (state)
            {
                case AppState.Description1:
                    content = LoadContentFromFile(@"IAT_Resources\Description\Description1\Description1.txt");
                    break;
                case AppState.Modifier1:
                    content = LoadContentFromFile(@"IAT_Resources\Modifiers\Modifier1\Modifier1.txt");
                    break;
                case AppState.Description2:
                    content = LoadContentFromFile(@"IAT_Resources\Description\Description2\Description2.txt");
                    break;
                case AppState.Modifier2:
                    content = LoadContentFromFile(@"IAT_Resources\Modifiers\Modifier2\Modifier2.txt");
                    break;
            }
            DescriptionTextBlock.Text = content;
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

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentState != AppState.Description1 && _currentState != AppState.Description2)
            {
                // Set the initial state to PlusSymbol and display the plus symbol
                _currentState = AppState.PlusSymbol;
                PlusSymbol.Visibility = Visibility.Visible;

                // Start the timer after the plus symbol is displayed
                _timer.Start();

                MainContent.Visibility = Visibility.Visible;
                // Move to the next state when the "Continue" button is pressed
                LoadNextImage();
            }   
            else if(_currentState == AppState.Description1)
            {
                _currentState = AppState.Modifier1;
            }
            else if (_currentState == AppState.Description2)
            {
                _currentState = AppState.Modifier2;
            }


        }

        private void LoadNextImage()
        {
            switch (_currentState)
            {
                case AppState.Start:
                    DisplayContent(AppState.Description1);
                    _currentState = AppState.Description1;
                    _timer.Start();
                    break;

                case AppState.Description1:
                    DisplayContent(AppState.Modifier1);
                    _currentState = AppState.Modifier1;
                    _timer.Start();
                    break;

                case AppState.Modifier1:
                    _currentState = AppState.PlusSymbol;
                    _timer.Start();
                    break;

                case AppState.PlusSymbol:
                    DescriptionTextBlockGrid.Visibility = Visibility.Collapsed;
                    if (_currentImageIndex >= _imagePaths.Count)
                    {
                        DisplayContent(AppState.Description2);
                        _currentState = AppState.Description2;
                        _timer.Start();
                    }
                    else
                    {
                        ImageBox.Visibility = Visibility.Visible;
                        PlusSymbol.Visibility = Visibility.Collapsed;

                        _currentImageName = System.IO.Path.GetFileNameWithoutExtension(_imagePaths[_currentImageIndex]);
                        DisplayedImage.Source = new BitmapImage(new Uri(_imagePaths[_currentImageIndex], UriKind.Absolute));
                        _currentState = AppState.DisplayImage;
                        _rsptStartTime = DateTime.Now;
                    }
                    break;

                case AppState.DisplayImage:
                case AppState.AwaitingKeyPress:
                    _currentImageIndex++;

                    ImageBox.Visibility = Visibility.Collapsed;
                    DescriptionTextBlockGrid.Visibility = Visibility.Collapsed;
                    DisplayedImage.Source = null;
                    PlusSymbol.Visibility = Visibility.Visible;

                    _currentState = AppState.PlusSymbol;
                    _timer.Start();
                    break;

                case AppState.Description2:
                    DisplayContent(AppState.Modifier2);
                    _currentState = AppState.Modifier2;
                    _timer.Start();
                    break;

                case AppState.Modifier2:
                    _currentImageIndex = 0; // Reset image index to start over with the images
                    _currentState = AppState.PlusSymbol;
                    _timer.Start();
                    break;

                case AppState.End:
                    SaveResults();
                    MessageBox.Show("Test completed! Results saved.", "Completion", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                    break;
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
