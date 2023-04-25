/*
 *  Program:        RockPaperScissorsLibrary.dll 
 *  Module:         RockPaperScissors.cs
 *  Author:         Ali, Dylan, Mahan, Manh
 *  Date:           March 18, 2022
 *  Description:    Defines a RockPaperScissors type that maintains and returns a score board collection
 *                  of strings that are updated individually by a client (or clients) after playing a round.
 *                  Note that the score board does not persist when the app is shut down.
 *                  
 *                  This module also defines a modified IRockPaperScissors service contract. Clients now
 *                  "register" their alias with the service by calling the Join() method and "unregister"
 *                  for the service by calling the Leave() method.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;  // WCF types

namespace RockPaperScissorsLibrary
{
    public interface ICallback
    {
        [OperationContract(IsOneWay = true)]
        void ReadyToStart();
        [OperationContract(IsOneWay = true)]
        void TieRound();
        [OperationContract(IsOneWay = true)]
        void WinRound();
        [OperationContract(IsOneWay = true)]
        void LossRound();
        [OperationContract(IsOneWay = true)]
        void SendLockedInBoard(string[] lockedInBoard);
        [OperationContract(IsOneWay = true)]
        void SendLobbyBoard(string[] lobbyBoard);
        [OperationContract(IsOneWay = true)]
        void SendScoreBoard(string[] scoreBoard);
        [OperationContract(IsOneWay = true)]
        void StartingGame(string[] scoreboard, int winningScore);
        [OperationContract(IsOneWay = true)]
        void EndGame(string[] finalScoreboard);
        [OperationContract(IsOneWay = true)]
        void SendHostDisconnected();
    }//End of ICallback

    // The service contract

    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface IRockPaperScissors
    {
        [OperationContract]
        bool IsHost();
        [OperationContract]
        bool GameInProgress();
        [OperationContract]
        bool LobbyIsFull();
        [OperationContract]
        bool Join(string name);
        [OperationContract(IsOneWay = true)]
        void Leave(string name);
        [OperationContract]
        void SelectHand(string name,string hand);
        [OperationContract]
        void SetPointLimit(int pointLimit);
        [OperationContract]
        void StartGame();
        [OperationContract]
        bool ValidPlayerCount();
        [OperationContract]
        void ResetScores();
    }

    // A class that implements the service contract
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class RockPaperScissors : IRockPaperScissors
    {
        // Private data members
        private List<Player> players = new List<Player>();


        // Game data members
        private ICallback _hostCallback;
        private bool _hostIsConnected;
        private bool _gameStarted = false;
        private int _pointLimit;

        //enum choice { rock,paper,scissor }

        private bool FindPlayer(string name)
        {
            name = name.ToUpper();
            foreach (Player p in players)
            {
                if (p.Name.ToUpper() == name)
                {
                    return true;
                }
            }

            return false;
        }

        private void RemovePlayer(string name)
        {
            name = name.ToUpper();
            foreach (Player p in players)
            {
                if (p.Name.ToUpper() == name)
                {
                    players.Remove(p);
                    return;
                }
            }
        }

        public bool IsHost()
        {
            if (!_hostIsConnected)
            {
                _hostCallback = OperationContext.Current.GetCallbackChannel<ICallback>();
                return _hostIsConnected = true;
            }
            return _hostCallback == OperationContext.Current.GetCallbackChannel<ICallback>();
        }//End of IsHost

        public bool GameInProgress()
        {
            return _gameStarted;
        }//End of GameInProgress

        public bool LobbyIsFull()
        {
            return players.Count >= 4;
        }//End of GameInProgress

        public bool ValidPlayerCount()
        {
            return players.Count >= 2 && players.Count <= 4;
        }//End of ValidPlayerCount

        public bool Join(string name)
        {
            if (FindPlayer(name) || _gameStarted || players.Count == 4)
            {
                // User alias must be unique
                return false;
            }
            else
            {
                // Retrieve client's callback proxy
                ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();

                Player newP = new Player()
                {
                    Name = name,
                    IsHost = _hostCallback == cb,
                    Callback = cb,
                    Score = 0,
                    Hand = ""
                };
                if (IsHost())
                {
                    players.Insert(0, newP);
                }
                else
                {
                    players.Add(newP);
                }

                UpdateUserLobbies();

                if (ValidPlayerCount())
                {
                    foreach (Player player in players)
                    {
                        if (player.IsHost)
                        {
                            player.Callback.ReadyToStart();
                        }
                    }
                }

                return true;
            }
        }//End of Join

        public void Leave(string name)
        {
            RemovePlayer(name);
            UpdateUserLobbies();
            UpdateUserScoreBoards();
            UpdateLockedInBoards();
            if (IsHost())
            {
                foreach (Player p in players)
                {
                    p.Callback.SendHostDisconnected();
                }
            }
        }//End of Leave

        public void SelectHand(string name, string hand)
        {
            if (FindPlayer(name))
            {
                foreach (Player p in players)
                {
                    if (p.Name.ToUpper() == name.ToUpper())
                    {
                        p.Hand = hand;
                    }
                }
            }

            foreach (Player p in players)
            {
                p.Callback.SendLockedInBoard(GetLockedInBoard());
            }

            //if everyone selected their hand (not empty) then start the game
            foreach (Player p in players)
            {
                if (p.Hand == "")
                {
                    return;
                }
            }

            Play();
        }//End of SelectHand
        

        //Call after getting all choice from players
        public void Play()
        {
            bool[] selectedHands = new bool[3];
            selectedHands[0] = selectedHands[1] = selectedHands[2] = false;
            foreach (Player p in players)
            {
                switch (p.Hand)
                {
                    case "Rock":
                        selectedHands[0] = true;
                        break;
                    case "Paper":
                        selectedHands[1] = true;
                        break;
                    case "Scissors":
                        selectedHands[2] = true;
                        break;
                }
            }
            if (selectedHands[0] && selectedHands[1] && selectedHands[2] || (selectedHands[0] && !selectedHands[1] && !selectedHands[2]) || (!selectedHands[0] && selectedHands[1] && !selectedHands[2]) || (!selectedHands[0] && !selectedHands[1] && selectedHands[2]))
            {
                foreach (Player p in players)
                {
                    p.Callback.TieRound();
                }
                ClearAllHands();
                UpdateLockedInBoards();
                return;
            }

            foreach (Player p in players)
            {
                switch (p.Hand)
                {
                    case "Rock":
                        if (!selectedHands[1] && selectedHands[2])
                        {
                            p.Score += 1;
                            p.Callback.WinRound();
                        }
                        else
                        {
                            p.Callback.LossRound();
                        }
                        break;
                    case "Paper":
                        if (!selectedHands[2] && selectedHands[0])
                        {
                            p.Score += 1;
                            p.Callback.WinRound();
                        }
                        else
                        {
                            p.Callback.LossRound();
                        }
                        break;
                    case "Scissors":
                        if (!selectedHands[0] && selectedHands[1])
                        {
                            p.Score += 1;
                            p.Callback.WinRound();
                        }
                        else
                        {
                            p.Callback.LossRound();
                        }
                        break;
                }
            }

            // clears every players hand for the next round
            ClearAllHands();

            UpdateUserScoreBoards();

            UpdateLockedInBoards();

            if (GameOver())
            {
                foreach (Player p in players)
                {
                    p.Callback.EndGame(GetFinalScoreBoard());
                }
            }
        }//End of Play()

        private void UpdateUserLobbies()
        {
            List<string> lobbyText = new List<string>();
            foreach (Player p in players)
            {
                lobbyText.Add(string.Format("{0} {1}", p.Name, p.IsHost ? "- Host" : ""));
            }

            string[] lobby = lobbyText.ToArray();

            foreach (Player p in players)
            {
                p.Callback.SendLobbyBoard(lobby);
            }
        }//End of UpdateUserLobbies()

        public string[] GetScoreBoard()
        {
            List<string> scoreBoardText = new List<string>();
            List<Player> sortedByScore = new List<Player>();
            sortedByScore.AddRange(players);
            sortedByScore.Sort((p1, p2) => p1.Score == p2.Score ? 0 : p1.Score < p2.Score ? 1 : -1);
            foreach (Player p in sortedByScore)
            {
                scoreBoardText.Add(string.Format("{0, 2} - {1}", p.Score, p.Name));
            }

            return scoreBoardText.ToArray();
        }//End of GetScoreBoard()

        private void UpdateUserScoreBoards()
        {
            string[] scoreboard = GetScoreBoard();
            foreach (Player p in players)
            {
                p.Callback.SendScoreBoard(scoreboard);
            }
        }//End of UpdateUserScoreBoards()

        public string[] GetLockedInBoard()
        {
            List<string> lockedInBoardText = new List<string>();
            foreach (Player p in players)
            {
                lockedInBoardText.Add(string.Format("{0} - {1}", p.Name, p.Hand == "" ? "Selecting..." : "Locked In"));
            }

            return lockedInBoardText.ToArray();
        }//End of GetLockedInBoard()

        private void UpdateLockedInBoards()
        {
            string[] lockedInBoard = GetLockedInBoard();
            foreach (Player p in players)
            {
                p.Callback.SendLockedInBoard(lockedInBoard);
            }
        }//End of UpdateLockedInBoards()

        public string[] GetFinalScoreBoard()
        {
            List<string> scoreBoardText = new List<string>();
            List<Player> sortedByScore = new List<Player>();
            sortedByScore.AddRange(players);
            sortedByScore.Sort((p1, p2) => p1.Score == p2.Score ? 0 : p1.Score < p2.Score ? 1 : -1);
            foreach (Player p in sortedByScore)
            {
                scoreBoardText.Add(string.Format("{0, -6} - {1, 2} - {2}", p.Score == _pointLimit ? "Winner" : "Loser", p.Score, p.Name));
            }

            return scoreBoardText.ToArray();
        }//End of GetFinalScoreBoard()

        public void SetPointLimit(int pointLimit)
        {
            _pointLimit = pointLimit;
        }

        private bool GameOver()
        {
            foreach (Player p in players)
            {
                if (p.Score >= _pointLimit)
                {
                    return true;
                }
            }
            return false;
        }//End of GameOver()

        private void ClearAllHands()
        {
            //Sets all of the player hands to an empty string for the next round
            foreach (Player p in players)
            {
                p.Hand = "";
            }
        }//End of ClearAllHands()

        public void StartGame()
        {
            string[] scoreboard = GetScoreBoard();
            string[] lockedInBoard = GetLockedInBoard();
            foreach (Player p in players)
            {
                p.Callback.StartingGame(scoreboard, _pointLimit);
                p.Callback.SendLockedInBoard(lockedInBoard);
            }
        }//End of StartGame()

        public void ResetScores()
        {
            foreach (Player p in players)
            {
                p.Score = 0;
                p.Hand = "";
            }
        }//End of StartGame()

    }//End of class
}//End of namespace
