using System;
using System.Collections.ObjectModel;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WpfSqlConnectDialog
{
    public class ConnectionViewModel : ViewModelBase
    {
        public string cConnectString;

        private ObservableCollection<string> m_ServerList;
        public ObservableCollection<string> ServerList
        {
            get
            {
                return m_ServerList;
            }
            set
            {
                m_ServerList = value;
                NotifyPropertyChanged("ServerList");
            }
        }

        private ObservableCollection<string> m_DatabaseList;
        public ObservableCollection<string> DatabaseList
        {
            get
            {
                return m_DatabaseList;
            }
            set
            {
                m_DatabaseList = value;
                NotifyPropertyChanged("DatabaseList");
            }
        }

        private ObservableCollection<string> m_AuthItems;
        public ObservableCollection<string> AuthItems
        {
            get
            {
                return m_AuthItems;
            }
            set
            {
                m_AuthItems = value;
                NotifyPropertyChanged("AuthItems");
            }
        }

        private string m_SelectedServer;
        public string SelectedServer
        {
            get
            {
                return m_SelectedServer;
            }
            set
            {
                m_SelectedServer = value;
                NotifyPropertyChanged("SelectedServer");
            }
        }

        private string m_SelectedDatabase;
        public string SelectedDatabase
        {
            get
            {
                return m_SelectedDatabase;
            }
            set
            {
                m_SelectedDatabase = value;
                NotifyPropertyChanged("SelectedDatabase");
            }
        }

        private string m_SelectedAuth;
        public string SelectedAuth
        {
            get
            {
                return m_SelectedAuth;
            }
            set
            {
                m_SelectedAuth = value;
                switch (m_SelectedAuth)
                {
                    case "Windows-Authentifizierung":
                        IsSqlAuth = false;
                        break;
                    case "SQL-Server Authentifizierung":
                        IsSqlAuth = true;
                        break;
                }
                NotifyPropertyChanged("SelectedAuth");
            }
        }

        private bool m_IsSqlAuth;
        public bool IsSqlAuth
        {
            get
            {
                return m_IsSqlAuth;
            }
            set
            {
                m_IsSqlAuth = value;
                NotifyPropertyChanged("IsSqlAuth");
            }
        }

        private string m_AuthUser;
        public string AuthUser
        {
            get
            {
                return m_AuthUser;
            }
            set
            {
                m_AuthUser = value;
                NotifyPropertyChanged("AuthUser");
            }
        }

        private string m_AuthPass;
        public string AuthPass
        {
            get
            {
                return m_AuthPass;
            }
            set
            {
                m_AuthPass = value;
                NotifyPropertyChanged("AuthPass");
            }
        }

        private bool m_IsConOkay;
        public bool IsConOkay
        {
            get
            {
                return m_IsConOkay;
            }
            set
            {
                m_IsConOkay = value;
                NotifyPropertyChanged("IsConOkay");
            }
        }

        public ConnectionViewModel()
        {
            ServerList = new ObservableCollection<string>();
            DatabaseList = new ObservableCollection<string>();
            AuthItems = new ObservableCollection<string>
            {
                "Windows-Authentifizierung",
                "SQL-Server Authentifizierung"
            };
            SelectedAuth = "Windows-Authentifizierung";
            IsConOkay = false;
        }


        #region Commands
        public ICommand RefreshCmd { get { return new RelayCommand(OnRefresh, AlwaysTrue); } }
        public ICommand RefreshDbCmd { get { return new RelayCommand(OnRefreshDb, AlwaysTrue); } }
        public ICommand TestCmd { get { return new RelayCommand(OnTestCon, AlwaysTrue); } }
        public ICommand OkCmd { get { return new RelayCommand(OnOk, AlwaysTrue); } }
        public ICommand AbortCmd { get { return new RelayCommand(OnAbort, AlwaysTrue); } }

        private bool AlwaysTrue() { return true; }
        private bool AlwaysFalse() { return false; }

        private void OnRefresh()
        {
            ReadSqlServers();
        }

        private void OnRefreshDb()
        {
            ReadDatabases();
        }

        private void OnTestCon()
        {
            var cConStr = GetSqlConString();
            using (SqlConnection connection = new SqlConnection(cConStr))
            {
                try
                {
                    connection.Open();
                    MessageBox.Show("Verbindung erfolgreich hergestellt zu : " + cConStr);
                    IsConOkay = true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Verbindung fehlgeschlagen: " + e.Message);
                    IsConOkay = false;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private void OnOk()
        {
            foreach (Window item in Application.Current.Windows)
            {
                if (item.DataContext == this)
                {
                    cConnectString = GetSqlConString();
                    item.Close();
                }
            }
        }

        private void OnAbort()
        {
            foreach (Window item in Application.Current.Windows)
            {
                if (item.DataContext == this)
                {
                    cConnectString = "";
                    item.Close();
                }
            }
        }

        #endregion

        public void ReadSqlServers()
        {
            ServerList.Clear();
            SqlDataSourceEnumerator instance = SqlDataSourceEnumerator.Instance;
            System.Data.DataTable table = instance.GetDataSources();
            foreach (System.Data.DataRow oRow in table.Rows)
            {
                var ServerName = oRow[0].GetType() == typeof(DBNull) ? "" : (string)oRow[0];
                var InstanceName = oRow[1].GetType() == typeof(DBNull) ? "" : (string)oRow[1];
                var Version = oRow[3].GetType() == typeof(DBNull) ? "" : (string)oRow[3];
                ServerList.Add(ServerName + " (" + InstanceName + ") " + " / " + Version);
            }
        }

        public void ReadDatabases()
        {
            using (SqlConnection connection = new SqlConnection(GetSqlConString()))
            {
                SqlCommand command = new SqlCommand("select NAME from SYS.DATABASES", connection);
                command.Connection.Open();
                using (var oReader = command.ExecuteReader())
                {
                    while (oReader.HasRows && oReader.Read())
                    {
                        DatabaseList.Add((string)oReader["NAME"]);
                    }
                }
            }
        }

        public string GetSqlConString()
        {
            var cConStr = "Server=" + SelectedServer;
            switch (IsSqlAuth)
            {
                case true:

                    break;
                case false:
                    cConStr += ";Trusted_Connection=True";
                    break;
            }
            if (!string.IsNullOrEmpty(SelectedDatabase))
            {
                cConStr += ";Database=" + SelectedDatabase;
            }

            return cConStr;
        }
    }
}
