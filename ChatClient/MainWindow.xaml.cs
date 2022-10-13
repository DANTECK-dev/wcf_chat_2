using System;
using System.Collections.Generic;
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
using ChatClient.ServiceChat;

namespace ChatClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IServiceChatCallback
    {
        bool isConnected = false;
        ServiceChatClient client;
        int ID;
        bool isReady = false;
        TextBox[] TB_Users = new TextBox[4];
        Button[] BT_Cards = new Button[10];
        Label[] L_Names = new Label[3];
        Label[] L_Selected = new Label[3];
        Label[] L_Stats = new Label[3];

        SolidColorBrush Green = new SolidColorBrush(Color.FromRgb(21, 243, 202));
        SolidColorBrush Orange = new SolidColorBrush(Color.FromRgb(243, 163, 21));
        SolidColorBrush Red = new SolidColorBrush(Color.FromRgb(243, 21, 21));
        SolidColorBrush Transparent = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));

        public MainWindow()
        {
            InitializeComponent();
            new TextBox[4] { TB_Gamer1, TB_Gamer2, TB_Gamer3, TB_Gamer4 }.CopyTo(TB_Users, 0);
            new Button[10] { BT_Card1, BT_Card2, BT_Card3, BT_Card4, BT_Card5, BT_Card6, BT_Card7, BT_Card8, BT_Card9, BT_Card10}.CopyTo(BT_Cards, 0);
            new Label[3] { L_Gamer1, L_Gamer2, L_Gamer3, }.CopyTo(L_Names, 0);
            new Label[3] { L_Gamer1_Select, L_Gamer2_Select, L_Gamer3_Select, }.CopyTo(L_Selected, 0);
            new Label[3] { L_Gamer1_Status, L_Gamer2_Status, L_Gamer3_Status, }.CopyTo(L_Stats, 0);
            Lobby.Visibility = Visibility.Visible;
            Game.Visibility = Visibility.Hidden;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        void ConnectUser()
        {
            if (!isConnected)
            {
                client = new ServiceChatClient(new System.ServiceModel.InstanceContext(this));
                ID = client.Connect(TB_UserName.Text);
                if(ID == 0)
                {
                    MessageBox.Show("Ошибка подключения\nВозможные проблемы:\nМаксимальное количество игроков\nНе запущен сервер", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    TB_UserName.IsEnabled = false;
                    BT_Connect.Content = "Отключится";
                    isConnected = true;
                }
            }
            if(client != null && isConnected) client.SendMessage("Users", 0);
        }

        void DisconnectUser()
        {
            if (isConnected)
            {
                client.Disconnect(ID);
                client.SendMessage("Users", 0);
                client = null;
                TB_UserName.IsEnabled = true;
                BT_Connect.Content = "Подключится";
                isConnected = false;
            }
            if (client == null || !isConnected) { CleanUsers(); }
        }

        private void BT_Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isConnected)
                {
                    DisconnectUser();
                    BT_Start.IsEnabled = false;
                    BT_Start.Content = "Играть";
                }
                else
                {
                    ConnectUser();
                    BT_Start.IsEnabled = true;
                    client.SendMessage("Ready", 0);
                }
            } 
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        public void MsgCallback(string message)
        {
            try
            {
                Logs.Items.Add(message);
                Logs.ScrollIntoView(Logs.Items[Logs.Items.Count - 1]);
                string[] splited = message.Split('|');


                if (splited[0] == "Ready" || splited[0] == "unReady")
                {
                    UpdateReady(Convert.ToInt32(splited[1]));
                    return;
                }

                if (splited[0] == "Users")
                {
                    UpdateUsers(splited);
                    return;
                }

                if (splited[0] == "StartGame" && splited.Length > 1)
                {
                    BT_Start.Content = splited[1];
                    BT_Start.IsEnabled = false;
                    return;
                }

                if (splited[0] == "StartGame" && splited.Length == 1)
                {
                    BT_Start.Content = 0.ToString();
                    BT_Start.IsEnabled = false;
                    StartGame();
                    return;
                }

                if (splited[0] == "Selected")
                {
                    //SelectId(Convert.ToInt32(splited[2]));
                    return;
                }

                if (splited[0] == "NextRound")
                {
                    //NextRound(splited[1]);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                DisconnectUser();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void BT_Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isConnected)
                {
                    if (isReady)
                    {
                        client.SendMessage("unReady", ID);
                        isReady = false;
                    }
                    else
                    {
                        client.SendMessage("Ready", ID);
                        isReady = true;
                    }
                }
                if (client != null && isConnected) client.SendMessage("Users", 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void UpdateReady(int counter)
        {
            if (isReady) BT_Start.Content = "Ожидание " + counter + "/4";
            else BT_Start.Content = "Играть " + counter + "/4";
        }

        private void CleanUsers()
        {
            for (int i = 0; i < TB_Users.Length; i++)
            {
                TB_Users[i].Text = "Бот";
            }
        }

        private void UpdateUsers(string[] users)
        {
            CleanUsers();
            int k = 1;
            int j = 0;
            for (int i = 0; i < TB_Users.Length && k < users.Length; i++)
            {
                if (users[k] == "Bot")
                {
                    TB_Users[i].Text = "Бот";
                    k++;
                }
                else
                {
                    TB_Users[i].Text = users[k] + " : " + users[k + 1];
                    if (Convert.ToInt32(users[k]) != ID)
                    {
                        L_Names[j].Content = users[k] + " : " + users[k + 1];
                        j++;
                    }
                    k += 2;
                }
            }
        }

        private void StartGame()
        {
            Lobby.Visibility = Visibility.Hidden;
            Game.Visibility = Visibility.Visible;
            SetDefaultGame();
            //client.SendMessage("Users", 0);
            //Window.Width = 800;
            //Window.Height = 600;

            return;
        }

        private void BT_Card_MouseLeave(object sender, MouseEventArgs e)
        {
            int selected = -1;
            for (int i = 0; i < BT_Cards.Length; i++)
                if (sender == BT_Cards[i]) { selected = i; break; }
            if (selected == -1) return;
            BT_Cards[selected].Content = "Карта " + BT_Cards[selected].Content.ToString().TrimStart("Выбрать\nкарту".ToCharArray());
        }

        private void BT_Card_MouseEnter(object sender, MouseEventArgs e)
        {
            int selected = -1;
            for (int i = 0; i < BT_Cards.Length; i++)
                if (sender == BT_Cards[i]) { selected = i; break; }
            if (selected == -1) return;
            BT_Cards[selected].Content = "Выбрать\nкарту " + BT_Cards[selected].Content.ToString().TrimStart("Карта ".ToCharArray());
        }

        private void BT_Card_Click(object sender, RoutedEventArgs e)
        {
            int selected = -1;
            for (int i = 0; i < BT_Cards.Length; i++)
                if (sender == BT_Cards[i]) { selected = i; break; }
            if (selected == -1) return;
            client.SendMessage("SelectCard|" + selected, ID);
            for (int i = 0; i < BT_Cards.Length; i++)
                BT_Cards[i].IsEnabled = false;
        }
        private void SetDefaultGame()
        {
            foreach (var item in L_Names)
            {
                item.Content = "Бот";
            }
            foreach (var item in L_Selected)
            {
                item.Content = "Ожидание\nвыбора";
            }
            foreach (var item in L_Stats)
            {
                item.Content = string.Empty;
            }
        }
        private void SelectId(int id)
        {

        }
        private void NextRound(string status)
        {

        }
    }
}
