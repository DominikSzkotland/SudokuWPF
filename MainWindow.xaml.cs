using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sudoku
{
    public partial class MainWindow : Window
    {
        public static MainWindow instance;
        public int numberOfBoxesInOneRow = MainMenu.instance.boardSize * MainMenu.instance.boardSize;
        public int numberOfPreBuildedBoxes = MainMenu.instance.difficultyLevel;
        public string readedFileContent = MainMenu.instance.fileContent;
        int numberOfAreasInOneRow;
        private List<TextBox> allBoxes = new List<TextBox>();

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += AskForSave;
            instance = this;
            if (readedFileContent != string.Empty)
            {
                RunGameFromFile();
            }
            else if (CheckNumberOfBoxesInOneRowValidation(numberOfBoxesInOneRow))
            {
                numberOfAreasInOneRow = int.Parse(Math.Sqrt(numberOfBoxesInOneRow).ToString());
                FillBoardWithBoxes(numberOfBoxesInOneRow);
                FillSudoku();
                ApplyDifficultyLevel();
                for (int i = 0; i < allBoxes.Count; i++)
                {
                    allBoxes[i].TextChanged += CheckIfYouWon;
                }
            }
            else
            {
                Close();
            }
        }
        private void AskForSave(object sender, CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Save game to file before exit?", "Save confirmation", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if (!SaveGameToFile())
                {
                    e.Cancel = true;
                }
                else
                {
                    MainMenu mainMenu = new MainMenu();
                    mainMenu.Show();
                }
            }
            if (result == MessageBoxResult.No)
            {
                MainMenu mainMenu = new MainMenu();
                mainMenu.Show();
            }
            if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }

        }
        void RunGameFromFile()
        {

            string separator = "-|-";
            string[] allData = readedFileContent.Split(separator);
            for (int i = 0; i < allData.Length; i++)
            {
                if (i == 0)
                {
                    try
                    {
                        numberOfBoxesInOneRow = int.Parse(allData[i]);
                        numberOfAreasInOneRow = int.Parse(Math.Sqrt(numberOfBoxesInOneRow).ToString());
                        FillBoardWithBoxes(numberOfBoxesInOneRow);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Wrong file selected or file is corrupted");
                        break;
                    }
                }
                else
                {
                    if (allData[i].StartsWith("R"))
                    {
                        allBoxes[i - 1].Text = allData[i].Remove(0, 1);
                        allBoxes[i - 1].IsReadOnly = true;
                        allBoxes[i - 1].IsEnabled = false;
                    }
                    else
                    {
                        if (allData[i] == "0")
                        {
                            allBoxes[i - 1].Text = string.Empty;
                        }
                        else
                        {
                            allBoxes[i - 1].Text = allData[i];
                        }
                    }
                }

            }
        }
        bool SaveGameToFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Pliki zapisu sudoku (*.sudokuSave)|*.sudokuSave";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.FileName = "board.sudokuSave";
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                string content = getContentToSave();
                WriteToFile(filePath, content);
                return true;
            }
            else
            {
                return false;
            }
            void WriteToFile(string filePath, string content)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        writer.Write(content);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Save Failed!");
                }
            }
            string getContentToSave()
            {
                string content = string.Empty;
                content += numberOfBoxesInOneRow.ToString();
                for (int i = 0; i < allBoxes.Count; i++)
                {
                    content += "-|-";
                    if (allBoxes[i].IsReadOnly == true)
                    {
                        content += "R";
                    }
                    if (allBoxes[i].Text == string.Empty)
                    {
                        content += "0";
                    }
                    else
                    {
                        content += allBoxes[i].Text;
                    }
                }
                return content;
            }
        }
        void FillSudoku()
        {
            int[,] allValues = new int[numberOfBoxesInOneRow, numberOfBoxesInOneRow];
            SolveSudoku(allValues, numberOfBoxesInOneRow);
            PrintBoard(allValues, numberOfBoxesInOneRow);
            bool SolveSudoku(int[,] board, int SizeOfSudoku)
            {
                int row = 0, col = 0;

                if (!FindMostConstrainedCell(board, SizeOfSudoku, ref row, ref col))
                {
                    return true;
                }

                Random rand = new Random();
                int[] numbers = GenerateRandomSequence(SizeOfSudoku, rand);

                foreach (int num in numbers)
                {
                    if (IsSafe(board, SizeOfSudoku, row, col, num))
                    {
                        board[row, col] = num;

                        if (SolveSudoku(board, SizeOfSudoku))
                        {
                            return true;
                        }

                        board[row, col] = 0;
                    }
                }

                return false;
            }
            int[] GenerateRandomSequence(int SizeOfSudoku, Random rand)
            {
                int[] sequence = new int[SizeOfSudoku];
                for (int i = 0; i < SizeOfSudoku; i++)
                {
                    sequence[i] = i + 1;
                }

                
                for (int i = 0; i < SizeOfSudoku; i++)
                {
                    int j = rand.Next(i, SizeOfSudoku);
                    int temp = sequence[i];
                    sequence[i] = sequence[j];
                    sequence[j] = temp;
                }

                return sequence;
            }
            bool FindMostConstrainedCell(int[,] board, int SizeOfSudoku, ref int row, ref int col)
            {
                int minOptions = SizeOfSudoku + 1;
                for (int r = 0; r < SizeOfSudoku; r++)
                {
                    for (int c = 0; c < SizeOfSudoku; c++)
                    {
                        if (board[r, c] == 0)
                        {
                            int options = CountOptions(board, SizeOfSudoku, r, c);
                            if (options < minOptions)
                            {
                                minOptions = options;
                                row = r;
                                col = c;
                            }
                        }
                    }
                }
                return minOptions != SizeOfSudoku + 1;
            }
            int CountOptions(int[,] board, int SizeOfSudoku, int row, int col)
            {
                bool[] used = new bool[SizeOfSudoku + 1];
                int boxSize = (int)Math.Sqrt(SizeOfSudoku);

                for (int i = 0; i < SizeOfSudoku; i++)
                {
                    if (board[row, i] != 0) used[board[row, i]] = true;
                    if (board[i, col] != 0) used[board[i, col]] = true;
                }

                int boxStartRow = row - row % boxSize;
                int boxStartCol = col - col % boxSize;
                for (int r = 0; r < boxSize; r++)
                {
                    for (int c = 0; c < boxSize; c++)
                    {
                        if (board[r + boxStartRow, c + boxStartCol] != 0)
                        {
                            used[board[r + boxStartRow, c + boxStartCol]] = true;
                        }
                    }
                }

                int options = 0;
                for (int i = 1; i <= SizeOfSudoku; i++)
                {
                    if (!used[i]) options++;
                }
                return options;
            }
            bool IsSafe(int[,] board, int SizeOfSudoku, int row, int col, int num)
            {
                int boxSize = (int)Math.Sqrt(SizeOfSudoku);
                return !UsedInRow(board, SizeOfSudoku, row, num) &&
                       !UsedInCol(board, SizeOfSudoku, col, num) &&
                       !UsedInBox(board, boxSize, row - row % boxSize, col - col % boxSize, num);
            }
            bool UsedInRow(int[,] board, int SizeOfSudoku, int row, int num)
            {
                for (int col = 0; col < SizeOfSudoku; col++)
                {
                    if (board[row, col] == num)
                    {
                        return true;
                    }
                }
                return false;
            }
            bool UsedInCol(int[,] board, int SizeOfSudoku, int col, int num)
            {
                for (int row = 0; row < SizeOfSudoku; row++)
                {
                    if (board[row, col] == num)
                    {
                        return true;
                    }
                }
                return false;
            }
            bool UsedInBox(int[,] board, int boxSize, int boxStartRow, int boxStartCol, int num)
            {
                for (int row = 0; row < boxSize; row++)
                {
                    for (int col = 0; col < boxSize; col++)
                    {
                        if (board[row + boxStartRow, col + boxStartCol] == num)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            void PrintBoard(int[,] board, int SizeOfSudoku)
            {
                for (int row = 0; row < SizeOfSudoku; row++)
                {
                    for (int col = 0; col < SizeOfSudoku; col++)
                    {
                        allBoxes[row * numberOfBoxesInOneRow + col].Text = board[row, col].ToString();
                    }
                }
            }
        }
        void FillBoardWithBoxes(int numberOfBoxesInOneRow)
        {
            double screenHeight = System.Windows.SystemParameters.WorkArea.Height;
            double screenWidth = System.Windows.SystemParameters.WorkArea.Width;
            if (screenHeight < this.Height || screenWidth < this.Width)
            {
                if(screenHeight < screenWidth)
                {
                    this.Height = screenHeight;
                    this.Width = screenHeight;
                }
                else
                {
                    this.Width = screenWidth;
                    this.Height = screenWidth;
                }
            }
            for (int i = 0; i < numberOfBoxesInOneRow; i++)
            {
                board.RowDefinitions.Add(new RowDefinition());
                board.ColumnDefinitions.Add(new ColumnDefinition());
            }
            int numberOfAreasInOneRow = int.Parse(Math.Sqrt(numberOfBoxesInOneRow).ToString());
            int rowIndexer = 0;
            int columnIndexer = 0;
            double fatLineThickness = 2;
            for (int i = 0; i < numberOfBoxesInOneRow * numberOfBoxesInOneRow;)
            {
                double cellTopThickness = 1;
                double cellRightThickness = 1;
                TextBox cell = new TextBox();
                cell.HorizontalContentAlignment = HorizontalAlignment.Center;
                cell.VerticalContentAlignment = VerticalAlignment.Center;
                cell.HorizontalAlignment = HorizontalAlignment.Stretch;
                cell.VerticalAlignment = VerticalAlignment.Stretch;
                cell.BorderBrush = new SolidColorBrush(Colors.Black);
                cell.BorderThickness = new Thickness(1);
                double fontSize = this.Height / (numberOfBoxesInOneRow * 2);
                if (fontSize < 8)
                {
                    fontSize = 8;
                }
                cell.FontSize = fontSize;
                Grid.SetRow(cell, rowIndexer);
                Grid.SetColumn(cell, columnIndexer);
                i++;
                if (i % numberOfAreasInOneRow == 0 && i % numberOfBoxesInOneRow != 0)
                {
                    cellRightThickness = fatLineThickness;
                }
                if (rowIndexer % numberOfAreasInOneRow == 0 && rowIndexer != 0)
                {
                    cellTopThickness = fatLineThickness;
                }
                cell.BorderThickness = new Thickness(1, cellTopThickness, cellRightThickness, 1);
                if (i % numberOfBoxesInOneRow == 0)
                {
                    rowIndexer++;
                    columnIndexer = 0;
                }
                else
                {
                    columnIndexer++;
                }
                allBoxes.Add(cell);
                board.Children.Add(cell);
            }
        }
        void ApplyDifficultyLevel()
        {
            int numberOfSolvedBoxes = allBoxes.Count - numberOfPreBuildedBoxes;
            Trace.WriteLine("Solved: " + numberOfSolvedBoxes + " All: " + allBoxes.Count + " Pre: " + numberOfPreBuildedBoxes);
            Random random = new Random();
            List<int> erasedIndexes = new List<int>();
            List<int> availableIndexes = new List<int>();
            for (int i = 0; i < allBoxes.Count; i++)
            {
                availableIndexes.Add(i);
            }
            for (int i = 0; i < numberOfSolvedBoxes; i++)
            {
                int drawedNumber = random.Next(0, availableIndexes.Count);
                int indexToErase = availableIndexes[drawedNumber];
                erasedIndexes.Add(indexToErase);
                availableIndexes.RemoveAt(drawedNumber);
            }
            for (int i = 0; i < erasedIndexes.Count; i++)
            {
                int indexToErase = erasedIndexes[i];
                allBoxes[indexToErase].Text = string.Empty;
            }
            for (int i = 0; i < allBoxes.Count; i++)
            {
                if (allBoxes[i].Text != string.Empty)
                {
                    allBoxes[i].IsReadOnly = true;
                    allBoxes[i].IsEnabled = false;
                }
            }
        }

        private bool CheckNumberOfBoxesInOneRowValidation(int numberOfBoxesInOneRow)
        {
            int nextNaturalNumbers = 0;
            while (true)
            {
                if (nextNaturalNumbers * nextNaturalNumbers > numberOfBoxesInOneRow)
                {
                    return false;
                }
                else if (nextNaturalNumbers * nextNaturalNumbers == numberOfBoxesInOneRow)
                {
                    return true;
                }
                else
                {
                    nextNaturalNumbers++;
                }
            }
        }
        private void CheckIfYouWon(object sender, TextChangedEventArgs e)
        {
            if (IsBoardOK() && !CheckIfBoardContainsEmptyBoxes())
            {

                if (numberOfPreBuildedBoxes > 1)
                {
                    MessageBox.Show("You Won!\n" + numberOfBoxesInOneRow + "x" + numberOfBoxesInOneRow + " board.\nStarted with " + numberOfPreBuildedBoxes + "/" + allBoxes.Count + " solved boxes");
                }
                else if (numberOfPreBuildedBoxes == 1)
                {
                    MessageBox.Show("You Won!\n" + numberOfBoxesInOneRow + "x" + numberOfBoxesInOneRow + " board.\nStarted with " + numberOfPreBuildedBoxes + "/" + allBoxes.Count + " solved box");
                }
                else
                {
                    MessageBox.Show("You Won!\n" + numberOfBoxesInOneRow + "x" + numberOfBoxesInOneRow + " board.\nStarted without solved boxes");
                }
            }
        }
        private bool IsBoardOK()
        {
            if (IsColumnsOk() && IsRowsOk() && IsAreasOk())
            {
                return true;
            }
            return false;
        }
        private bool IsColumnsOk()
        {

            for (int columnIndexer = 0; columnIndexer < numberOfBoxesInOneRow; columnIndexer++)
            {
                List<string> listOfColumnBoxes = new List<string>();
                for (int i = columnIndexer; i < allBoxes.Count; i += numberOfBoxesInOneRow)
                {
                    listOfColumnBoxes.Add(allBoxes[i].Text);
                }
                if (CheckIfAnyElementInListIsRepeated(listOfColumnBoxes) || CheckIfAnyElementInListIsGreaterOrLessThanAllowed(listOfColumnBoxes))
                {
                    return false;
                }
            }
            return true;
        }
        private bool IsRowsOk()
        {
            for (int rowIndexer = 0; rowIndexer < numberOfBoxesInOneRow; rowIndexer++)
            {
                List<string> listOfRowBoxes = new List<string>();
                for (int i = rowIndexer * numberOfBoxesInOneRow; i < (rowIndexer * numberOfBoxesInOneRow) + numberOfBoxesInOneRow; i++)
                {
                    listOfRowBoxes.Add(allBoxes[i].Text);
                }
                if (CheckIfAnyElementInListIsRepeated(listOfRowBoxes) || CheckIfAnyElementInListIsGreaterOrLessThanAllowed(listOfRowBoxes))
                {
                    return false;
                }
            }
            return true;
        }
        private bool IsAreasOk()
        {
            for (int columnIndexer = 0; columnIndexer < numberOfBoxesInOneRow; columnIndexer += numberOfAreasInOneRow)
            {
                for (int rowIndexer = 0; rowIndexer < numberOfBoxesInOneRow; rowIndexer += numberOfAreasInOneRow)
                {
                    List<string> listOfAreaBoxes = new List<string>();
                    int x = 0;
                    for (int i = rowIndexer * numberOfBoxesInOneRow + columnIndexer; ;)
                    {
                        x++;
                        if (x == numberOfBoxesInOneRow + 1)
                        {
                            break;
                        }
                        listOfAreaBoxes.Add(allBoxes[i].Text);
                        if (x % numberOfAreasInOneRow == 0)
                        {
                            i += (numberOfBoxesInOneRow - (numberOfAreasInOneRow)) + 1;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    if (CheckIfAnyElementInListIsRepeated(listOfAreaBoxes) || CheckIfAnyElementInListIsGreaterOrLessThanAllowed(listOfAreaBoxes))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private bool CheckIfAnyElementInListIsGreaterOrLessThanAllowed(List<string> listToCheck)
        {
            listToCheck.RemoveAll(string.IsNullOrEmpty);
            for (int i = 0; i < listToCheck.Count; i++)
            {
                if (int.TryParse(listToCheck[i], out int parsedValue))
                {
                    if (parsedValue < 1 || parsedValue > numberOfBoxesInOneRow)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private bool CheckIfAnyElementInListIsRepeated(List<string> listToCheck)
        {
            List<string> nonEmptyElements = listToCheck.Where(element => !string.IsNullOrEmpty(element)).ToList();
            HashSet<string> uniqueNumbers = new HashSet<string>(nonEmptyElements);
            if (nonEmptyElements.Count - uniqueNumbers.Count != 0)
            {
                return true;
            }
            return false;
        }
        private bool CheckIfBoardContainsEmptyBoxes()
        {
            for (int i = 0; i < allBoxes.Count; i++)
            {
                if (string.IsNullOrEmpty(allBoxes[i].Text))
                {
                    return true;
                }
            }
            return false;
        }
    }
}