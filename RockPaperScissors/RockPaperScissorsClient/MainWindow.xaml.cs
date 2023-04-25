/*
 *  Program:        RockPaperScissorsClient.exe 
 *  Module:         MainWindow.xaml.cs
 *  Author(s):      Ali, Dylan, Mahan, Manh
 *  Date:           March 18, 2021
 *  Description:    A WCF client for the RockPaperScissors service defined in RockPaperScissorsLibrary.dll. Allows the 
 *                  player to establish an alias, then lets them play rock paper scissors against other players on the LAN network.
 *                  The players score are displayed in the client's GUI.
 */

using System;
using System.Windows;

using System.ServiceModel;  // WCF

using RockPaperScissorsLibrary;

namespace RockPaperScissorsClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public partial class MainWindow : Window, ICallback
    {
        private IRockPaperScissors rps = null;
        private string PlayerName = "";
        private string PlayerHand = "";
        private int RoundNumber = 1;

        public MainWindow()
        {
            InitializeComponent();
            lobbyCanvas.Visibility = Visibility.Visible;
            gameCanvas.Visibility = Visibility.Hidden;
            finalScoresCanvas.Visibility = Visibility.Hidden;
            hostDisconnectedCanvas.Visibility = Visibility.Hidden;
            try
            {
                // Configure the ABCs of using the RockPaperScissors service
                DuplexChannelFactory<IRockPaperScissors> channel = new DuplexChannelFactory<IRockPaperScissors>(this, "GameService");

                // Activate a RockPaperScissors object
                rps = channel.CreateChannel();

                if (!rps.IsHost())
                {
                    scoreLabel.Visibility = Visibility.Hidden;
                    scoreTextBox.Visibility = Visibility.Hidden;
                    startButton.Visibility = Visibility.Hidden;
                    backToLobbyButton.Visibility = Visibility.Hidden;
                }
                else
                {
                    inputLabel.Content = "Set an alias and a score to play up to!";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Helper methods

        private bool connectToRockPaperScissors()
        {
            try
            {
                // If the user that joined is the host set the game score

                // Add GUI components to get alias then update GUI components on join(score board and buttons)
                if (setName.Text.Length > 0 && rps.Join(setName.Text))
                {
                    // Alias accepted by the service so update GUI
                    setName.IsEnabled = setInputButton.IsEnabled = scoreTextBox.IsEnabled = false;
                    return true;
                }
                else if (rps.GameInProgress())
                {
                    // Update lobby canvas board to notify the game has started
                    inputLabel.Content = "Game in progress, please wait...";
                    setName.Text = "";
                    MessageBox.Show("Game is in progress please wait for the next game...");
                    return false;
                }
                else if (rps.LobbyIsFull())
                {
                    // Update lobby canvas notify the lobby is full
                    inputLabel.Content = "Lobby is full, please wait...";
                    setName.Text = "";
                    MessageBox.Show("Lobby is full please wait for the next game...");
                    return false;
                }
                else
                {
                    // Alias rejected by the service so nullify service proxies
                    MessageBox.Show("ERROR: Alias in use. Please try again.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        // Callback Methods

        private delegate void GuiLobbyUpdateDelegate();

        public void ReadyToStart()
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    // Update lobby canvas board to display joined players
                    lobbyLabel.Content = "Lobby - Ready To Start!";
                    startButton.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new GuiLobbyUpdateDelegate(ReadyToStart));
            }
        }

        private delegate void GuiStartUpdateDelegate(string[] scoreBoard, int winningScore);

        public void StartingGame(string[] scoreBoard, int winningScore)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    lobbyCanvas.Visibility = Visibility.Hidden;
                    gameCanvas.Visibility = Visibility.Visible;
                    finalScoresCanvas.Visibility = Visibility.Hidden;
                    RoundNumber = 1;
                    scoreBoardLabel.Content = $"Score Board - Reach {winningScore} to win!";
                    gameStatusLabel.Content = $"Round {RoundNumber} - Select a Hand!";
                    scoreBoardListBox.ItemsSource = scoreBoard;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new GuiStartUpdateDelegate(StartingGame), new object[] { scoreBoard, winningScore } );
            }
        }

        private delegate void GuiRoundUpdateDelegate();

        public void TieRound()
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    // Update game canvas notify the player they tied
                    gameStatusLabel.Content = $"Round {RoundNumber++} - Tie! No points gained.";
                    Rock.IsEnabled = true;
                    Paper.IsEnabled = true;
                    Scissors.IsEnabled = true;
                    lockInButton.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new GuiRoundUpdateDelegate(TieRound));
            }
        }

        public void WinRound()
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    // Update game canvas notify the player won
                    gameStatusLabel.Content = $"Round {RoundNumber++} - Win! Gained 1 Point.";
                    Rock.IsEnabled = true;
                    Paper.IsEnabled = true;
                    Scissors.IsEnabled = true;
                    lockInButton.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new GuiRoundUpdateDelegate(WinRound));
            }
        }

        public void LossRound()
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    // Update game canvas notify the player lost
                    gameStatusLabel.Content = $"Round {RoundNumber++} - Loss. No points gained.";
                    Rock.IsEnabled = true;
                    Paper.IsEnabled = true;
                    Scissors.IsEnabled = true;
                    lockInButton.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new GuiRoundUpdateDelegate(LossRound));
            }
        }

        public void SendHostDisconnected()
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    // Update disconnect canvas notify the host has left
                    hostDisconnectedCanvas.Visibility = Visibility.Visible;
                    lobbyCanvas.Visibility = Visibility.Hidden;
                    gameCanvas.Visibility = Visibility.Hidden;
                    finalScoresCanvas.Visibility = Visibility.Hidden;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new GuiRoundUpdateDelegate(SendHostDisconnected));
            }
        }

        private delegate void GuiScoreUpdateDelegate(string[] scoreBoard);

        public void SendLobbyBoard(string[] lobbyBoard)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    // Update lobby canvas board to display joined players
                    lobbyListBox.ItemsSource = lobbyBoard;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new GuiScoreUpdateDelegate(SendLobbyBoard), new object[] { lobbyBoard });
            }
        }

        public void SendScoreBoard(string[] scoreBoard)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    scoreBoardListBox.ItemsSource = scoreBoard;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new GuiScoreUpdateDelegate(SendScoreBoard), new object[] { scoreBoard });
            }
        }

        public void SendLockedInBoard(string[] lockedInBoard)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    playerStatusListBox.ItemsSource = lockedInBoard;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new GuiScoreUpdateDelegate(SendLockedInBoard), new object[] { lockedInBoard });
            }
        }

        public void EndGame(string[] scoreBoard)
        {
            if (this.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                try
                {
                    gameCanvas.Visibility = Visibility.Hidden;
                    finalScoresCanvas.Visibility = Visibility.Visible;

                    // Update final canvas score board with current scoreboard details after game ends
                    finalListBox.ItemsSource = scoreBoard;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new GuiScoreUpdateDelegate(EndGame), new object[] { scoreBoard });
            }
        }

        // Event Handlers

        // lobby canvas functions
        private void Button_Click_Set(object sender, RoutedEventArgs e)
        {
            try
            {
                PlayerName = setName.Text;

                bool clientIsHost = rps.IsHost();
                bool connected = connectToRockPaperScissors();

                if (connected && !clientIsHost)
                {
                    lobbyLabel.Content = "Lobby - Waiting on the host to start...";
                }
                else if (connected && clientIsHost)
                {
                    lobbyLabel.Content = "Lobby - Waiting for players to join...";
                    rps.SetPointLimit(int.Parse(scoreTextBox.Text));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button_Click_Start(object sender, RoutedEventArgs e)
        {
            rps.StartGame();
        }

        // game canvas functions
        private void Button_Click_Rock(object sender, RoutedEventArgs e)
        {
            gameStatusLabel.Content = $"Round {RoundNumber} - Rock Selected!";
            PlayerHand = "Rock";
        }

        private void Button_Click_Paper(object sender, RoutedEventArgs e)
        {
            gameStatusLabel.Content = $"Round {RoundNumber} - Paper Selected!";
            PlayerHand = "Paper";
        }

        private void Button_Click_Scissors(object sender, RoutedEventArgs e)
        {
            gameStatusLabel.Content = $"Round {RoundNumber} - Scissors Selected!";
            PlayerHand = "Scissors";
        }

        // set isEnabled to false on lock in button
        private void Button_Click_Lock_In(object sender, RoutedEventArgs e)
        {
            if(PlayerHand != "")
            {
                Rock.IsEnabled = false;
                Paper.IsEnabled = false;
                Scissors.IsEnabled = false;
                lockInButton.IsEnabled = false;
                gameStatusLabel.Content = $"Round {RoundNumber} - {PlayerHand} Locked In! Waiting...";
                rps.SelectHand(PlayerName, PlayerHand);
            }
        }

        private void Button_Click_To_Lobby(object sender, RoutedEventArgs e)
        {
            lobbyCanvas.Visibility = Visibility.Visible;
            gameCanvas.Visibility = Visibility.Hidden;
            finalScoresCanvas.Visibility = Visibility.Hidden;
            scoreTextBox.Text = "";
            scoreTextBox.IsEnabled = true;
            setScoreButton.Visibility = Visibility.Visible;
            setScoreButton.IsEnabled = true;
            setInputButton.Visibility = Visibility.Hidden;
            startButton.IsEnabled = false;
            inputLabel.Content = "Set a score to play up to!";
            rps.ResetScores();
        }

        private void Button_Click_Set_Score(object sender, RoutedEventArgs e)
        {
            try
            {
                rps.SetPointLimit(int.Parse(scoreTextBox.Text));
                scoreTextBox.IsEnabled = false;
                setScoreButton.IsEnabled = false;
                if (rps.ValidPlayerCount())
                {
                    startButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // final canvas functions
        private void Button_Click_End_Game(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(rps != null && PlayerName != "")
            {
                rps.Leave(PlayerName);
            }
        }

    }
}
