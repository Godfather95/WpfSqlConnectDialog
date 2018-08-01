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

        #region Commands
        public ICommand RefreshCmd { get { return new RelayCommand(OnRefresh, AlwaysTrue); } }
        public ICommand RefreshDbCmd { get { return new RelayCommand(OnRefreshDb, AlwaysTrue); } }
        public ICommand TestCmd { get { return new RelayCommand(OnTestCon, AlwaysTrue); } }

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
                }
                catch (Exception e)
                {
                    MessageBox.Show("Verbindung fehlgeschlagen: " + e.Message);
                }
                finally
                {
                    connection.Close();
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
                ServerList.Add((string)oRow[0]);
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
