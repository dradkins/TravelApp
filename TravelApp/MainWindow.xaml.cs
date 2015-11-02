using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TravelApp.Models;
using Microsoft.Maps.MapControl.WPF;
using TravelApp.Helpers;
using TravelApp.EF;
using System.Data.Entity.Core.Objects;
using System.Threading.Tasks;

namespace TravelApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TravelAppEntities dbEntityObject = new TravelAppEntities();
        //TRAVEL_APPEntities dbEntityObject = new TRAVEL_APPEntities();
        string WindowTitle = "Travel App";
        int SelectedId;

        public MainWindow()
        {
            InitializeComponent();

            this.Title = WindowTitle;
            dtpPickUpDateTime.Value = DateTime.Now;

            UpdateButtons(true, false);
            PopulateComboBox();
            PopulateListView();
            SetMap();
        }

        private void btnCheckDistance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RemovePointsFromMap();
                this.Cursor = Cursors.Wait;
                if (!Validate(true))
                    return;

                var pickupCity = cmbPickUpCity.SelectedItem as City;
                var destCity = cmbDestinationCity.SelectedItem as City;

                AddPointOnMap(Convert.ToDouble(pickupCity.Latitude), Convert.ToDouble(pickupCity.Longitude));
                AddPointOnMap(Convert.ToDouble(destCity.Latitude), Convert.ToDouble(destCity.Longitude));

                var model = new GetDistanceModel()
                {
                    Location1Latitude = pickupCity.Latitude,
                    Location1Longitude = pickupCity.Longitude,
                    Location2Latitude = destCity.Latitude,
                    Location2Longitude = destCity.Longitude
                };

                var data = ServiceHelper.GetInfo(model);
                if (data == null)
                {
                    MessageBox.Show("Unable to get info at this time, please try again later", Title, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    SetTravelInfo(data);
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(this, exp.Message, "Error :" + Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void btnBook_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                if (!Validate())
                    return;

                ObjectParameter id = new ObjectParameter("Id", 1);
                dbEntityObject.AddTravelRequest(txtName.Text, Convert.ToInt32(cmbPickUpCity.SelectedValue), Convert.ToInt32(cmbDestinationCity.SelectedValue), Convert.ToDateTime(dtpPickUpDateTime.Text), txtEmail.Text, id);
                ClearValues();
                PopulateListView();
                MessageBox.Show(this, "Booking compeleted successfully.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                RemovePointsFromMap();
            }
            catch (Exception exp)
            {
                MessageBox.Show(this, exp.Message, "Error :" + Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                if (!Validate())
                    return;

                dbEntityObject.UpdateTravelRequest(txtName.Text, Convert.ToInt32(cmbPickUpCity.SelectedValue), Convert.ToInt32(cmbDestinationCity.SelectedValue), Convert.ToDateTime(dtpPickUpDateTime.Value), txtEmail.Text, SelectedId);
                ClearValues();
                PopulateListView();
                MessageBox.Show(this, "Booking updated successfully.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateButtons(true, false);
                RemovePointsFromMap();
            }
            catch (Exception exp)
            {
                MessageBox.Show(this, exp.Message, "Error :" + Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ClearValues();
            UpdateButtons(true, false);
            RemovePointsFromMap();
        }

        private void btnDeleteRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                Button b = sender as Button;
                ListViewModel record = b.CommandParameter as ListViewModel;
                if (MessageBox.Show("Are you sure to delete this record", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    dbEntityObject.DeleteTravelRequest(record.Id);
                    PopulateListView();
                }
                ClearValues();
                RemovePointsFromMap();
                UpdateButtons(true, false);
            }
            catch (Exception exp)
            {
                MessageBox.Show(this, exp.Message, "Error :" + Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void btnEditRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                Button b = sender as Button;
                ListViewModel record = b.CommandParameter as ListViewModel;
                cmbPickUpCity.SelectedValue = record.PickUpPlace.Id;
                cmbDestinationCity.SelectedValue = record.DestinationPlace.Id;
                txtEmail.Text = record.EmailAddress;
                txtName.Text = record.Name;
                dtpPickUpDateTime.Value = record.TravelDateTime;
                SelectedId = record.Id;
                UpdateButtons(false, true);
                btnCheckDistance_Click(null, null);
            }
            catch (Exception exp)
            {
                MessageBox.Show(this, exp.Message, "Error :" + Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }


        #region Helpers

        private void UpdateButtons(bool bookButton, bool updateButton)
        {
            btnBook.Visibility = (bookButton) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            btnUpdate.Visibility = (updateButton) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }

        private void GetValues(out Dictionary<string, object> list)
        {
            try
            {
                list = new Dictionary<string, object>();

                if (lstViewDetails.SelectedIndex >= 0)
                {
                    dynamic obj = lstViewDetails.SelectedItem;
                    list.Add("Name", obj.Name);
                    list.Add("Destination", obj.DestinationPlace);
                    list.Add("PickUpPlace", obj.PickUpPlace);
                    list.Add("Date", obj.TravelDateTime);
                    list.Add("Id", obj.Id);
                    list.Add("Email", obj.EmailAddress);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void AddPointOnMap(double lat, double lng)
        {
            Pushpin pin = new Pushpin();
            pin.Location = new Location(lat, lng);
            myMap.Children.Add(pin);
        }

        private void RemovePointsFromMap()
        {
            myMap.Children.Clear();
        }

        private bool Validate(bool checkDistance = false)
        {

            if (cmbPickUpCity.SelectedIndex <= -1)
            {
                MessageBox.Show(this, "Select PickUp City.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                cmbPickUpCity.Focus();
                return false;
            }
            else if (cmbDestinationCity.SelectedIndex <= -1)
            {
                MessageBox.Show(this, "Select Destination City.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                cmbDestinationCity.Focus();
                return false;
            }
            else if (checkDistance)
                return true;

            else if (String.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show(this, "Enter Your Name.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                txtName.Focus();
                return false;
            }
            else if (!Regex.IsMatch(txtName.Text, @"^[a-zA-Z\s]+$"))
            {
                MessageBox.Show(this, "Enter Valid Name.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                txtName.Focus();
                return false;
            }
            else if (String.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show(this, "Enter Your Email Address.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                txtEmail.Focus();
                return false;
            }
            else if (!Regex.IsMatch(txtEmail.Text, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase))
            {
                MessageBox.Show(this, "Enter Valid Email Address.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                txtEmail.Focus();
                return false;
            }
            else if (String.IsNullOrWhiteSpace(dtpPickUpDateTime.Text))
            {
                MessageBox.Show(this, "Select Date.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                dtpPickUpDateTime.Focus();
                return false;
            }
            else if (!String.IsNullOrWhiteSpace(dtpPickUpDateTime.Text))
            {
                try
                {
                    var dateTime = Convert.ToDateTime(dtpPickUpDateTime.Value);
                    if (dateTime < DateTime.Now)
                    {
                        MessageBox.Show(this, "Please select date and time greater than current date and time", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                        return false;
                    }
                    return true;
                }
                catch (Exception)
                {
                    MessageBox.Show(this, "Please enter valid date and time", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                    dtpPickUpDateTime.Focus();
                    return false;
                }
            }
            else
                return true;
        }

        private void ClearValues()
        {
            cmbDestinationCity.SelectedIndex = cmbPickUpCity.SelectedIndex = -1;
            lblDistance.Content = lblDestPlace.Content = lblPickupPlace.Content = lblTravellingTime.Content = txtEmail.Text = txtName.Text = dtpPickUpDateTime.Text = "";
        }

        private void PopulateListView()
        {
            try
            {
                var query = from tr in dbEntityObject.TravelRequests
                            join cty in dbEntityObject.Cities on tr.PickupPlace equals cty.Id
                            join cty1 in dbEntityObject.Cities on tr.DestinationPlace equals cty1.Id
                            where tr.Active == true && cty.Active == true && cty1.Active == true
                            select new ListViewModel()
                            {
                                Name = tr.Name,
                                PickUpPlace = new PlaceViewModel
                                {
                                    Latitude = cty.Latitude,
                                    Longitude = cty.Longitude,
                                    Id = cty.Id,
                                    PlaceName = cty.Name
                                },
                                DestinationPlace = new PlaceViewModel
                                {
                                    Latitude = cty1.Latitude,
                                    Longitude = cty1.Longitude,
                                    Id = cty1.Id,
                                    PlaceName = cty1.Name
                                },
                                TravelDateTime = tr.TravelDateTime,
                                EmailAddress = tr.EmailAddress,
                                Id = tr.Id,
                            };

                lstViewDetails.ItemsSource = query.ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void PopulateComboBox()
        {
            cmbPickUpCity.ItemsSource = cmbDestinationCity.ItemsSource = dbEntityObject.Cities.ToList();
            cmbPickUpCity.DisplayMemberPath = cmbDestinationCity.DisplayMemberPath = "Name";
            cmbPickUpCity.SelectedValuePath = cmbDestinationCity.SelectedValuePath = "Id";
        }

        private void SetMap()
        {
            myMap.Center = new Microsoft.Maps.MapControl.WPF.Location(33.6667, 73.1667);
            myMap.ZoomLevel = 4;
        }

        private void SetTravelInfo(TravelInfoModel model)
        {
            lblDistance.Content = model.TotalDistance;
            lblTravellingTime.Content = model.EstimatedTime;
            lblPickupPlace.Content = model.PickUpPlace;
            lblDestPlace.Content = model.DestPlace;
        }

        #endregion
    }
}
