using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;

namespace DBCommon
{
    
    class MysqlDB
    {
        public string UserID { get { return _id; } set { _id = value; } }
        public string UserPW { get { return _pw; } set { _pw = value; } }
        public string ServerName { get { return _serverName; } set { _serverName = value; } }
        public string DBSchema { get { return _schema; } set { _schema = value; } }
        public string DBTable { get { return _table; } set { _table = value; } }
        public DataList DataCollections 
        { 
            get { return _dataCollection; } 
            set 
            { 
                _dataCollection.co2 = value.co2;
                _dataCollection.temperature= value.temperature;
                _dataCollection.humidity = value.humidity;
                _dataCollection.roomOccupied= value.roomOccupied;
                _dataCollection.outdoorTemperature= value.outdoorTemperature;
                _dataCollection.outdoorHumidity= value.outdoorHumidity;
                // 출처: https://www.dlenc.co.kr/rnd/download/download.do?no_ntc_plte_sral=9491
                if (value.co2 < 1000)
                {
                    _dataCollection.co2Approx = EApproxData.LOW;
                }
                else if(value.co2 >= 1000 && value.co2 < 5000)
                {
                    _dataCollection.co2Approx= EApproxData.MEDIUM;
                }
                else
                {
                    _dataCollection.co2Approx= EApproxData.HIGH;
                }
                //
                //  TODO: 써보고 값을 바꿀것
                //
                if (value.gas < 200)
                {
                    _dataCollection.gasApprox= EApproxData.LOW;
                }
                else if(value.gas >= 200 && value.gas < 400)
                {
                    _dataCollection.gasApprox= EApproxData.MEDIUM;
                }
                else
                {
                    _dataCollection.gasApprox= EApproxData.HIGH;
                }
                //
                //  기준값: 13 미만 LOW, 25 적정
                //
                if(value.temperature< 13)
                {
                    _dataCollection.temperatureApprox= EApproxData.LOW;
                }
                else if(value.temperature >= 13 && value.temperature < 26)
                {
                    _dataCollection.temperatureApprox= EApproxData.MEDIUM;
                }
                else
                {
                    _dataCollection.temperatureApprox= EApproxData.HIGH;
                }

                if(value.humidity < 50)
                {
                    _dataCollection.humidityApprox= EApproxData.LOW;
                }
                else if(value.humidity >= 50 && value.humidity <70)
                {
                    _dataCollection.humidityApprox = EApproxData.MEDIUM;
                }
                else
                {
                    _dataCollection.humidityApprox = EApproxData.HIGH;
                }
            }
        }

        public enum EApproxData { LOW, MEDIUM, HIGH }
        public enum EYesNo { Y, N }
        public struct DataList
        {
            public int idx;
            public float co2;
            public EApproxData co2Approx;
            public float gas;
            public EApproxData gasApprox;
            public float temperature;
            public EApproxData temperatureApprox;
            public float humidity;
            public EApproxData humidityApprox;
            public EYesNo roomOccupied;
            public float outdoorTemperature;
            public float outdoorHumidity;
        }
        MySqlConnection conn;
        MySqlCommand cmd;
        public MysqlDB() 
        {
            DBTable = "room";
            DoConnSQL(); 
        }
        public MysqlDB(string id, string pw, string defaultServer = "localhost", string schema = "HumidTempBoard", string table = "room") 
        {
            _id = id;
            _pw = pw;
            DBSchema = schema;
            DBTable = table;
            DoConnSQL(false);
            _dataCollection = new DataList();
        }
        ~MysqlDB() { }

        private string _id;
        private string _pw;
        private string _serverName;
        private string _schema;
        private string _table;
        private string connQuery;
        private DataList _dataCollection;
        private string[] dataColumn =
        { "idx", "co2", "co2_approx", "gas", "gas_approx", "temp", 
            "temp_approx", "humid", "humid_approx", "room_occupied", 
            "outdoor_temp", "outdoor_humid", "curr_date"};


        // <summary>
        //  MySQL에 연결 후 올바르게 연결했는지 나타내줍니다
        // </summary>
        // <return>
        // 올바르게 연결한경우 true, 그렇지 않으면 false
        // </return>
        public bool DoConnSQL(bool defaultMode = true)
        {
            if(defaultMode)
            {
                connQuery = "Server=localhost;Database=HumidTempBoard;Uid=root;Pwd=root;";
            }
            else
            {
                connQuery = $"Server={ServerName};Database={DBSchema};Uid={UserID};Pwd={UserPW};";
            }
            conn = new MySqlConnection(connQuery);
            try
            {
                conn.Open ();
            }
            catch(MySqlException ex)
            {
                Console.WriteLine($"SQL 연결 예외 발생: {ex.Message}");
            }
            cmd = new MySqlCommand();
            if(conn.State == ConnectionState.Open)
            {
                return true;
            }
            
            return false;
        }

        public bool DisconnSQL()
        {
            if(conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
            return true;
        }
        //
        //  return dataColumn[] string
        //
        public string ColumnToString(bool InsertMode = false)
        {
            string line = "";
            if(InsertMode == true)
            {
                for (int index = 1; index < dataColumn.Length - 1; index++)
                {
                    line += dataColumn[index];
                    if (index < dataColumn.Length - 2)
                    {
                        line += ", ";
                    }
                }
            }
            else
            {
                for (int index = 0; index < dataColumn.Length; index++)
                {
                    line += dataColumn[index];
                    if (index < dataColumn.Length - 1)
                    {
                        line += ", ";
                    }
                }
            }            
            return line;
        }
        public string ColumnDataTravelsal(bool InsertMode = false)
        {
            string line = "";
            if(InsertMode == true)
            {
                line = $"{DataCollections.co2}, '{DataCollections.co2Approx.ToString()}', {DataCollections.gas}, '{DataCollections.gasApprox.ToString()}', {DataCollections.temperature}, '{DataCollections.temperatureApprox.ToString()}', {DataCollections.humidity}, '{DataCollections.humidityApprox.ToString()}', '{DataCollections.roomOccupied.ToString()}', {DataCollections.outdoorTemperature}, {DataCollections.outdoorHumidity}";
            }
            else
            {
                line = $"{DataCollections.idx}, {DataCollections.co2}, '{DataCollections.co2Approx.ToString()}', {DataCollections.gas}, '{DataCollections.gasApprox.ToString()}', {DataCollections.temperature}, '{DataCollections.temperatureApprox.ToString()}', {DataCollections.humidity}, '{DataCollections.humidityApprox.ToString()}', '{DataCollections.roomOccupied.ToString()}', {DataCollections.outdoorTemperature}, {DataCollections.outdoorHumidity}";
            }
            return line;
        }

        public string DataListToString(DataList dl)
        {
            string line = "";
            line = $"{dl.co2}, '{dl.co2Approx.ToString()}', {dl.gas}, '{dl.gasApprox.ToString()}', {dl.temperature}, '{dl.temperatureApprox.ToString()}', {dl.humidity}, '{dl.humidityApprox.ToString()}', '{dl.roomOccupied.ToString()}', {dl.outdoorTemperature}, {dl.outdoorHumidity}";
            return line;
        }

        public bool InsertData()
        {            
            if(DoConnSQL() == false)
            {
                return false;
            }
            cmd.CommandText = $"INSERT INTO {DBTable}({ColumnToString(true)}) VALUE ({ColumnDataTravelsal(true)});";
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.ExecuteNonQuery();

            return true;
        }

        public List<DataList> DataListsDetail()
        {
            List<DataList> WholeDataList = new List<DataList>();
            cmd.CommandText = $"SELECT {ColumnToString(false)} FROM {DBTable};";
            cmd.Connection = conn;

            cmd.CommandType = CommandType.TableDirect;
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    DataList data = new DataList
                    {
                        idx = Convert.ToInt32(reader["idx"]),
                        co2 = Convert.ToSingle(reader["co2"]),
                        co2Approx = (EApproxData)Enum.Parse(typeof(EApproxData), reader["co2Approx"].ToString()),
                        gas = Convert.ToSingle(reader["gas"]),
                        gasApprox = (EApproxData)Enum.Parse(typeof(EApproxData), reader["gasApprox"].ToString()),
                        temperature = Convert.ToSingle(reader["temperature"]),
                        temperatureApprox = (EApproxData)Enum.Parse(typeof(EApproxData), reader["temperatureApprox"].ToString()),
                        humidity = Convert.ToSingle(reader["humidity"]),
                        humidityApprox = (EApproxData)Enum.Parse(typeof(EApproxData), reader["humidityApprox"].ToString()),
                        roomOccupied = (EYesNo)Enum.Parse(typeof(EYesNo), reader["roomOccupied"].ToString()),
                        outdoorTemperature = Convert.ToSingle(reader["outdoorTemperature"]),
                        outdoorHumidity = Convert.ToSingle(reader["outdoorHumidity"])
                    };
                    WholeDataList.Add(data);
                }
            }
            return WholeDataList;
        }

        public DataList DataDetail(int idx)
        {
            DataList data = new DataList();
            cmd.CommandText = $"SELECT {ColumnToString(false)} FROM {DBTable} WHERE idx = {idx};";
            cmd.Connection = conn;
            cmd.CommandType = CommandType.TableDirect;

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    data.idx = Convert.ToInt32(reader["idx"]);
                    data.co2 = Convert.ToSingle(reader["co2"]);
                    data.co2Approx = (EApproxData)Enum.Parse(typeof(EApproxData), reader["co2Approx"].ToString());
                    data.gas = Convert.ToSingle(reader["gas"]);
                    data.gasApprox = (EApproxData)Enum.Parse(typeof(EApproxData), reader["gasApprox"].ToString());
                    data.temperature = Convert.ToSingle(reader["temperature"]);
                    data.temperatureApprox = (EApproxData)Enum.Parse(typeof(EApproxData), reader["temperatureApprox"].ToString());
                    data.humidity = Convert.ToSingle(reader["humidity"]);
                    data.humidityApprox = (EApproxData)Enum.Parse(typeof(EApproxData), reader["humidityApprox"].ToString());
                    data.roomOccupied = (EYesNo)Enum.Parse(typeof(EYesNo), reader["roomOccupied"].ToString());
                    data.outdoorTemperature = Convert.ToSingle(reader["outdoorTemperature"]);
                    data.outdoorHumidity = Convert.ToSingle(reader["outdoorHumidity"]);
                }
            }
            return data;
        }

        public bool UpdateData(DataList data)
        {
            if (DoConnSQL() == false)
            {
                return false;
            }

            cmd.CommandText = $"UPDATE {DBTable} SET {DataListToString(data)} WHERE idx = {data.idx};";
            cmd.Connection = conn;
            cmd.CommandType = CommandType.TableDirect;
            cmd.ExecuteNonQuery();
            return true;
        }

        public int TotalCount()
        {
            int total = 0;

            if (DoConnSQL() == false)
            {
                return -1;
            }
            cmd.CommandText = "SELECT COUNT(*) FROM room";
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader != null && reader.Read())
                {
                    total = Convert.ToInt32(reader["COUNT(*)"]); ;
                }
            }
            return total;
        }
    }
}
