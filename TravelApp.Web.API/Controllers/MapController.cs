using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using TravelApp.Web.API.Models;

namespace TravelApp.Web.API.Controllers
{
    [RoutePrefix("api/map")]
    public class MapController : ApiController
    {
        [HttpPost]
        [AllowAnonymous]
        [Route("GetAirDistance")]
        public IHttpActionResult GetAirDistance(LocationModel model)
        {
            try
            {
                var R = 6378137;
                var l1lat = Convert.ToDouble(model.Location1Latitude);
                var l1long = Convert.ToDouble(model.Location1Longitude);
                var l2lat = Convert.ToDouble(model.Location2Latitude);
                var l2long = Convert.ToDouble(model.Location2Longitude);
                var dLat = Rad(l2lat - l1lat);
                var dLong = Rad(l2long - l1long);
                var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(Rad(l1lat)) * Math.Cos(Rad(l2lat)) * Math.Sin(dLong / 2) * Math.Sin(dLong / 2);
                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                var d = R * c;
                return Ok(d / 1000);
            }
            catch (DivideByZeroException)
            {
                return BadRequest("Please dont enter 0 values");
            }
            catch (InvalidCastException)
            {
                return BadRequest("Please enter valid double values");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("GetBestTravelDistance")]
        public async Task<IHttpActionResult> GetBestTravelDistance(LocationModel model)
        {
            try
            {
                var l1lat = model.Location1Latitude;
                var l1long = model.Location1Longitude;
                var l2lat = model.Location2Latitude;
                var l2long = model.Location2Longitude;
                var _address = "https://maps.googleapis.com/maps/api/distancematrix/json?origins=" + l1lat + "," + l1long + "&destinations=" + l2lat + "," + l2long + "&key=AIzaSyBsgN6RY1RbVU51gNq8RwwQj7ro-TaNMO0";
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(_address);
                string responseBody = await response.Content.ReadAsStringAsync();

                // Check that response was successful or throw exception
                response.EnsureSuccessStatusCode();

                JObject json = JObject.Parse(responseBody);
                return Ok(json);
            }
            catch (Exception)
            {
                return BadRequest("Some error occured during processing your request");
            }
        }


        private double Rad(double value)
        {
            return value * Math.PI / 180;

        }
    }
}
