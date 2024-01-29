using System;
using System.IO;
using System.Net;
using System.Xml;
using Google.Protobuf.WellKnownTypes;
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
        public string ForcastTime { get { return _fctTime; } set { _fctTime = value; } }
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
            TMP,                                       // 기온
            RN1,                                        // 1시간 강수량
            SKY,                                       // 하늘상태
            UUU,                                       // 동서바람성분
            VVV,                                       // 남북바람성분
            REH,                                       // 습도
            PTY,                                       // 강수형태
            LGT,                                       // 낙뢰
            VEC,                                       // 풍향
            WSD,                                      // 풍속
            PCP,                                      // 강수
            SNO                                       // 적설
        }
        // TODO: 데이터에 뭐가 들어있는지 채울것
        public enum EPCPStatus
        {
            강수없음
        }
        // TODO: 데이터에 뭐가 들어있는지 채울것
        public enum ESNOStatus
        {
            적설없음
        }
        public enum ESKYStatus
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
        private string CurrentBaseTime;
        private DateTime _dateTime;
        private string _fctTime;

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

        public bool baseTimeSet(DateTime currentTime)
        {
            if (currentTime == null)
            {
                // unsuccessful return
                return false;
            }
            int numMax, index = 0, closestTime = int.MinValue;
            string currTime = "";
            DateTime = currentTime;
            currTime = currentTime.ToString("HHmm");
            // 현재시간0100
            // 베이스 전날 23시여야함 여기서만 예외처리
            for(int i = 0; i < _basetimeSelect.Length; i++)
            {
                numMax = int.Parse(_basetimeSelect[i]) - int.Parse(currTime);
                if(numMax <= 0 && numMax > closestTime)
                {
                    closestTime = numMax;
                    index = i;
                }
            }
            CurrentBaseTime = _basetimeSelect[index];
            return true;
        }
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
            
            return Url + $"?ServiceKey={ServiceKey}&numOfRows={NumberOfRow}&pageNo={PageNo}&dataType=XML&base_date={Date}&base_time={CurrentBaseTime}&nx={tupleGrid.latitude}&ny={tupleGrid.longitude}";
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
        // 현재시각 새벽 1시에 2시 베이스 타임일시에 새벽 3시부터 알려줌
        public string GrabNearestTimeValFromAPI(string query, string categoryValue)
        {
            string value = "";
            string nearestTimeStr = "";
            string nearestDay = "";
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(query);
            XmlNode xmlNormalState = xd["response"]["header"]["resultMsg"];
            if (xmlNormalState.InnerText != "NORMAL_SERVICE")
            {
                // 쿼리 에러
                return xmlNormalState.InnerText;
            }            

            XmlNode xn = xd["response"]["body"]["items"];            
            nearestTimeStr= HourPicker(DateTime);
            if(nearestTimeStr.Equals("0000"))
            {
                nearestDay = (int.Parse(Date) + 1).ToString();
            }
            else
            {
                nearestDay = Date;
            }
            for (int index = 0; index < xn.ChildNodes.Count; index++)
            {
                if (xn.ChildNodes[index]["category"].InnerText.Equals(categoryValue) 
                    && xn.ChildNodes[index]["fcstDate"].InnerText.Equals(nearestDay)
                    && xn.ChildNodes[index]["fcstTime"].InnerText.Equals(nearestTimeStr))
                {
                    value = xn.ChildNodes[index]["fcstValue"].InnerText;
                    ForcastTime = $"{xn.ChildNodes[index]["fcstDate"].InnerText}, {xn.ChildNodes[index]["fcstTime"].InnerText}";
                }
            }

             return value;
        }

        // <summary>
        // 0 반환시 눈/비 없음, 그 외 횟수로 반환, -1일시 접속불가
        // </summary>
        public int FcstRainCount(string query)
        {
            string nearestTimeStr = "";
            string nearestDay = "";
            int rain = 0;
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(query);
            XmlNode xmlNormalState = xd["response"]["header"]["resultMsg"];
            if (xmlNormalState.InnerText != "NORMAL_SERVICE")
            {
                // 쿼리 에러
                return -1;
            }

            XmlNode xn = xd["response"]["body"]["items"];
            nearestTimeStr = HourPicker(DateTime);
            if (nearestTimeStr.Equals("0000"))
            {
                nearestDay = (int.Parse(Date) + 1).ToString();
            }
            else
            {
                nearestDay = (int.Parse(Date)).ToString();
            }

            for (int index = 0; index < xn.ChildNodes.Count; index++)
            {
                if (xn.ChildNodes[index]["category"].InnerText.Equals("PTY")
                    && xn.ChildNodes[index]["fcstDate"].InnerText.Equals(nearestDay)
                    && xn.ChildNodes[index]["fcstValue"].InnerText.Equals("1"))
                {
                    rain++;
                }
            }
            return rain;
        }

        public ESKYStatus ReadWeatherStatus(string SKYinput)
        {
            ESKYStatus status;
            switch( SKYinput )
            {
                case "1":
                    status = ESKYStatus.SUNNY;
                    break;
                case "3":
                    status = ESKYStatus.CLOUDY;
                    break;
                case "4":
                    status = ESKYStatus.FOGGY;
                    break;
                default:
                    status = ESKYStatus.NODATA; 
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
        public string HourPicker(DateTime now)
        {
            string str = now.ToString("HH");
            int toNumber = (int.Parse(str) + 1) * 100;
            str = toNumber.ToString("D4");
            if (str.Equals("2400"))
            {
                str = "0000";
            }
            return str;
        }
        // END OF CLASS
    }
}
