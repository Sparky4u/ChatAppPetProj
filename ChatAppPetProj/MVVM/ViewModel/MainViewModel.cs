using ChatClient.MVVM.Core;
using ChatClient.MVVM.Model;
using ChatClient.Net;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ChatClient.MVVM.ViewModel
{
    internal class MainViewModel : BaseNotifyPropertyChanged
    {
        private readonly Server _server;
        private string _message;

        public MainViewModel()
        {
            Users = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<string>();

            _server = new Server();
            _server.ConnectedEvent += UserConnected;
            _server.MsgReceivedEvent += MessageReceived;
            _server.UserDisconnectEvent += RemovedUser;
            ConnectToSeverCommand = new RelayCommand(_ => ExecuteConnectToServer(), _ => GetCanConnectToServer());
            SendMessageCommand = new RelayCommand(_ => ExecuteSendMessage(), _ => GetCanSendMessage());
            
        }

        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<string> Messages { get; set; }
        public RelayCommand ConnectToSeverCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }

        public string Username { get; set; }

        public string Message
        {
            get => _message;
            set
            {
                if (value == _message) return;
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        private void UserConnected(UserModel connectedUser)
        {
            if (Users.All(x => x.UID != connectedUser.UID))
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(connectedUser));
            }
        }

        private void RemovedUser(string uid)
        {
            var user = Users.FirstOrDefault(x => x.UID == uid);
            Users.Remove(user);
        }

        private void MessageReceived(string message)
        {
            Application.Current.Dispatcher.Invoke(() => Messages.Add(message));
        }

        private void ExecuteConnectToServer()
        {
            _server.ConnectToServer(Username);
        }

        private void ExecuteSendMessage()
        {
            _server.SendMessageToServer(Message);
            Message = string.Empty;
        }

        private bool GetCanConnectToServer()
        {
            return !string.IsNullOrEmpty(Username);
        }

        private bool GetCanSendMessage()
        {
            return !string.IsNullOrEmpty(Message);
        }
    }
}
