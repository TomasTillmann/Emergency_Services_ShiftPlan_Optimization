using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

class Program
{
    private static readonly string apiKey = File.ReadAllText("/home/tom/GoogleAPI/DistanceMatrixAPIkey.txt");

    static async Task Main(string[] args)
    {
        string address = "Prague";
        string geocodeUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={address}&key={apiKey}";

        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage geocodeResponse = await client.GetAsync(geocodeUrl);
            if (geocodeResponse.IsSuccessStatusCode)
            {
                string geocodeContent = await geocodeResponse.Content.ReadAsStringAsync();
                JObject geocodeData = JObject.Parse(geocodeContent);

                if (geocodeData["status"].ToString() == "OK")
                {
                    string placeId = geocodeData["results"][0]["place_id"].ToString();
                    string placeDetailsUrl = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={placeId}&key={apiKey}";

                    HttpResponseMessage placeDetailsResponse = await client.GetAsync(placeDetailsUrl);
                    if (placeDetailsResponse.IsSuccessStatusCode)
                    {
                        string placeDetailsContent = await placeDetailsResponse.Content.ReadAsStringAsync();
                        JObject placeDetailsData = JObject.Parse(placeDetailsContent);

                        if (placeDetailsData["status"].ToString() == "OK")
                        {
                            JObject geometry = (JObject)placeDetailsData["result"]["geometry"];
                            Console.WriteLine(geometry.ToString());
                        }
                        else
                        {
                            Console.WriteLine("Error fetching place details: " + placeDetailsData["status"]);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Error fetching geocode data: " + geocodeData["status"]);
                }
            }
        }
    }
}
