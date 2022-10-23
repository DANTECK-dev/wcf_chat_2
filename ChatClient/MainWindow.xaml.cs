using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        Random random = new Random();

        bool isConnected = false;
        ServiceChatClient client;

        int ID;
        bool isReady = false;

        TextBox[] TB_Users = new TextBox[4];
        Button[] BT_Cards = new Button[10];
        Label[] L_Names = new Label[3];
        Label[] L_Selected = new Label[3];
        Label[] L_Stats = new Label[3];

        int selectedCard;

        SolidColorBrush Green = new SolidColorBrush(Color.FromRgb(21, 243, 202));
        SolidColorBrush Orange = new SolidColorBrush(Color.FromRgb(243, 163, 21));
        SolidColorBrush Red = new SolidColorBrush(Color.FromRgb(243, 21, 21));
        SolidColorBrush Transparent = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));

        public MainWindow()
        {
            InitializeComponent();
            new TextBox[4] { TB_Gamer1, TB_Gamer2, TB_Gamer3, TB_Gamer4 }.CopyTo(TB_Users, 0);
            new Button[10] { BT_Card1, BT_Card2, BT_Card3, BT_Card4, BT_Card5, BT_Card6, BT_Card7, BT_Card8, BT_Card9, BT_Card10 }.CopyTo(BT_Cards, 0);
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
                if (TB_UserName.Text.Count(c => c == '|') != 0)
                {
                    MessageBox.Show("Имя пользователя не должно содержать символа |", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                client = new ServiceChatClient(new System.ServiceModel.InstanceContext(this));
                ID = client.Connect(TB_UserName.Text);
                if (ID == 0)
                {
                    MessageBox.Show("Ошибка подключения\nВозможные проблемы:\nМаксимальное количество игроков\nНе запущен сервер\nИгра уже началась", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    BT_Connect.Content = "Отключится";
                    client.SendMessage("Ready", 0);
                    TB_UserName.IsEnabled = false;
                    BT_Start.IsEnabled = true;
                    isConnected = true;
                }
            }
            if (client != null && isConnected) client.SendMessage("Users", 0);
        }

        void DisconnectUser()
        {
            if (isConnected)
            {
                client.Disconnect(ID);
                client.SendMessage("Users", 0);
                client.SendMessage("unReady", ID);
                BT_Connect.Content = "Подключится";
                BT_Start.Content = "Играть";
                TB_UserName.IsEnabled = true;
                BT_Start.IsEnabled = false;
                isConnected = false;
                isReady = false;
                client = null;
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
                }
                else
                {
                    ConnectUser();
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

                else if (splited[0] == "Users")
                {
                    UpdateUsers(splited);
                    return;
                }

                else if (splited[0] == "StartGame" && splited.Length > 1)
                {
                    BT_Start.Content = splited[1];
                    BT_Start.IsEnabled = false;
                    return;
                }

                else if (splited[0] == "StartGame" && splited.Length == 1)
                {
                    BT_Start.Content = 0.ToString();
                    BT_Start.IsEnabled = false;
                    StartGame();
                    return;
                }

                else if (splited[0] == "Selected")
                {
                    SelectId(Convert.ToInt32(splited[1]));
                    return;
                }

                else if (splited[0] == "SelectedCard")
                {
                    SelectedCard(splited);
                    return;
                }

                else if (splited[0] == "TransformCard")
                {
                    TransformCard();
                    return;
                }

                else if (splited[0] == "NextRound")
                {
                    NextRound();
                    return;
                }

                else if (splited[0] == "NextGame")
                {
                    NextGame();
                    return;
                }

                else if (splited[0] == "Score")
                {
                    //Score(Convert.ToInt32(splited[1]));
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
            SetDefaultGame();
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
            client.SendMessage("Users", 0);
            BT_Cards[random.Next(10)].Visibility = Visibility.Hidden;
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
            BT_Cards[selected].Visibility = Visibility.Hidden;
            client.SendMessage("SelectCard|" + ++selected, ID);
            selectedCard = selected;
            for (int i = 0; i < BT_Cards.Length; i++)
                BT_Cards[i].IsEnabled = false;
            L_MySelect.Content = "Выбрана\nкарта " + selected;
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
            foreach(var item in BT_Cards)
            {
                item.IsEnabled = true;
            }
            L_MySelect.Content = "Ожидание\nвыбора";
        }
        private void SelectId(int id)
        {
            for (int i = 0; i < L_Names.Length; i++)
            {
                if (L_Names[i].Content.ToString() == "Бот") continue;
                if (Convert.ToInt32(L_Names[i].Content.ToString().Split(' ')[0]) == id)
                {
                    L_Selected[i].Content = "Выбрал";
                    break;
                }
            }
        }
        int[] sel = new int[4];
        private void SelectedCard(string[] selectedCards)
        {
            sel = new int[4];
            sel[0] = selectedCard;
            int x = 1;
            int k = 1;
            for (int i = 1; i < selectedCards.Length; i += 2)
            {
                for (var j = 0; j < L_Selected.Length; j++)
                {
                    if (selectedCards[i] == "Бот" && L_Selected[j].Content.ToString().Split(' ').Length == 1)
                    {
                        sel[x] = Convert.ToInt32(selectedCards[i + 1]);
                        L_Selected[j].Content = "Выбрал " + sel[x];
                        x++;
                        break;
                    }
                    if (selectedCards[i] == L_Names[j].Content.ToString().Split(' ')[0] && L_Selected[j].Content.ToString().Split(' ').Length == 1)
                    {
                        sel[x] = Convert.ToInt32(selectedCards[i + 1]);
                        L_Selected[j].Content = "Выбрал " + sel[x];
                        x++;
                        break;
                    }
                }
            }
            //client.SendMessage("TransformCard", 0);
        }
        private void TransformCard()
        {
            sel[0] = Convert.ToInt32(L_MySelect.Content.ToString().Split(' ')[1]);
            for(int i = 0; i < L_Selected.Length; i++)
                sel[i + 1] = Convert.ToInt32(L_Selected[i].Content.ToString().Split(' ')[1]);
            
            bool find10 = false;
            bool find1 = false;

            for (int i = 0; i < sel.Length; i++)
                if (sel[i] == 10) find10 = true;
                else if (sel[i] == 1) find1 = true;

            if (find10 && find1)
            {
                for (int i = 0; i < sel.Length; i++)
                    if (sel[i] == 10) sel[i] = 0;
                    else if (sel[i] == 1) sel[i] = 10;
            }

            for (int i = 0; i < sel.Length; i++)
                for (int j = 0; j < sel.Length && j != i; j++)
                    if (sel[j] == sel[i]) sel[j] = sel[i] = 0;

            L_MySelect.Content = "Стало\nкарта " + sel[0];

            for (var i = 0; i < L_Selected.Length; i++)
                L_Selected[i].Content = "Стало " + sel[i + 1];
        }
        private void NextRound()
        {
            SetDefaultGame();
            L_Round.Content = "Раунд " + (Convert.ToInt32(L_Round.Content.ToString().Split(' ')[1]) + 1);
            if (sel.Max() == sel[0] && sel[0] > 0)
                L_Score.Content = "Счёт " + (Convert.ToInt32(L_Score.Content.ToString().Split(' ')[1]) + 1);
            client.SendMessage("Users", 0);
            //if (L_Round.Content.ToString() == "Раунд 8") 
            //    client.SendMessage("Score", Convert.ToInt32(L_Score.Content.ToString().Split(' ')[1]));
        }
        private void NextGame()
        {
            Lobby.Visibility = Visibility.Visible;
            Game.Visibility = Visibility.Hidden;
            client.SendMessage("Users", 0);
            for(int i = 0; i < BT_Cards.Length; i++)
            {
                BT_Cards[i].Visibility = Visibility.Visible;
            }
            BT_Start.IsEnabled = true;
            client.SendMessage("unReady", ID);
            L_Score.Content = "Счёт 0";
            isReady = false;
            L_Round.Content = "Раунд 1";
            L_Win.Visibility = Visibility.Hidden;
        }

        //private void Score(int score)
        //{
        //    if(score == sel[0]) L_Win.Visibility = Visibility.Visible;
        //}
    }
}
