using APICommon;
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
using static System.Net.Mime.MediaTypeNames;

namespace 가습기_컴퓨터_제어
{
    public partial class Form1 : Form
    {
        const int HOUR = 216000000;
        private WeatherApiCommon dataSets;
        private string dataQuery;
        private string title;
        private int currentImageIndex = 0;
        private System.Drawing.Image[] images = { Properties.Resources.kommiMad, Properties.Resources.kommiNelm, Properties.Resources.kommiHappy, Properties.Resources.kommiCry };
        private Timer timer;
        private DateTime startTime;
        public Form1()
        {
            InitializeComponent();
            timer = new Timer();
            //timer.Interval = 108000000;
            timer.Interval = 1000;
            timer.Start();
            timer.Tick += timer1_Tick;
            
            //MessageBox.Show("Hello World", "hi", MessageBoxButtons.OK, MessageBoxIcon.Question);
        }

        private void InitializeValue()
        {
            const int dataLength = 120;
            Series series = new Series();
            ChartArea chartArea = new ChartArea();
            dataSets = new WeatherApiCommon();
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
                series.Points.AddXY(axisX, weightedData(axisX));
            }
            chart1.ChartAreas.Add(chartArea);
            chart1.Series.Add(series);

            chartArea.AxisX.ScaleView.ZoomReset();
            chartArea.AxisX.MajorGrid.Enabled = false;
            chartArea.AxisY.MajorGrid.Enabled = false;
            chartArea.AxisX.LabelStyle.Interval = 10;
            chartArea.AxisX.ScaleView.Zoom(0, 30);
            chartArea.AxisY.ScaleView.Zoom(-1, 2);

            textBox1.Text = dataSets.ExtractXMLQuery(dataQuery, WeatherApiCommon.EForcastCode.REH.ToString()) + "%";
            textBox3.Text = dataSets.ExtractXMLQuery(dataQuery, WeatherApiCommon.EForcastCode.TMP.ToString()) + "°C";
            WeatherApiCommon.EWeatherStatus status = dataSets.ReadWeatherStatus(dataSets.ExtractXMLQuery(dataQuery, WeatherApiCommon.EForcastCode.SKY.ToString()));
            textBox6.Text = status.ToString();
        }

        private double weightedData(int humid)
        {
            if (humid < 55)
            {
                return 1;
            }
            else if (humid >= 75)
            {

                return 0;

            }
            else
            {
                return humid > 0 ? 1 : 0;
            }
        }
        private void emoteImg_DoubleClick(object sender, EventArgs e)
        {
            currentImageIndex = ( currentImageIndex + 1 ) % images.Length;
            emoteImg.Image = images[currentImageIndex];
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeValue();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Text = title + $" (경과시간: {( DateTime.MinValue + ( DateTime.Now - startTime ) ).ToString("HH:mm:ss")})";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = dataQuery;
        }
    }
}
