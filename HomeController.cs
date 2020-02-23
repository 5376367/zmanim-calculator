using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Working_With_APIs.Models;

namespace Working_With_APIs.Controllers
{
    public class HomeController : Controller
    {
      public ActionResult Index()
        {
            //get IP Address
            string ip = Request.UserHostAddress.ToString();
            //use when debugging 
           // ip = "213.137.64.78";
            //get city name from IP address
            var client3 = new RestClient($"http://api.ipstack.com/{ip}?access_key=87a3700cf1e0c968427d6116954fbca1&format=1");
            var request3 = new RestRequest(Method.GET);
            IRestResponse response3 = client3.Execute(request3);
            var result3 = JsonConvert.DeserializeObject<IP>(response3.Content);
            //when redirecting to action, send paramter that it is coming from IP address to warn user that address might be wrong
            return RedirectToAction("ShowZmanim", new { address = result3.city, date = DateTime.Now, fromIP = true });
        }
        public ActionResult ShowZmanim(string address, DateTime date, bool fromIP)
        {
            //get GPS coordinates from API
            var client = new RestClient($"https://google-maps-geocoding.p.rapidapi.com/geocode/json?language=en&address={address}");
            var request = new RestRequest(Method.GET);
            request.AddHeader("x-rapidapi-host", "google-maps-geocoding.p.rapidapi.com");
            request.AddHeader("x-rapidapi-key", "f2d150e770mshc2025d22c392a64p1482cejsnd9ca63413c93");
            IRestResponse response = client.Execute(request);
            var result = JsonConvert.DeserializeObject<GPS>(response.Content);
            // get Time Zone from API
            if (result.status == "OK")
            {
              
                string latitude = result.results.FirstOrDefault().geometry.location.lat.ToString();
                string longitude = result.results.FirstOrDefault().geometry.location.lng.ToString();
                //needs timestamp to check daylight savings time
                string timestamp = date.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString();
                string key = "AIzaSyAtPtUBndupOqZg0gwHKS_oZD09uz9-j7I";
                var client2 = new RestClient($"https://maps.googleapis.com/maps/api/timezone/json?location={latitude},{longitude}&timestamp={timestamp}&key={key}");
                var request2 = new RestRequest(Method.GET);
                IRestResponse response2 = client2.Execute(request2);
                var result2 = JsonConvert.DeserializeObject<TZ>(response2.Content);
                bool isDST = result2.dstOffset != 0;
                int timeZone = (result2.rawOffset +result2.dstOffset)/ 3600;
                // get zmanim based on GPS and Timezone with my model
                Zman zman = new Zman(result.results.FirstOrDefault().formatted_address, result.results.FirstOrDefault().geometry.location.lng, result.results.FirstOrDefault().geometry.location.lat, timeZone, date);
                //if address came from IP  and wasn't entered, warn user
                if (fromIP==true)
                {
                    ViewBag.FromIP = true;
                }
                if (isDST == true)
                {
                    ViewBag.IsDST = true;
                }
                return View(zman);
               
            }
            else return Content("error in GPS api");
           
        
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TryAgain(string name, DateTime date)
        {
            return RedirectToAction("ShowZmanim", new { address= name, date=date, fromIP=false });
        }
        
    }
}