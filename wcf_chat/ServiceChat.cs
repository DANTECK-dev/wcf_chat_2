using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Windows;

namespace wcf_chat
{
  
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ServiceChat : IServiceChat
    {
        List<ServerUser> users = new List<ServerUser>();
        bool[] ready = new bool[4] { false, false, false, false };
        int nextId = 1;
        Game game = new Game();
        string[] selectedCard = new string[4] { string.Empty, string.Empty, string.Empty, string.Empty };
        //List<int> scores = new List<int>();

        public int Connect(string name)
        {
            try
            {
                if (users.Count() >= 4 || game.isGameStarted) return 0;
                ServerUser user = new ServerUser()
                {
                    ID = nextId,
                    Name = name,
                    operationContext = OperationContext.Current,
                    Ready = false,
                    Bot = false,
                    BlocedCards = new bool[10] { false, false, false, false, false, false, false, false, false, false }
                };
                nextId++;

                //Send(user.Name + "|Connected");
                game.SelectedDicriment();
                users.Add(user);
                return user.ID;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }

        public void Disconnect(int id)
        {
            try
            {
                var user = users.FirstOrDefault(i => i.ID == id);
                if (user != null)
                {
                    users.Remove(user);
                    Send(user.Name + "|Disconnected");
                    game.SelectedIncriment();
                }
                if (users.Count() == 0 && game.isGameStarted)
                {
                    Send("NextGame");
                    game.round = 1;
                    game.isGameStarted = false;
                }
                }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void Send(string message)
        {
            try
            {
                Console.WriteLine(message);
                foreach (var item in users)
                    item.operationContext.GetCallbackChannel<IServerChatCallback>().MsgCallback(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        public void SendMessage(string message, int id)
        {
            try
            {
                //Console.WriteLine(message + "|" + id);
                string[] strings = message.Split('|');
                if (strings[0] == "Ready")
                {
                    Ready(id);
                    return;
                }

                else if (strings[0] == "unReady")
                {
                    notReady(id);
                    return;
                }

                else if (strings[0] == "Users")
                {
                    Users();
                    return;
                }
                else if (strings[0] == "SelectCard")
                {
                    SelectCard(strings[1], id);
                    return;
                }

                else if (strings[0] == "CheckSelect")
                {
                    CheckSelect();
                    return;
                }

                //else if (strings[0] == "Score")
                //{
                //    Score(id);
                //    return;
                //}
                //else if (strings[0] == "TransformCard")
                //{
                //    TransformCards();
                //    return;
                //}
                //else if (strings[0] == "NextRound")
                //{
                //    NextRound();
                //    return;
                //}
                else
                {
                    Send(message + "|" + id);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void Ready(int id)
        {
            int cou = 0;
            for (int i = users.Count - 1; i >= 0; i--)
            {
                if (users[i].ID == id && users[i].Ready == false)
                {
                    users[i].Ready = true;
                    ready[i] = true;
                }
                if (users[i].Ready == true)
                {
                    cou++;
                }
            }
            Send("Ready|" + (cou + (4 - users.Count)));
            if (cou == users.Count)
            {
                for (int i = 0; i < 3; i++)
                {
                    Send("StartGame|" + (3 - i));
                    Thread.Sleep(1000);
                }
                Send("StartGame");
                game.isGameStarted = true;
            }
        }
        private void notReady(int id)
        {
            int cou = 0;
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].ID == id && users[i].Ready == true)
                {
                    users[i].Ready = false;
                    ready[i] = false;
                }
                if (users[i].Ready == true)
                {
                    cou++;
                }
            }
            Send("Ready|" + (cou + (4 - users.Count)));
        }
        private void Users()
        {
            string answear = "Users|";
            for(int i = 0; i < users.Count; i++)
            {
                if (users[i].Bot == true)
                {
                    answear += "Bot|";
                }
                else
                {
                    answear += users[i].ID + "|" + users[i].Name + "|";
                }
            }
            Send(answear.TrimEnd('|'));
        }
        private void SelectCard(string selected, int id)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].ID == id)
                {
                    //users[i].BlocedCards[Convert.ToInt32(selected) - 1] = true;
                    for(var j = 0; j < selectedCard.Length; j++)
                    {
                        if (selectedCard[j] == string.Empty)
                        {
                            selectedCard[j] = "|" + id + "|" + selected;
                            break;
                        }
                    }
                    break;
                }
            }
            Send("Selected|" + id);
            game.SelectedIncriment();
            if(game.selected == 4)
            {
                CheckSelect();
            }
        }
        private void CheckSelect()
        {
            Random rand = new Random();
            string answerSelectedCard = "SelectedCard";
            for(int i = 0; i < selectedCard.Length; i++)
            {
                if (selectedCard[i] == string.Empty) break;
                answerSelectedCard += selectedCard[i];
            }
            for(int i = 0; i < 4 - users.Count; i++)
            {
                answerSelectedCard += "|Бот|" + rand.Next(1, 10);
            }
            Send(answerSelectedCard);
            game.selected = 4 - users.Count;
            game.RoundIncriment();
            Thread.Sleep(1000);
            Send("TransformCard");
            Thread.Sleep(1000);
            Send("NextRound");
            Thread.Sleep(1000);
            for (int i = 0; i < selectedCard.Length; i++)
            {
                selectedCard[i] = string.Empty;
            }
            if(game.round == 9)
            {
                Send("NextGame");
                //scores.Clear();
                game.round = 1;
                game.isGameStarted = false;
            }
        }
        //private void Score(int score)
        //{
        //    if (users.Count > 1)
        //    {
        //        scores.Add(score);

        //        if(scores.Count == users.Count)
        //        {
        //          Send("Score|" + scores.Max());
        //          Thread.Sleep(3000);
        //        }
        //    }
        //}
        //private void TransformCards()
        //{
        //    Thread.Sleep(3000);
        //    Send("TransformCard");
        //}
        //private void NextRound()
        //{
        //    Thread.Sleep(3000);
        //    Send("NextRound");
        //}
    }
}