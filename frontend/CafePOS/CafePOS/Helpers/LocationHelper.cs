using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;


namespace CafePOS.Helpers
{
    public class LocationHelper
    {
        public async Task<Geoposition?> GetCurrentLocationAsync()
        {
            try
            {
                var accessStatus = await Geolocator.RequestAccessAsync().AsTask();
                if (accessStatus != GeolocationAccessStatus.Allowed)
                    return null;

                var geolocator = new Geolocator { DesiredAccuracyInMeters = 100 };
                return await geolocator.GetGeopositionAsync().AsTask();
            }
            catch
            {
                return null;
            }
        }
    }
}
