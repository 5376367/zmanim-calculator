using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Working_With_APIs.Models
{
    public class Zman
    {
        public string Name { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public int TimeZone { get; set; }
        public int DayOfYear { get; set; }
        public double Alos { get; set; }
        public double Netz { get; set; }
        public double Shkiya { get; set; }
        public double Tzeis { get; set; }
        public string AlosSixteenPointOne { get; set; }
        public string Sunrise { get; set; }
        public string Sunset { get; set; }
        public string TzeisEightPointFive { get; set; }
        public double ShaZmanisGra { get; set; }
        public double ShaZmanisMGA { get; set; }
        public string ShmaGra { get; set; }
        public string ShmaMagen { get; set; }
        public string Chatzos { get; set; }
        public string Plag { get; set; }
        public string GMTTZ { get; set; }
        public DateTime Date { get; set; }


        //constructor
        public Zman(string name, double longitude, double latitude, int timeZone, DateTime date)
        {
            Name = name;
            Longitude = longitude;
            Latitude = latitude;
            TimeZone = timeZone;
            Date = date;
            //get the day of the year of chosen date
            DayOfYear = date.DayOfYear; ;
            //Sunrise, sunset alos and tzeis
            Alos = SunriseOrSunset(DayOfYear, true, 16.1);
            Netz = SunriseOrSunset(DayOfYear, true, 0.8333333333);
            Shkiya = SunriseOrSunset(DayOfYear, false, 0.8333333333);
            Tzeis = SunriseOrSunset(DayOfYear, false, 8.5);
            //sha'os zmaniyos
            ShaZmanisGra = (Shkiya - Netz) / 12;
            ShaZmanisMGA = (Tzeis - Alos) / 12;
            // zmanim in string format
            AlosSixteenPointOne = TranslateTime(Alos);
            Sunrise = TranslateTime(Netz);
            Sunset = TranslateTime(Shkiya);
            TzeisEightPointFive = TranslateTime(Tzeis);
            ShmaGra = TranslateTime(Netz + ShaZmanisGra * 3);
            ShmaMagen = TranslateTime(Alos + ShaZmanisMGA * 3);
            Chatzos = TranslateTime(Netz + ShaZmanisGra * 6);
            Plag = TranslateTime(Netz + ShaZmanisGra * 10.75);
            //time zone in string format
            switch (TimeZone)
            {
                case int x when (x > 0):
                    GMTTZ = "GMT+" + TimeZone;
                    break;
                case int x when (x < 0):
                    GMTTZ = "GMT" + TimeZone;
                    break;
                default:
                    GMTTZ = "GMT";
                    break;
            }
        }
        //times are returned in decimal notation, must be hours and minutes
        public string TranslateTime(double inTime)
        {
            double hours = (Math.Floor(inTime) % 24);
            double mins = Math.Round(60 * (inTime - Math.Floor(inTime)));
            string minsString = mins.ToString();
            if (mins < 10) minsString = "0" + minsString;
            return hours + ":" + minsString;
        }
        public double SunriseOrSunset(int day, bool sunrise, double degreesToAdd)
        {
            //algorithm taken from https://gist.github.com/Tafkas/4742250
            var zenith = 90 + degreesToAdd; //you must add 0.83333333 for sunset and sunrise
            var D2R = Math.PI / 180;
            var R2D = 180 / Math.PI;

            // convert the longitude to hour value and calculate an approximate time
            var lnHour = Longitude / 15;
            double t = 0;
            if (sunrise)
            {
                t = day + ((6 - lnHour) / 24);
            }
            else
            {
                t = day + ((18 - lnHour) / 24);
            };

            //calculate the Sun's mean anomaly
            double M = (0.9856 * t) - 3.289;

            //calculate the Sun's true longitude
            double L = M + (1.916 * Math.Sin(M * D2R)) + (0.020 * Math.Sin(2 * M * D2R)) + 282.634;
            if (L > 360)
            {
                L = L - 360;
            }
            else if (L < 0)
            {
                L = L + 360;
            };

            //calculate the Sun's right ascension
            double RA = R2D * Math.Atan(0.91764 * Math.Tan(L * D2R));
            if (RA > 360)
            {
                RA = RA - 360;
            }
            else if (RA < 0)
            {
                RA = RA + 360;
            };

            //right ascension value needs to be in the same qua
            double Lquadrant = (Math.Floor(L / (90))) * 90;
            double RAquadrant = (Math.Floor(RA / 90)) * 90;
            RA = RA + (Lquadrant - RAquadrant);

            //right ascension value needs to be converted into hours
            RA = RA / 15;

            //calculate the Sun's declination
            double sinDec = 0.39782 * Math.Sin(L * D2R);
            double cosDec = Math.Cos(Math.Asin(sinDec));

            //calculate the Sun's local hour angle
            double cosH = (Math.Cos(zenith * D2R) - (sinDec * Math.Sin(Latitude * D2R))) / (cosDec * Math.Cos(Latitude * D2R));
            double H;
            if (sunrise)
            {
                H = 360 - R2D * Math.Acos(cosH);
            }
            else
            {
                H = R2D * Math.Acos(cosH);
            };
            H = H / 15;

            //calculate local mean time of rising/setting
            double T = H + RA - (0.06571 * t) - 6.622;

            //adjust back to UTC
            double UT = T - lnHour;
            if (UT > 24)
            {
                UT = UT - 24;
            }
            else if (UT < 0)
            {
                UT = UT + 24;
            }

            //convert UT value to local time zone of latitude/longitude
            double localT = UT + TimeZone;

            //convert to Milliseconds
            return localT;





        }
    }
}
