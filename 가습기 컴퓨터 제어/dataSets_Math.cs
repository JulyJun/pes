using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APICommon
{
    // 요약:
    //          기상청 격자 <-> 위경도 변경
    class GPSMath
    {
        // LCC DFS 좌표변환을 위한 기초 자료
        public const double RE = 6371.00877;                 // 지구 반경(km)
        public const double GRID = 5.0;                           // 격자 간격(km)
        public const double SouthLatitude1 = 30.0;     // 투영 위도1(degree)
        public const double SouthLatitude2 = 60.0;    // 투영 위도2(degree)
        public const double OriginLongitude = 126.0;   // 기준점 경도(degree)
        public const double OriginLatitude = 38.0;       // 기준점 위도(degree)
        public const double OriginX = 43.0;                    // 기준점 X좌표(GRID)
        public const double OriginY = 136;                      // 기준점 Y좌표(GRID)
        public const double DEGtoRAD = Math.PI / 180.0;
        public const double RADtoDEG = 180.0 / Math.PI;
        
        public struct SPositionXY
        { 
            public double latitude;
            public double longitude;
        }


        public GPSMath() { }

        public SPositionXY ConvertGpsToPositions(SPositionXY positions, string code = "toGrid")
        {
            double roundedEarth = RE / GRID;
            double slat1 = SouthLatitude1 * DEGtoRAD;
            double slat2 = SouthLatitude2 * DEGtoRAD;
            double olon = OriginLongitude* DEGtoRAD;
            double olat = OriginLatitude* DEGtoRAD;
            double sn = Math.Tan(Math.PI * 0.25f + slat2 * 0.5f) / Math.Tan(Math.PI * 0.25f + slat1 * 0.5f);
            double sf = Math.Tan(Math.PI * 0.25f + slat1 * 0.5f);
            double ro = Math.Tan(Math.PI * 0.25f + olat * 0.5f);
            SPositionXY positionXY= new SPositionXY();

            double ra;
            double theta;

            sn = Math.Log(Math.Cos(slat1) / Math.Cos(slat2)) / Math.Log(sn);
            sf = Math.Pow(sf, sn) * Math.Cos(slat1) / sn;
            ro = roundedEarth * sf / Math.Pow(ro, sn); 

            if(code.Equals("toGrid"))
            {
                ra = Math.Tan(Math.PI * 0.25f + positions.latitude * DEGtoRAD * 0.5f);
                ra = roundedEarth * sf / Math.Pow(ra, sn);
                theta = positions.longitude * DEGtoRAD - olon;
                if(theta > Math.PI)
                {
                    theta -= 2.0 * Math.PI;
                }
                if(theta < -Math.PI)
                {
                    theta += 2.0 * Math.PI;
                }

                theta *= sn;
                positionXY.latitude = Math.Floor(ra * Math.Sin(theta) + OriginX + .5);
                positionXY.longitude = Math.Floor(ro - ra * Math.Cos(theta) + OriginY + .5);
            }
            else
            {
                double xn = positions.latitude - OriginX;
                double yn = ro - positions.longitude + OriginY;
                ra = Math.Sqrt(xn* xn + yn * yn);
                if(sn < 0.0)
                {
                    ra = -ra;
                }
                double alat = Math.Pow((roundedEarth * sf / ra), (1.0) / sn);
                alat = 2.0*Math.Atan(alat) - Math.PI * .5;

                if(Math.Abs(xn) <= 0.0)
                {
                    theta = 0.0;
                }
                else
                {
                    if(Math.Abs(yn) <= 0.0)
                    {
                        theta = Math.PI * .5;
                        if(xn < 0.0)
                        {
                            theta = -theta;
                        }
                    }
                    else
                    {
                        theta = Math.Atan2(xn, yn);
                    }
                }
                double alon = theta / sn + olon;
                positionXY.latitude = alat * RADtoDEG;
                positionXY.longitude = alon * RADtoDEG;
            }
            return positionXY;
        }
    }
}
