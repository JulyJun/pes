using APICommon;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO.Ports;
using static System.Net.Mime.MediaTypeNames;
using DBCommon;
using static DBCommon.MysqlDB;
using pAlgorithms;

namespace 가습기_컴퓨터_제어
{
    public partial class Form1 : Form
    {
        const int HOUR = 216000000;
        private WeatherApiCommon dataSets;
        private MysqlDB myDb;
        private Algorithms calc = new Algorithms();
        private string temp_g;
        private string humid_g;
        private string dataQuery;
        private string title;
        private int currentImageIndex = 0;
        private System.Drawing.Image[] images = { Properties.Resources.kommiMad, Properties.Resources.kommiNelm, Properties.Resources.kommiHappy, Properties.Resources.kommiCry };
        private Timer timer;
        private DateTime startTime;
        private int SQLSavecounter = 0;
        //private string Conn = "Server=localhost;Database=HumidTempBoard;Uid=root;Pwd=root;";
        private enum EEmote 
        {
            쾌적함,
            좋음,
            보통,
            우울
        }
        public Form1()
        {
            InitializeComponent();
            timer = new Timer();
            //timer.Interval = 108000000;
            timer.Interval = 1000;
            timer.Start();
            timer.Tick += timer1_Tick;
            //this.serialPort1.Open();
            if (serialPort1.IsOpen)
            {
                receiveSerialTBox.Text = $"{DateTime.Now.ToString("HH:mm:ss")} > 연결되었습니다.\n";
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (SQLSavecounter % (30 * 60) == 0)
            {
                // 데이터베이스에 추가함
                // TODO: 나중에 바꿀것
                DataList dl = new DataList();
                dl.co2 = Convert.ToSingle("30");
                dl.gas = Convert.ToSingle("30");
                dl.temperature = Convert.ToSingle("30");
                dl.humidity = Convert.ToSingle("30");
                dl.roomOccupied = EYesNo.Y;
                dl.outdoorTemperature = Convert.ToSingle(temp_g.ToString());
                dl.outdoorHumidity = Convert.ToSingle(humid_g.ToString());
                myDb.DataCollections = dl;
                myDb.InsertData();
                SQLSavecounter = 0;
            }

            SQLSavecounter++;

            this.Text = title + $" (경과시간: {(DateTime.MinValue + (DateTime.Now - startTime)).ToString("HH:mm:ss")})";
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeValue();
        }

        private void InitializeValue()
        {
            const int dataLength = 120;
            Series series = new Series();
            ChartArea chartArea = new ChartArea();
            myDb = new MysqlDB();
            dataSets = new WeatherApiCommon();
            dataSets.baseTimeSet(DateTime.Now);
            dataSets.HourPicker(DateTime.Now);
            dataQuery = dataSets.ReqestApi();
            title = this.Text;

            startTime = new DateTime();
            startTime = DateTime.Now;

            chart1.Series.Clear();
            chart1.ChartAreas.Clear();

            chart2.Series.Clear();
            chart2.ChartAreas.Clear();

            series.ChartType = SeriesChartType.Spline;
            series.IsValueShownAsLabel = true;
            series.IsVisibleInLegend = false;

            for (int axisX = 0; axisX < dataLength; axisX++)
            {
                series.Points.AddXY(axisX, calc.weightedData(axisX));
            }
            chart1.ChartAreas.Add(chartArea);
            chart1.Series.Add(series);

            chartArea.AxisX.ScaleView.ZoomReset();
            chartArea.AxisX.MajorGrid.Enabled = false;
            chartArea.AxisY.MajorGrid.Enabled = false;
            chartArea.AxisX.LabelStyle.Interval = 10;
            chartArea.AxisX.ScaleView.Zoom(0, 30);
            chartArea.AxisY.ScaleView.Zoom(-1, 2);

            temp_g = dataSets.GrabNearestTimeValFromAPI(dataQuery, WeatherApiCommon.EForcastCode.TMP.ToString());
            humid_g = dataSets.GrabNearestTimeValFromAPI(dataQuery, WeatherApiCommon.EForcastCode.REH.ToString());
            textBox1.Text = humid_g  + "%";
            textBox3.Text = temp_g + "°C";
            WeatherApiCommon.ESKYStatus status = dataSets.ReadWeatherStatus(dataSets.GrabNearestTimeValFromAPI(dataQuery, WeatherApiCommon.EForcastCode.SKY.ToString()));
            textBox5.Text = status.ToString();
            int rainCheck = dataSets.FcstRainCount(dataQuery);
            textBox6.Text = rainCheck > 0 ? "비 예상" : "비 예보없음";
            dateTime_label.Text = $"{dataSets.Date.Substring(0, 4)}년 {dataSets.Date.Substring(4, 2)}월 {dataSets.Date.Substring(6,2)}일 { dataSets.DateTime.DayOfWeek.ToString() }" ;
            groupBox2.Text = $"{dataSets.ForcastTime}시 날씨요약";
        }

        private void UpdateWeather()
        {
            dataSets.baseTimeSet(DateTime.Now);
            dataSets.HourPicker(DateTime.Now);
            dataQuery = dataSets.ReqestApi();
        }

        private void emoteImg_DoubleClick(object sender, EventArgs e)
        {
            currentImageIndex = ( currentImageIndex + 1 ) % images.Length;
            emoteImg.Image = images[currentImageIndex];
        }

        //  *********************************************************************
        //  *                                                                                                                       *
        //  *                                               test Area                                                      *
        //  *                                                                                                                       *
        //  *********************************************************************
        private void button1_Click(object sender, EventArgs e)
        {
            //richTextBox1.Text = dataQuery;
            richTextBox1.Text = dataSets.GetUrl();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            richTextBox2.Text = myDb.TotalCount().ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(textBox7.Text.Equals("") || textBox8.Text.Equals("") || textBox9.Text.Equals("") || textBox10.Text.Equals(""))
            {
                MessageBox.Show("데이터를 채워주세요", "확인", MessageBoxButtons.OK);
                return;
            }
            else
            {
                DataList dl= new DataList();
                dl.co2 = Convert.ToSingle(textBox7.Text);
                dl.gas = Convert.ToSingle(textBox8.Text);
                dl.temperature = Convert.ToSingle(textBox9.Text);
                dl.humidity= Convert.ToSingle(textBox10.Text);
                dl.roomOccupied = EYesNo.Y;
                dl.outdoorTemperature= Convert.ToSingle(temp_g);
                dl.outdoorHumidity= Convert.ToSingle(humid_g);
                myDb.DataCollections = dl;
                myDb.InsertData();
                textBox2.Text = textBox10.Text; // 습도
                textBox4.Text = textBox9.Text;  // 온도
                textBox11.Text = myDb.DataCollections.co2Approx.ToString();
                textBox12.Text = myDb.DataCollections.gasApprox.ToString();
                textBox13.Text = myDb.DataCollections.roomOccupied.ToString();
                richTextBox2.Text = myDb.ColumnToString(false) + "\n";
                richTextBox2.Text += myDb.ColumnDataTravelsal();

                // emote
                double humid;
                if (double.TryParse(textBox2.Text, out humid))
                {
                    double result = calc.weightedData(humid);
                    if(result == 0)
                    {
                        status_label.Text = EEmote.우울.ToString();
                        emoteImg.Image = images[3];
                    }
                    else if(result == 1)
                    {
                        status_label.Text = EEmote.쾌적함.ToString();
                        emoteImg.Image = images[2];                        
                    }
                    else
                    {
                        status_label.Text = EEmote.보통.ToString();
                        emoteImg.Image = images[1];
                    }
                    richTextBox1.Text = result.ToString();
                }
            }
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            this.Invoke(new EventHandler(SerialReceived));
        }
        private void SerialReceived(object s, EventArgs e)
        {
            string receivedData = serialPort1.ReadLine();
            receiveSerialTBox.Text += $"{DateTime.Now.ToString("HH:mm:ss")} > {receivedData}";
        }
        private void sendSerial_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyValue == 13)
            {
                serialPort1.Write($"{sendSerialTBox.Text}\n");
                sendSerialTBox.Text = "";
            }
        }
    }
}
