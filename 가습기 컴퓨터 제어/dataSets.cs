using System;
using System.IO;
using System.Net;
using System.Xml;
using static APICommon.GPSMath;

namespace APICommon
{
    class WeatherApiCommon
    {
        readonly string[] FixedHour = { "0000", "0100", "0200", "0300", "0400", "0500", "0600", "0700", 
                                                                    "0800", "0900", "1000", "1100", "1200", "1300", "1400", "1500", 
                                                                    "1600", "1700","1800","1900", "2000","2100","2200", "2300" };
        public string Url { get { return _url; } set { _url = value; } }
        public string ServiceKey { get { return _ServiceKey; } set { _ServiceKey = value; } }
        public int PageNo { get { return _pageNo; } }
        public int NumberOfRow { get { return _numberOfRow; } }
        public DateTime DateTime { get { return _dateTime;  } set { _dateTime = value;  } }
        public string Date { get { return _date; } set { _date = value; } }
        public string Time { get { return _time; } set { _time = value; } }
        public double nx { get { return _nx; } set { _nx = value; } }
        public double ny { get { return _ny; } set { _ny = value; } }
        public string AccessType
        {
            get { return _accessType.ToString(); }
            set
            {
                if (value.Equals("GET"))
                {
                    _accessType = EAccessMethod.GET;
                }
                else
                {
                    _accessType = EAccessMethod.POST;
                }
            }
        }
        public string[] BaseTimeAll { get { return _basetimeSelect; } }
        public enum EAccessMethod
        {
            GET,
            POST
        }
        public enum EForcastCode
        { 
            TMP,                                        // 기온
            RN1,                                        // 1시간 강수량
            SKY,                                       // 하늘상태
            UUU,                                       // 동서바람성분
            VVV,                                       // 남북바람성분
            REH,                                       // 습도
            PTY,                                       // 강수형태
            LGT,                                       // 낙뢰
            VEC,                                       // 풍향
            WSD,                                      // 풍속
        }

        public enum EWeatherStatus
        {
            SUNNY,
            CLOUDY,
            FOGGY,
            NODATA
        }
        public enum EPrecipitation
        {
            NONE,
            RAIN,
            SLEET,                              // 눈비
            SNOW,
            DRIZZLE,                         // 이슬비
            FREEZING_DRIZZLE,     // 이슬눈
            HAIL,                                // 눈날림
            NODATA
        }



        private string _url = "https://apis.data.go.kr/1360000/VilageFcstInfoService_2.0/getVilageFcst";
        private string _ServiceKey = "FKxEWMvXM5W1kO%2BzI0YyvAp0Q7GuvLSJVs5CJQvzXEPSOf0UzEOoOA7fqdYmVFGK44%2F3GfysUECV%2BHiAThIttw%3D%3D";
        private readonly int _pageNo = 1;
        private readonly int _numberOfRow = 1000;
        private string _date;
        private string _time;
        private double _nx;
        private double _ny;
        private EAccessMethod _accessType;
        private string[] _basetimeSelect = { "0200", "0500", "0800", "1100", "1400", "1700", "2000", "2300"};
        private DateTime _dateTime;

        public WeatherApiCommon() { }

        //
        // 요약:
        //      api를 받는 모든 변수를 재정의합니다
        public WeatherApiCommon(string url, string key, int pageNo, int numofPage, string date, string time, double nx, double ny, EAccessMethod accessType = EAccessMethod.GET)
        {
            _url = url;
            _ServiceKey = key;
            _pageNo = pageNo;
            _numberOfRow = numofPage;
            _date = date;
            _time = time;
            _nx = nx;
            _ny = ny;
            _accessType = accessType;
        }
        ~WeatherApiCommon() { }

        public string GetUrl(EAccessMethod accessDefault = EAccessMethod.GET)
        {
            GPSMath map = new GPSMath();
            SPositionXY tupleGrid = map.ConvertGpsToPositions(new SPositionXY { latitude = 37.471139, longitude = 127.09 });
            DateTime = DateTime.Now;
            Date = DateTime.ToString("yyyyMMdd");
            //  
            //  base_time, api 생성시간을 나타냄
            //
            Time = GetTimeTraversal(1);
            
            return Url + $"?ServiceKey={ServiceKey}&numOfRows={NumberOfRow}&pageNo={PageNo}&dataType=XML&base_date={Date}&base_time={Time}&nx={tupleGrid.latitude}&ny={tupleGrid.longitude}";
        }

        public string GetTimeTraversal(int idx)
        {
            return BaseTimeAll[idx];
        }

        public string ReqestApi()
        {
            string query = GetUrl();
            WebRequest wr = WebRequest.Create(query);
            wr.Method = AccessType;
            WebResponse wrs = wr.GetResponse();
            Stream s = wrs.GetResponseStream();
            StreamReader sr = new StreamReader(s);
            return sr.ReadToEnd();
        }
        public string ExtractXMLQuery(string query, string categoryValue, int hour = 0)
        {
            string value = "";
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(query);
            XmlNode xn = xd["response"]["body"]["items"];

            if(hour < 0 || hour > 23)
            {
                hour = 0;
            }

            for(int index = 0; index < xn.ChildNodes.Count; index++)
            {
                if (xn.ChildNodes[index]["category"].InnerText.Equals(categoryValue) 
                    && xn.ChildNodes[index]["fcstTime"].InnerText.Equals(FixedHour[hour]))
                {
                    value = xn.ChildNodes[index]["fcstValue"].InnerText;
                    break;
                }
            }
            return value;
        }

        public EWeatherStatus ReadWeatherStatus(string SKYinput)
        {
            EWeatherStatus status;
            switch( SKYinput )
            {
                case "1":
                    status = EWeatherStatus.SUNNY;
                    break;
                case "3":
                    status = EWeatherStatus.CLOUDY;
                    break;
                case "4":
                    status = EWeatherStatus.FOGGY;
                    break;
                default:
                    status = EWeatherStatus.NODATA; 
                    break;
            }
            return status;
        }
        public EPrecipitation ReadPrecipitationStatus(string PTYinput)
        {
            EPrecipitation status;
            switch( PTYinput)
            {
                case "0":
                    status = EPrecipitation.NONE;
                    break;
                case "1":
                    status = EPrecipitation.RAIN;
                    break;                
                case "3":
                    status = EPrecipitation.DRIZZLE;
                    break;
                case "5":
                    status = EPrecipitation.SLEET;
                    break;
                case "6":
                    status = EPrecipitation.FREEZING_DRIZZLE;
                    break;
                case "7":
                    status = EPrecipitation.HAIL;
                    break;
                default:
                    status = EPrecipitation.NODATA; 
                    break;
            }
            return status;
        }
    }


}
