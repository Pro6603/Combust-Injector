using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Combust_Injector
{
    public partial class MainWindow : Window
    {
        private Border _selectedCard;
        // Add this as a private field in your MainWindow class for reuse if you want:
        private DropShadowEffect _loadButtonGlowEffect;

        private readonly Random _random = new Random();
        private readonly DispatcherTimer _animationTimer = new DispatcherTimer();

        private readonly List<Dot> _dots = new List<Dot>();
        private DateTime _lastUpdateTime = DateTime.Now;

        private const int DotCount = 40;
        private const double DotRadius = 2.5;
        private const double ConnectionDistance = 100;

        public MainWindow()
        {
            InitializeComponent();

            PopulateCards();
            GenerateDots(40); // Adjust the number of dots
            StartDotAnimation();
            InitializeLoadButtonGlow();
        }

        private void LoadButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (_selectedCard?.Tag is CardData data && data.Id == "combust_inject1")
            {
                // BELOW, there are 3 ways the dllpath string is defined. each option represents a diffrent way of acessing the dll.
                // for external users, the second option is reccomended, because the others literally dont work.
                // when building the software, make sure to use the top one.

                // THIS ONE WHEN BUILDING
                string dllPath = @"C:\Users\Max\VS\projects-cpp\Combust\x64\Debug\Combust.dll"; // grab dll externally (dev envrioment reccomended)

                // string dllPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Combust.dll"); // grab dll internally (USES CMD STILL)

                /*
                string dllPath = System.IO.Path.Combine( // grab dll from desktop (broken atm)
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "combustclient",
                    "Combust.dll"
                );
                */



                if (!File.Exists(dllPath))
                {
                    MessageBox.Show($"❌ DLL not found at:\n{dllPath}", "Missing DLL", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                bool success = ReliableDllInjector.Inject("javaw", dllPath);

                if (success)
                {
                    MessageBox.Show("Combust injected successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            if (_selectedCard?.Tag is CardData data2 && data2.Id == "combust_inject2")
            {
                string dllPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Combust.dll");

                if (!File.Exists(dllPath))
                {
                    MessageBox.Show($"❌ DLL not found at:\n{dllPath}", "Missing DLL", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                bool success = ReliableDllInjector.Inject("javaw", dllPath);

                if (success)
                {
                    MessageBox.Show("Combust injected successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("⚠️ This option is invalid.", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }




        private void ExtractEmbeddedDll(string resourceName, string outputPath)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new FileNotFoundException("Embedded resource not found: " + resourceName);

                using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void InitializeLoadButtonGlow()
        {
            _loadButtonGlowEffect = new DropShadowEffect
            {
                Color = Color.FromRgb(180, 150, 255), // Same light purple glow
                BlurRadius = 15,
                ShadowDepth = 0,
                Opacity = 0
            };

            LoadButton.Effect = _loadButtonGlowEffect;

            LoadButton.MouseEnter += (s, e) =>
            {
                AnimateGlowOpacity(_loadButtonGlowEffect, 0.7, 200); // fade in to 0.7 over 200ms
            };

            LoadButton.MouseLeave += (s, e) =>
            {
                AnimateGlowOpacity(_loadButtonGlowEffect, 0, 200); // fade out to 0 over 200ms
            };
        }

        private void AnimateGlowOpacity(DropShadowEffect effect, double toOpacity, int durationMilliseconds)
        {
            var animation = new System.Windows.Media.Animation.DoubleAnimation
            {
                To = toOpacity,
                Duration = TimeSpan.FromMilliseconds(durationMilliseconds),
                AccelerationRatio = 0.3,
                DecelerationRatio = 0.3
            };
            effect.BeginAnimation(DropShadowEffect.OpacityProperty, animation);
        }


        private void PopulateCards()
        {
            CardsItemsControl.Items.Clear();

            CardsItemsControl.Items.Add(CreateCard("combust_inject1", "Combust DEV", "1.21", "Combust designed for Minecraft 1.21 (DONT USE, ONLY WORKS ON THE DEVELOPERS MACHINE)"));
            CardsItemsControl.Items.Add(CreateCard("combust_inject2", "Combust local", "1.21", "BETA version, made to test diffrent methods of grabbing the dll"));
        }




        private Border CreateCard(string id, string name, string sideDescription, string description)
        {
            var glowEffect = new DropShadowEffect
            {
                Color = Color.FromRgb(180, 150, 255), // Light purple
                BlurRadius = 15,
                ShadowDepth = 0,
                Opacity = 0
            };

            var cardData = new CardData
            {
                Id = id,
                Name = name,
                SideDescription = sideDescription,
                Description = description
            };

            var border = new Border
            {
                Tag = cardData,  // Store full CardData here
                Width = Double.NaN,
                MaxWidth = 345,
                MinWidth = 345,
                Height = 99,
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(Color.FromRgb(30, 33, 44)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(60, 66, 88)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(10, 6, 10, 6),
                HorizontalAlignment = HorizontalAlignment.Center,
                Cursor = Cursors.Hand,
                Effect = glowEffect,
                Child = new Grid
                {
                    Margin = new Thickness(12),
                    ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            },
                    RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
            }
                }
            };

            var nameTextBlock = new TextBlock
            {
                Text = name,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.White,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                TextAlignment = TextAlignment.Left
            };
            Grid.SetRow(nameTextBlock, 0);
            Grid.SetColumn(nameTextBlock, 0);

            var sideDescTextBlock = new TextBlock
            {
                Text = sideDescription,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 50, 150)),
                FontSize = 14,
                FontWeight = FontWeights.Medium,
                TextAlignment = TextAlignment.Right,
                Margin = new Thickness(8, 0, 0, 0)
            };
            Grid.SetRow(sideDescTextBlock, 0);
            Grid.SetColumn(sideDescTextBlock, 1);

            var descTextBlock = new TextBlock
            {
                Text = description,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Foreground = new SolidColorBrush(Color.FromRgb(150, 120, 180)),
                FontSize = 13,
                FontWeight = FontWeights.Normal,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Left,
                Margin = new Thickness(0, 6, 0, 0)
            };
            Grid.SetRow(descTextBlock, 1);
            Grid.SetColumn(descTextBlock, 0);
            Grid.SetColumnSpan(descTextBlock, 2);

            // Add all TextBlocks to the Grid children
            ((Grid)border.Child).Children.Add(nameTextBlock);
            ((Grid)border.Child).Children.Add(sideDescTextBlock);
            ((Grid)border.Child).Children.Add(descTextBlock);

            border.MouseLeftButtonUp += (s, e) =>
            {
                SelectCard(border, name);
                SelectedCardText.Text = name;
                LoadButton.IsEnabled = true;
            };

            return border;
        }


        private void SelectCard(Border selected, string title)
        {
            // No change if the same card is selected again
            if (_selectedCard == selected)
                return;

            // Fade out glow on previously selected card
            if (_selectedCard != null)
            {
                AnimateGlowOpacity(_selectedCard, 0);
                _selectedCard.BorderBrush = new SolidColorBrush(Color.FromRgb(60, 66, 88)); // Default border color
            }

            // Update the selected card reference
            _selectedCard = selected;

            if (_selectedCard != null)
            {
                AnimateGlowOpacity(_selectedCard, 1); // Fade in glow
                _selectedCard.BorderBrush = new SolidColorBrush(Color.FromRgb(180, 150, 255)); // Light purple glow color
            }

            // Update UI
            SelectedCardText.Text = title;
            LoadButton.IsEnabled = true;
        }

        // Animate glow opacity helper method
        private void AnimateGlowOpacity(Border border, double toOpacity)
        {
            if (border.Effect is DropShadowEffect glow)
            {
                var animation = new DoubleAnimation
                {
                    To = toOpacity,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                };
                glow.BeginAnimation(DropShadowEffect.OpacityProperty, animation);
            }
        }


        private void GenerateDots(int count)
        {
            double width = BackgroundCanvas.ActualWidth > 0 ? BackgroundCanvas.ActualWidth : 770;
            double height = BackgroundCanvas.ActualHeight > 0 ? BackgroundCanvas.ActualHeight : 450;

            _dots.Clear();
            BackgroundCanvas.Children.Clear();

            for (int i = 0; i < count; i++)
            {
                var pos = new Point(_random.NextDouble() * width, _random.NextDouble() * height);
                var vel = new Vector(_random.NextDouble() * 50 - 25, _random.NextDouble() * 50 - 25); // velocity px/s

                var ellipse = new Ellipse
                {
                    Width = DotRadius * 2,
                    Height = DotRadius * 2,
                    Fill = new SolidColorBrush(Color.FromRgb(100, 100, 150)),
                    Opacity = 0.3
                };

                Canvas.SetLeft(ellipse, pos.X - DotRadius);
                Canvas.SetTop(ellipse, pos.Y - DotRadius);

                BackgroundCanvas.Children.Add(ellipse);

                _dots.Add(new Dot
                {
                    Position = pos,
                    Velocity = vel,
                    Ellipse = ellipse
                });
            }

            _lastUpdateTime = DateTime.Now;
        }

        private void StartDotAnimation()
        {
            _animationTimer.Interval = TimeSpan.FromMilliseconds(33);
            _animationTimer.Tick += (s, e) => UpdateDots();
            _animationTimer.Start();
        }

        private void UpdateDots()
        {
            var now = DateTime.Now;
            double deltaTime = (now - _lastUpdateTime).TotalSeconds;
            _lastUpdateTime = now;

            double width = BackgroundCanvas.ActualWidth;
            double height = BackgroundCanvas.ActualHeight;

            if (width == 0 || height == 0)
                return;

            // Remove old connection lines (keep only the original ellipses)
            BackgroundCanvas.Children.RemoveRange(DotCount, BackgroundCanvas.Children.Count - DotCount);

            foreach (var dot in _dots)
            {
                var newPos = dot.Position + dot.Velocity * deltaTime;

                // Bounce on edges
                if (newPos.X < DotRadius || newPos.X > width - DotRadius)
                    dot.Velocity = new Vector(-dot.Velocity.X, dot.Velocity.Y);
                if (newPos.Y < DotRadius || newPos.Y > height - DotRadius)
                    dot.Velocity = new Vector(dot.Velocity.X, -dot.Velocity.Y);

                dot.Position += dot.Velocity * deltaTime;

                Canvas.SetLeft(dot.Ellipse, dot.Position.X - DotRadius);
                Canvas.SetTop(dot.Ellipse, dot.Position.Y - DotRadius);
            }

            // Draw connection lines
            for (int i = 0; i < _dots.Count; i++)
            {
                for (int j = i + 1; j < _dots.Count; j++)
                {
                    var d1 = _dots[i];
                    var d2 = _dots[j];
                    var dist = (d1.Position - d2.Position).Length;

                    if (dist <= ConnectionDistance)
                    {
                        byte alpha = (byte)(255 * (1 - dist / ConnectionDistance) * 0.3);
                        var line = new Line
                        {
                            X1 = d1.Position.X,
                            Y1 = d1.Position.Y,
                            X2 = d2.Position.X,
                            Y2 = d2.Position.Y,
                            Stroke = new SolidColorBrush(Color.FromArgb(alpha, 110, 0, 150)),
                            StrokeThickness = 1.2
                        };
                        BackgroundCanvas.Children.Add(line);
                    }
                }
            }
        }

    }

    public static class UIElementExtensions
    {
        public static T SetGrid<T>(this T element, int column, int row, int columnSpan = 1, int rowSpan = 1) where T : UIElement
        {
            Grid.SetColumn(element, column);
            Grid.SetRow(element, row);
            Grid.SetColumnSpan(element, columnSpan);
            Grid.SetRowSpan(element, rowSpan);
            return element;
        }
    }


    // Define a simple class to hold your card data
    public class CardData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SideDescription { get; set; }
        public string Description { get; set; }
    }


    public class Dot
    {
        public Point Position { get; set; }
        public Vector Velocity { get; set; }
        public Ellipse Ellipse { get; set; }
    }
}
