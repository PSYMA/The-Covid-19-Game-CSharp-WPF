namespace The_COVID_19_Game {
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using System.Windows.Threading;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        // good cells
        private List<ProgressBar> gcProgressBarList = new List<ProgressBar>();
        private List<Ellipse> gcList = new List<Ellipse>();
        private List<Ellipse> gcMapList = new List<Ellipse>();

        // virus
        private List<ProgressBar> vProgressBarList = new List<ProgressBar>();
        private List<Ellipse> vList = new List<Ellipse>();
        private List<Ellipse> vMapList = new List<Ellipse>();

        // target cells
        private List<int> gcTargetList = new List<int>();
        
        private DispatcherTimer killCellsTimer = new DispatcherTimer();
        private DispatcherTimer titleTimer = new DispatcherTimer();
        private Random random = new Random();

        // virus images
        private List<string> imagePath = new List<string>();

        // player
        private Ellipse player = new Ellipse();
        private Ellipse playerMap = new Ellipse();
        private Ellipse playerShadow = new Ellipse();

        private double virusSpeed = 5d;
        private double playerSpeed = 10d;
        private double playerDamage = 2d;
        private double virusDamage = .2d;

        private int angleRotate = -1;
        private int ratio = 15;
        private int vIndex = 0;
        private const int numOfCells = 60;
        private const int numOfVirus = 2;
        private const int num = 300;

        private bool moveRight = false;
        private bool moveLeft = false;
        private bool moveUp = false;
        private bool moveDown = false;
        private double time = .5;

        public MainWindow() {
            InitializeComponent();
            AddGoodCells();
            AddCoronaVirus();
            SetTargetCells();

            killCellsTimer.Interval = TimeSpan.FromMilliseconds(time);
            killCellsTimer.Tick += new EventHandler(FindGoodCells);
            killCellsTimer.Tick += new EventHandler(KillCoronaVirus);

            titleTimer.Interval = TimeSpan.FromSeconds(.05);
            titleTimer.Tick += new EventHandler(ChangedTitleOpacity);
            titleTimer.Start();

            PlayerUI();
            // virus images path
            for (int i = 1; i <= 3; i++) {
                imagePath.Add("../../Images/v" + i.ToString() + ".png");
            }   
        }
        double opacity = .1;
        private void KillCoronaVirus(object sender, EventArgs e) {
            time = _timeSlider.Value;
            killCellsTimer.Interval = TimeSpan.FromMilliseconds(time);
            for (int i = 0; i < vList.Count; i++) {
                if (Intersect(playerShadow, vList[i])) {
                    vProgressBarList[i].Value -= playerDamage;
                    if (vProgressBarList[i].Value == 0) {
                        vList[i].Fill = new ImageBrush(new BitmapImage(new Uri("../../Images/cells.png", UriKind.Relative)));
                        vList[i].Stroke = Brushes.White;
                        vList[i].Stretch = Stretch.Fill;
                        vProgressBarList[i].Value = 100;
                        vMapList[i].Fill = Brushes.White;

                        gcList.Add(vList[i]);
                        gcMapList.Add(vMapList[i]);
                        gcProgressBarList.Add(vProgressBarList[i]);

                        double left = random.Next(num, (int)_bgCanvas.Width - num);
                        double top = random.Next(num, (int)_bgCanvas.Height - num);
                        Canvas.SetLeft(vList[i], left);
                        Canvas.SetTop(vList[i], top);

                        left = (int)_bgCanvas.Width / left;
                        top = (int)_bgCanvas.Height / top;
                        Canvas.SetLeft(vMapList[i], _mapCanvas.Width / left);
                        Canvas.SetTop(vMapList[i], _mapCanvas.Height / top);

                        Panel.SetZIndex(vList[i], 1);
                        Panel.SetZIndex(vMapList[i], 1);
                        Panel.SetZIndex(vProgressBarList[i], 1);

                        vList.Remove(vList[i]);
                        vProgressBarList.Remove(vProgressBarList[i]);
                        vMapList.Remove(vMapList[i]);
                        gcTargetList.Remove(gcTargetList[i]);
                        gcTargetList.Add(random.Next(gcList.Count));
                    }
                }
            }
            CheckWinner();
        }
        private void FindGoodCells(object sender, EventArgs e) {
           
            if (vIndex >= vList.Count) {
                vIndex = 0;
            }
            if (gcList.Count == 1) {
               
                vIndex = 0;
            }
            if (gcList.Count == 0) {
                gcTargetList.Clear();
            }
            if (vList.Count != 0 && gcTargetList.Count != 0) {
                // rotate virus
                vList[vIndex].RenderTransformOrigin = new Point(0.5, 0.5);
                vList[vIndex].RenderTransform = new RotateTransform(angleRotate);
                
                if (gcTargetList[vIndex] < gcList.Count && gcList.Count != 0 && vList.Count != 0) {
                    VirusAutoMove();
                    KillGoodCells();
                }
                else {
                    gcTargetList[vIndex]--;
                }
            }
            vIndex++;
            PlayerRotation();
        }
        private void ChangedTitleOpacity(object sender, EventArgs e) {
            if(_titleLabel.Opacity <= 0) {
                _titleLabel.Opacity = 1;
            }
            _titleLabel.Opacity -= opacity;
        }
        private void CheckWinner() {
            bool stop = false;
            if (gcList.Count == 0) {
                killCellsTimer.Stop();
                stop = true;
            }
            else if (vList.Count == 0) {
                killCellsTimer.Stop();
                stop = true;
            }
            if(stop) {
                _playAgainButton.Visibility = Visibility.Visible;
                _playAgainButton.IsEnabled = true;
            }
        }
        private void VirusAutoMove() {
            if (Canvas.GetLeft(vList[vIndex]) > Canvas.GetLeft(gcList[gcTargetList[vIndex]])) {
                Canvas.SetLeft(vList[vIndex], Canvas.GetLeft(vList[vIndex]) - virusSpeed);
                Canvas.SetLeft(vMapList[vIndex], Canvas.GetLeft(vMapList[vIndex]) - virusSpeed / ratio);
            }
            else if (Canvas.GetLeft(vList[vIndex]) < Canvas.GetLeft(gcList[gcTargetList[vIndex]])) {
                Canvas.SetLeft(vList[vIndex], Canvas.GetLeft(vList[vIndex]) + virusSpeed);
                Canvas.SetLeft(vMapList[vIndex], Canvas.GetLeft(vMapList[vIndex]) + virusSpeed / ratio);
            }
            if (Canvas.GetTop(vList[vIndex]) < Canvas.GetTop(gcList[gcTargetList[vIndex]])) {
                Canvas.SetTop(vList[vIndex], Canvas.GetTop(vList[vIndex]) + virusSpeed);
                Canvas.SetTop(vMapList[vIndex], Canvas.GetTop(vMapList[vIndex]) + virusSpeed / ratio);
            }
            else if (Canvas.GetTop(vList[vIndex]) > Canvas.GetTop(gcList[gcTargetList[vIndex]])) {
                Canvas.SetTop(vList[vIndex], Canvas.GetTop(vList[vIndex]) - virusSpeed);
                Canvas.SetTop(vMapList[vIndex], Canvas.GetTop(vMapList[vIndex]) - virusSpeed / ratio);
            }
            Canvas.SetLeft(vProgressBarList[vIndex], Canvas.GetLeft(vList[vIndex]));
            Canvas.SetTop(vProgressBarList[vIndex], Canvas.GetTop(vList[vIndex]) + vList[vIndex].Height / 2);
            Canvas.SetLeft(vProgressBarList[vIndex], Canvas.GetLeft(vList[vIndex]) + vList[vIndex].Width / 4);
        }
        private void KillGoodCells() {
            if (Intersect(vList[vIndex], gcList[gcTargetList[vIndex]])) {
                gcProgressBarList[gcTargetList[vIndex]].Value -= virusDamage;
                if (gcProgressBarList[gcTargetList[vIndex]].Value == 0) {

                    // infected cells turn into virus
                    gcList[gcTargetList[vIndex]].Fill = new ImageBrush(new BitmapImage(new Uri(imagePath[random.Next(imagePath.Count)], UriKind.Relative)));
                    gcList[gcTargetList[vIndex]].Stroke = Brushes.Red;
                    gcList[gcTargetList[vIndex]].Stretch = Stretch.Fill;
                    gcProgressBarList[gcTargetList[vIndex]].Value = 100;
                    gcMapList[gcTargetList[vIndex]].Fill = Brushes.Red;

                    // add to virus list
                    vList.Add(gcList[gcTargetList[vIndex]]);
                    vMapList.Add(gcMapList[gcTargetList[vIndex]]);
                    vProgressBarList.Add(gcProgressBarList[gcTargetList[vIndex]]);

                    // set zindex
                    Panel.SetZIndex(gcList[gcTargetList[vIndex]], 2);
                    Panel.SetZIndex(gcMapList[gcTargetList[vIndex]], 2);
                    Panel.SetZIndex(gcProgressBarList[gcTargetList[vIndex]], 2);

                    // remote infected cells from list
                    gcList.Remove(gcList[gcTargetList[vIndex]]);
                    gcMapList.Remove(gcMapList[gcTargetList[vIndex]]);
                    gcProgressBarList.Remove(gcProgressBarList[gcTargetList[vIndex]]);
                    gcTargetList.Add(random.Next(gcList.Count));

                    //if (vList.Count < 20) {
                    //    time = 0.06;
                    //}
                    //else if (vList.Count < 30 && vList.Count > 20) {
                    //    time = 0.05;
                    //}
                    //else if (vList.Count < 50 && vList.Count > 30) {
                    //    time = 0.04;
                    //}
                    //else {
                    //    time = 0.03;
                    //}
                    //killCellsTimer.Interval = TimeSpan.FromMilliseconds(time);
                }
            }
        }
        private void AddCoronaVirus() {
            for (int i = 0; i < numOfVirus; i++) {
                Ellipse virus = new Ellipse();
                virus.Width = 50;
                virus.Height = 50;
                virus.Stroke = Brushes.Red;
                virus.Fill = new ImageBrush(new BitmapImage(new Uri("../../Images/v3.png", UriKind.Relative)));
                virus.Stretch = Stretch.Uniform;
                double left = random.Next(num, (int)_bgCanvas.Width - num) ;
                double top = random.Next(num, (int)_bgCanvas.Height - num);
                Canvas.SetLeft(virus, left  );
                Canvas.SetTop(virus, top  );
                _bgCanvas.Children.Add(virus);
                vList.Add(virus);
                Panel.SetZIndex(virus, 2);

                ProgressBar virusBar = new ProgressBar();
                virusBar.Minimum = 0;
                virusBar.Maximum = 100;
                virusBar.Value = 100;
                virusBar.Width = 30;
                virusBar.Height = 3;
                _bgCanvas.Children.Add(virusBar);
                vProgressBarList.Add(virusBar);
                Panel.SetZIndex(virusBar, 2);
                virusBar.Visibility = Visibility.Hidden;

                Ellipse virusMap = new Ellipse();
                virusMap.Width = 30 / 5;
                virusMap.Height = 30 / 5;
                virusMap.Fill = Brushes.Red;

                left = (int)_bgCanvas.Width / left;
                top =  (int)_bgCanvas.Height / top;
                Canvas.SetLeft(virusMap, _mapCanvas.Width / left);
                Canvas.SetTop(virusMap, _mapCanvas.Height / top );
                _mapCanvas.Children.Add(virusMap);
                vMapList.Add(virusMap);
                Panel.SetZIndex(virusMap, 2);
            }
        }
        private void SetTargetCells() {
            for (int i = 0; i < vList.Count; i++) {
                int index = random.Next(gcList.Count);
                if (gcTargetList.Contains(index)) {
                    i--;
                }
                else {
                    gcTargetList.Add(index);
                }
            }
        }
        private void AddGoodCells() {
            for (int i = 0; i < numOfCells; i++) {
                Ellipse goodCells = new Ellipse();
                goodCells.Width = 50;
                goodCells.Height = 50;
                goodCells.Stroke = Brushes.White;
                goodCells.Fill = new ImageBrush(new BitmapImage(new Uri("../../Images/cells.png", UriKind.Relative)));
                goodCells.Stretch = Stretch.Uniform;
                double left = random.Next(num, (int)_bgCanvas.Width - num);
                double top = random.Next(num, (int)_bgCanvas.Height - num);
                Canvas.SetLeft(goodCells, left);
                Canvas.SetTop(goodCells, top);
               
                goodCells.RenderTransformOrigin = new Point(.5, .5);
                goodCells.RenderTransform = new RotateTransform(random.Next(360));
                _bgCanvas.Children.Add(goodCells);
                gcList.Add(goodCells);
                Panel.SetZIndex(goodCells, 1);

                ProgressBar goodCellsBar = new ProgressBar();
                goodCellsBar.Minimum = 0;
                goodCellsBar.Maximum = 100;
                goodCellsBar.Value = 100;
                goodCellsBar.Width = 30;
                goodCellsBar.Height = 3;
                Canvas.SetLeft(goodCellsBar, Canvas.GetLeft(goodCells) + goodCellsBar.Width / 2);
                Canvas.SetTop(goodCellsBar, Canvas.GetTop(goodCells) + goodCellsBar.Height / 2);
                _bgCanvas.Children.Add(goodCellsBar);
                gcProgressBarList.Add(goodCellsBar);
                Panel.SetZIndex(goodCellsBar, 3);
                goodCellsBar.Visibility = Visibility.Hidden;

                Ellipse goodCellsMap = new Ellipse();
                goodCellsMap.Width = 30 / 5;
                goodCellsMap.Height = 30 / 5;
                goodCellsMap.Fill = Brushes.White;
                left = (int)_bgCanvas.Width / left;
                top = (int)_bgCanvas.Height / top;
                Canvas.SetLeft(goodCellsMap, _mapCanvas.Width / left);
                Canvas.SetTop(goodCellsMap, _mapCanvas.Height / top);
                _mapCanvas.Children.Add(goodCellsMap);
                gcMapList.Add(goodCellsMap);
                Panel.SetZIndex(goodCellsMap, 1);
            }
        }

        private void PlayerMove() {
            if (moveRight && !Intersect(_rightWall, player)) {
                if(Canvas.GetLeft(player) >= _window.Width - player.Width * 2 + 20) {
                    Canvas.SetLeft(_rightWall, Canvas.GetLeft(_rightWall) - playerSpeed);
                    Canvas.SetLeft(_leftWall, Canvas.GetLeft(_leftWall) - playerSpeed);
                    Canvas.SetLeft(_bgCanvas, Canvas.GetLeft(_bgCanvas) - playerSpeed);
                    Canvas.SetLeft(playerMap, Canvas.GetLeft(playerMap) + playerSpeed / ratio);
                    Canvas.SetLeft(playerShadow, Canvas.GetLeft(playerShadow) + playerSpeed);
                }
                else {
                    Canvas.SetLeft(player, Canvas.GetLeft(player) + playerSpeed);
                    Canvas.SetLeft(playerShadow, Canvas.GetLeft(playerShadow) + playerSpeed);
                    Canvas.SetLeft(playerMap, Canvas.GetLeft(playerMap) + playerSpeed / ratio);
                }
            }
            if (moveLeft && !Intersect(_leftWall, player)) {
                if (Canvas.GetLeft(player) <= player.Width - 20) {
                    Canvas.SetLeft(_leftWall, Canvas.GetLeft(_leftWall) + playerSpeed);
                    Canvas.SetLeft(_rightWall, Canvas.GetLeft(_rightWall) + playerSpeed);
                    Canvas.SetLeft(_bgCanvas, Canvas.GetLeft(_bgCanvas) + playerSpeed);
                    Canvas.SetLeft(playerMap, Canvas.GetLeft(playerMap) - playerSpeed / ratio);
                    Canvas.SetLeft(playerShadow, Canvas.GetLeft(playerShadow) - playerSpeed);
                }
                else {
                    Canvas.SetLeft(player, Canvas.GetLeft(player) - playerSpeed);
                    Canvas.SetLeft(playerShadow, Canvas.GetLeft(playerShadow) - playerSpeed);
                    Canvas.SetLeft(playerMap, Canvas.GetLeft(playerMap) - playerSpeed / ratio);
                }
            }
            if(moveUp && !Intersect(_topWall, player)) {
                if(Canvas.GetTop(player) <= player.Width - 20) {
                    Canvas.SetTop(_topWall, Canvas.GetTop(_topWall) + playerSpeed);
                    Canvas.SetTop(_btmWall, Canvas.GetTop(_btmWall) + playerSpeed);
                    Canvas.SetTop(_bgCanvas, Canvas.GetTop(_bgCanvas) + playerSpeed);
                    Canvas.SetTop(playerMap, Canvas.GetTop(playerMap) - playerSpeed / ratio);
                    Canvas.SetTop(playerShadow, Canvas.GetTop(playerShadow) - playerSpeed);
                }
                else {
                    Canvas.SetTop(player, Canvas.GetTop(player) - playerSpeed);
                    Canvas.SetTop(playerShadow, Canvas.GetTop(playerShadow) - playerSpeed);
                    Canvas.SetTop(playerMap, Canvas.GetTop(playerMap) - playerSpeed / ratio);
                }
            }
            if (moveDown && !Intersect(_btmWall, player)) {
                if (Canvas.GetTop(player) >= _window.Height - player.Height * 2 + 20) {
                    Canvas.SetTop(_btmWall, Canvas.GetTop(_btmWall) - playerSpeed);
                    Canvas.SetTop(_topWall, Canvas.GetTop(_topWall) - playerSpeed);
                    Canvas.SetTop(_bgCanvas, Canvas.GetTop(_bgCanvas) - playerSpeed);
                    Canvas.SetTop(playerMap, Canvas.GetTop(playerMap) + playerSpeed / ratio);
                    Canvas.SetTop(playerShadow, Canvas.GetTop(playerShadow) + playerSpeed);
                }
                else {
                    Canvas.SetTop(player, Canvas.GetTop(player) + playerSpeed);
                    Canvas.SetTop(playerShadow, Canvas.GetTop(playerShadow) + playerSpeed);
                    Canvas.SetTop(playerMap, Canvas.GetTop(playerMap) + playerSpeed / ratio);
                }
            }
        }
        private void PlayerRotation() {
            angleRotate -= 1;
            if (angleRotate <= -360) {
                angleRotate = -1;
            }
            player.RenderTransform = new RotateTransform(angleRotate);
        }
        private void PlayerUI() {
            // player on map
            playerMap.Width = 50 / 5;
            playerMap.Height = 50 / 5;
            playerMap.Fill = Brushes.Green;

            Canvas.SetLeft(playerMap, _mapCanvas.Width / 2 - playerMap.Width / 2);
            Canvas.SetTop(playerMap, _mapCanvas.Height / 2 - playerMap.Height / 2);
            _mapCanvas.Children.Add(playerMap);
            Panel.SetZIndex(playerMap, 3);

            // player shadow
            playerShadow.Width = 50;
            playerShadow.Height = 50;
            Canvas.SetLeft(playerShadow, _bgCanvas.Width / 2  - playerShadow.Width / 2);
            Canvas.SetTop(playerShadow,  _bgCanvas.Height / 2 - playerShadow.Height / 2);
            _bgCanvas.Children.Add(playerShadow);

            // player

            player.Width = 50;
            player.Height = 50;
            player.Stroke = Brushes.Green;
            player.RenderTransformOrigin = new Point(.5, .5);
            player.Fill = new ImageBrush {
                ImageSource = new BitmapImage(new Uri("../../Images/capsule.png", UriKind.Relative)),
                Stretch = Stretch.Fill
            };
            Canvas.SetLeft(player, _window.Width / 2 - player.Width / 2);
            Canvas.SetTop(player, _window.Height / 2 - player.Height / 2);
            _mainCanvas.Children.Add(player);
        }

        private bool Intersect(Rectangle walls, Ellipse player) {
            Rect rect1 = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.ActualWidth, player.ActualHeight);
            Rect rect2 = new Rect(Canvas.GetLeft(walls), Canvas.GetTop(walls), walls.ActualWidth, walls.ActualHeight);
            if (rect1.IntersectsWith(rect2)) {
                return true;
            }
            return false;
        }
        private bool Intersect(Ellipse c1, Ellipse c2) {
            Rect rect1 = new Rect(Canvas.GetLeft(c2), Canvas.GetTop(c2), c2.ActualWidth, c2.ActualHeight);
            Rect rect2 = new Rect(Canvas.GetLeft(c1), Canvas.GetTop(c1), c1.ActualWidth, c1.ActualHeight);
            if (rect1.IntersectsWith(rect2)) {
                return true;
            }
            return false;
        }
        private void WindowPressKey_PreviewKeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.D) {
                moveRight = true;
            }
            if(e.Key == Key.A) {
                moveLeft = true;
            }
            if(e.Key == Key.W) {
                moveUp = true;
            }
            if(e.Key == Key.S) {
                moveDown = true;
            }
            PlayerMove();
        }
        private void WindowPressKey_PreviewKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.D) {
                moveRight = false;
            }
            if (e.Key == Key.A) {
                moveLeft = false;
            }
            if (e.Key == Key.W) {
                moveUp = false;
            }
            if (e.Key == Key.S) {
                moveDown = false;
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e) {
            killCellsTimer.Start();
        }
        private void PauseButton_Click(object sender, RoutedEventArgs e) {
            killCellsTimer.Stop();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        private void PlayAgainButton_Click(object sender, RoutedEventArgs e) {
            _playAgainButton.Visibility = Visibility.Hidden;
            _playAgainButton.IsEnabled = false;
            MainWindow mainWindow = new MainWindow();
            this.Close();
            mainWindow.ShowDialog();
        }
    }
}
