using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using TravelApp.Models;

namespace TravelApp.Helpers
{
    public class ServiceHelper
    {
        public static TravelInfoModel GetInfo(GetDistanceModel model)
        {
            using (var client = new HttpClient())
            {

                client.BaseAddress = new Uri("http://travelapp.xivtech.com/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.PostAsJsonAsync("api/map/GetBestTravelDistance", model).Result;
                if (response.IsSuccessStatusCode)
                {
                    RootObject obj = response.Content.ReadAsAsync<RootObject>().Result;
                    TravelInfoModel returnModel = new TravelInfoModel(obj);
                    return returnModel;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
