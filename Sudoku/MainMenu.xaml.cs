using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Sudoku
{
    /// <summary>
    /// Logika interakcji dla klasy MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Window
    {
        public static MainMenu instance;
        public int boardSize = 0;
        public int difficultyLevel = 0;
        public string fileContent = string.Empty;
        public MainMenu()
        {
            InitializeComponent();
            instance = this;
            InitializeAllProperties();
        }

        private void InitializeAllProperties()
        {
            SetBoardSize(3);
            SetDifficultySliderMaximum();
        }
        private void HandlePlayButtonClick(object sender, RoutedEventArgs e)
        {
            MainWindow game = new MainWindow();
            Trace.WriteLine("boardSize: " + boardSize + " DifficultyLevel: " + difficultyLevel);
            Close();
            game.Show();
        }
        private void HandleMinusButtonClick(object sender, RoutedEventArgs e)
        {
            SetBoardSize(-1);
            SetDifficultySliderMaximum();
        }
        private void HandlePlusButtonClick(object sender, RoutedEventArgs e)
        {
            SetBoardSize(1);
            SetDifficultySliderMaximum();
        }
        private void SetBoardSize(int IncreaseOrSubtractBy)
        {
            if (boardSize > 1 || IncreaseOrSubtractBy > -1)
            {
                boardSize = boardSize + IncreaseOrSubtractBy;
                int boardSizeBySmallBoxes = boardSize * boardSize;
                boardSizeTextBox.Text = boardSizeBySmallBoxes.ToString() + " x " + boardSizeBySmallBoxes.ToString();
            }
        }
        private void SetDifficultySliderMaximum()
        {
            difficultySlider.Maximum = boardSize * boardSize * boardSize * boardSize;
        }
        private void SetDifficultyLevel(int value)
        {
            difficultyLevel = value;
        }

        private void HandledifficultySliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetDifficultyLevel((int)difficultySlider.Value);
        }

        private void HandleLoadFileButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Pliki zapisu sudoku (*.sudokuSave)|*.sudokuSave";
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                fileContent = ReadFile(filePath);
                MainWindow game = new MainWindow();
                Close();
                game.Show();
            }
            else
            {
                fileContent = string.Empty;
            }
            string ReadFile(string filePath)
            {
                try
                {
                    return System.IO.File.ReadAllText(filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Opening File Failed!");
                    return null;
                }
            }
        }
    }
}
