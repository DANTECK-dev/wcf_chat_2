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

        public int Connect(string name)
        {
            try
            {
                if (users.Count() >= 4) return 0;
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
            Send("Ready|" + cou);
            if (cou == users.Count)
            {
                for (int i = 0; i < 3; i++)
                {
                    Send("StartGame|" + (3 - i));
                    Thread.Sleep(1000);
                }
                Send("StartGame");
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
            Send("Ready|" + cou);
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
                    users[i].BlocedCards[Convert.ToInt32(selected)] = true;
                }
            }
            Send("Selected|" + selected + "|" + id);
        }
    }
}