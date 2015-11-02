using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelApp.Models
{
    public class ListViewModel
    {
        public ListViewModel()
        {
            PickUpPlace = new PlaceViewModel();
            DestinationPlace = new PlaceViewModel();
        }

        public string Name { get; set; }
        public PlaceViewModel PickUpPlace { get; set; }
        public PlaceViewModel DestinationPlace { get; set; }
        public DateTime TravelDateTime { get; set; }
        public string EmailAddress { get; set; }
        public int Id { get; set; }

    }

    public class PlaceViewModel
    {
        public int Id { get; set; }
        public string PlaceName { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}
