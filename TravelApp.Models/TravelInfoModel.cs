using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelApp.Models
{
    public class TravelInfoModel
    {
        public TravelInfoModel(RootObject obj)
        {
            var rootObj = obj.rows.First().elements.First();
            TotalDistance = rootObj.distance.text;
            EstimatedTime = rootObj.duration.text;
            PickUpPlace = obj.origin_addresses.First();
            DestPlace = obj.destination_addresses.First();
        }

        public string TotalDistance { get; set; }
        public string EstimatedTime { get; set; }
        public string PickUpPlace { get; set; }
        public string DestPlace { get; set; }
    }
}
