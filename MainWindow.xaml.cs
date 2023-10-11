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
using WpfAnimatedGif;
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
        private bool _secondSetDisplayed = false;
        private string descriptionValue;
        private string isMilitary;
        private string whatsThumbsUp;
        private string modifierThumbsUpValue;
        private string whatsThumbsDown;
        private string modifierThumbsDownValue;
        private List<string> militaryImages = new List<string>();
        private List<string> nonMilitaryImages = new List<string>();
        private string answer;
        private readonly DispatcherTimer _splashTimer = new DispatcherTimer();

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

            // Setting the animated GIF for the splash screen
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(@"\IAT_Resources\Logo\logo.gif", UriKind.RelativeOrAbsolute);
            image.EndInit();
            ImageBehavior.SetAnimatedSource(img, image);  // Assuming SplashImage is the name of your Image control in XAML

            // Initialize and start the timer for the splash screen
            _splashTimer = new DispatcherTimer();
            _splashTimer.Interval = TimeSpan.FromSeconds(5);  // Set to your desired duration
            _splashTimer.Tick += OnSplashTimerTick;
            _splashTimer.Start();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) }; // 2. Set up your timer and its event
            _timer.Tick += Timer_Tick;
            LoadImagesFromFolder("IAT_Resources", "Images"); // 3. Load the images from the folder

            // Set the initial state AFTER initializing components
            _currentState = AppState.Start;

            // Load the first image (plus symbol) and start the timer
            LoadNextImage();

            this.KeyDown += MainWindow_KeyDown; // 6. Attach the key down event handler
        }

        private void OnSplashTimerTick(object sender, EventArgs e)
        {
            _splashTimer.Stop();
            SplashScreen.Visibility = Visibility.Collapsed;
            // Show other main content if needed
            StartScreen.Visibility = Visibility.Visible;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartScreen.Visibility = Visibility.Collapsed; // Hide the start screen

            MainContent.Visibility = Visibility.Visible; // Make the main content grid visible

            DescriptionTextBlockGrid.Visibility = Visibility.Visible; // Show the description grid
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (_currentState == AppState.DisplayImage || _currentState == AppState.AwaitingKeyPress)
            {
                string answer = "Incorrect"; // Default to incorrect

                // If the current test is for "Military" images
                if (isMilitary == "Military")
                {
                    // If the image name contains "Military" and the key pressed matches modifierThumbsUpValue
                    if (militaryImages.Contains(_imagePaths[_currentImageIndex]) && e.Key.ToString() == modifierThumbsUpValue)
                    {
                        answer = "Correct";
                    }
                    // If the image name does not contain "Military" and the key pressed matches modifierThumbsDownValue
                    else if (nonMilitaryImages.Contains(_imagePaths[_currentImageIndex]) && e.Key.ToString() == modifierThumbsDownValue)
                    {
                        answer = "Correct";
                    }
                }
                // If the current test is for "NonMilitary" images
                else if (isMilitary == "NonMilitary")
                {
                    // If the image name does not contain "Military" and the key pressed matches modifierThumbsUpValue
                    if (nonMilitaryImages.Contains(_imagePaths[_currentImageIndex]) && e.Key.ToString() == modifierThumbsUpValue)
                    {
                        answer = "Correct";
                    }
                    // If the image name contains "Military" and the key pressed matches modifierThumbsDownValue
                    else if (militaryImages.Contains(_imagePaths[_currentImageIndex]) && e.Key.ToString() == modifierThumbsDownValue)
                    {
                        answer = "Correct";
                    }
                }

                TimeSpan reactionTime = DateTime.Now - _rsptStartTime;
                _results.Add($"{descriptionValue}: {isMilitary}, ImageName: {_currentImageName}, Key Pressed: {e.Key}, ThumbsUp: {modifierThumbsUpValue}, ThumbsDown: {modifierThumbsDownValue}, Answer: {answer}, pressed in {reactionTime.TotalMilliseconds} ms");

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

            // Read all lines from the file
            var lines = System.IO.File.ReadAllLines(fullPath);

            // Process the Description files
            if (relativePath == @"IAT_Resources\Description\Description1\Description1.txt" ||
                relativePath == @"IAT_Resources\Description\Description2\Description2.txt")
            {
                // Check if we have at least two lines
                if (lines.Length >= 2)
                {
                    // Set the two variables with the first two lines
                    descriptionValue = lines[0];
                    isMilitary = lines[1];

                    // Return the content of the file, skipping the first two lines
                    return string.Join(Environment.NewLine, lines.Skip(2));
                }
            }
            // Process the Modifier files
            else if (relativePath == @"IAT_Resources\Modifiers\Modifier1\Modifier1.txt" ||
                     relativePath == @"IAT_Resources\Modifiers\Modifier2\Modifier2.txt")
            {
                // Check if we have at least four lines
                if (lines.Length >= 4)
                {
                    // Set the four variables with the first four lines
                    whatsThumbsUp = lines[0];
                    modifierThumbsUpValue = lines[1];
                    whatsThumbsDown = lines[2];
                    modifierThumbsDownValue = lines[3];

                    // Return the content of the file, skipping the first four lines
                    return string.Join(Environment.NewLine, lines.Skip(4));
                }
            }

            // For other files, return the entire content
            return string.Join(Environment.NewLine, lines);
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

        private void DisplayImage()
        {
            ImageBox.Visibility = Visibility.Visible;
            PlusSymbol.Visibility = Visibility.Collapsed;
            _currentImageName = System.IO.Path.GetFileNameWithoutExtension(_imagePaths[_currentImageIndex]);
            DisplayedImage.Source = new BitmapImage(new Uri(_imagePaths[_currentImageIndex], UriKind.Absolute));
            _currentState = AppState.DisplayImage;
            _rsptStartTime = DateTime.Now;
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

            foreach (var imagePath in _imagePaths)
            {
                if (System.IO.Path.GetFileNameWithoutExtension(imagePath).Contains("Military"))
                {
                    militaryImages.Add(imagePath);
                }
                else
                {
                    nonMilitaryImages.Add(imagePath);
                }
            }
        }


        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            DescriptionTextBlockGrid.Visibility = Visibility.Collapsed; // Hide the description and button by default
            PlusSymbol.Visibility = Visibility.Collapsed; // Hide the plus symbol by default

            switch (_currentState)
            {
                case AppState.Description1:
                    _currentState = AppState.Modifier1;
                    DisplayContent(_currentState);
                    DescriptionTextBlockGrid.Visibility = Visibility.Visible; // Show the description and button
                    break;

                case AppState.Modifier1:
                    _currentState = AppState.PlusSymbol;
                    PlusSymbol.Visibility = Visibility.Visible;
                    _timer.Start();
                    break;

                case AppState.Description2:
                    _currentState = AppState.Modifier2;
                    DisplayContent(_currentState);
                    DescriptionTextBlockGrid.Visibility = Visibility.Visible; // Show the description and button
                    break;

                case AppState.Modifier2:
                    _currentImageIndex = 0; // Reset image index to start over with the images
                    _currentState = AppState.PlusSymbol;
                    PlusSymbol.Visibility = Visibility.Visible;
                    _timer.Start();
                    break;

                default:
                    // For all other states, we'll just progress to the next image/state.
                    LoadNextImage();
                    break;
            }
        }

        private void LoadNextImage()
        {
            switch (_currentState)
            {
                case AppState.Start:
                    DisplayContent(AppState.Description1);
                    _currentState = AppState.Description1;
                    break;

                case AppState.Description1:
                    DisplayContent(AppState.Modifier1);
                    _currentState = AppState.Modifier1;
                    break;

                case AppState.Modifier1:
                    _currentState = AppState.PlusSymbol;
                    _timer.Start();
                    break;

                case AppState.PlusSymbol:
                    DescriptionTextBlockGrid.Visibility = Visibility.Collapsed; // Ensure description is hidden

                    if (_currentImageIndex < _imagePaths.Count && !_secondSetDisplayed)
                    {
                        // Display images from the first set
                        DisplayImage();
                    }
                    else if (!_secondSetDisplayed)
                    {
                        // Transition to the Description2 state after the first set of images
                        _secondSetDisplayed = true;
                        _currentImageIndex = 0; // Reset the image index for the second set
                        DisplayContent(AppState.Description2);
                        _currentState = AppState.Description2;
                        DescriptionTextBlockGrid.Visibility = Visibility.Visible;
                        PlusSymbol.Visibility = Visibility.Collapsed;
                    }
                    else if (_currentImageIndex < _imagePaths.Count && _secondSetDisplayed)
                    {
                        // Display images from the second set
                        DisplayImage();
                    }
                    else
                    {
                        _currentState = AppState.End;
                        LoadNextImage(); // Process the end state
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
