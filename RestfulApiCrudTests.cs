
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ApiAssignment
{
    [TestCaseOrderer("ApiAssignment.PriorityOrderer", "ApiAssignment")]
    public class RestfulApiCrudTests
    {
        private readonly RestClient _client;
        private static string? _createdObjectId; // store the ID of created object

        public RestfulApiCrudTests()
        {
            _client = new RestClient("https://api.restful-api.dev");
        }

        // Helper to read a value from the "data" object using multiple possible key names
        private static string? GetDataValue(JObject? dataObj, params string[] possibleKeys)
        {
            if (dataObj == null) return null;
            foreach (var key in possibleKeys)
            {
                if (dataObj.TryGetValue(key, out var token) && token != null)
                    return token.ToString();
            }
            // fallback: try a case-insensitive match ignoring spaces/underscores
            var normalized = dataObj.Properties()
                                    .Select(p => new { Name = p.Name, Norm = p.Name.Replace(" ", "").Replace("_", "").ToLower(), Value = p.Value })
                                    .FirstOrDefault(p => possibleKeys.Any(k => k.Replace(" ", "").Replace("_", "").ToLower() == p.Norm));
            return normalized?.Value.ToString();
        }

        [Fact(DisplayName = "1) Get list of all objects"),TestPriority(1)]
        public async Task Get_All_Objects_ShouldReturnList()
        {
            var request = new RestRequest("/objects", Method.Get);
            var response = await _client.ExecuteAsync(request);

            Assert.Equal(200, (int)response.StatusCode);
            Assert.False(string.IsNullOrEmpty(response.Content));

            var jsonArray = JArray.Parse(response.Content!);
            Assert.True(jsonArray.Count >= 13, "Expected at least 13 objects in the list");

            // Validate first known object
            var obj1 = jsonArray.FirstOrDefault(x => x["id"]?.ToString() == "1");
            Assert.NotNull(obj1);
            Assert.Equal("Google Pixel 6 Pro", obj1!["name"]?.ToString());
            Assert.Equal("Cloudy White", obj1!["data"]?["color"]?.ToString());
            Assert.Equal("128 GB", obj1!["data"]?["capacity"]?.ToString());

            // Validate another known object
            var obj5 = jsonArray.FirstOrDefault(x => x["id"]?.ToString() == "5");
            Assert.NotNull(obj5);
            Assert.Equal("Samsung Galaxy Z Fold2", obj5!["name"]?.ToString());
            Assert.Equal("Brown", obj5!["data"]?["color"]?.ToString());
            Assert.Equal("689.99", obj5!["data"]?["price"]?.ToString());
        }

        [Fact(DisplayName = "2) Add an object using POST"), TestPriority(2)]
        public async Task Create_Object_ShouldReturnId()
        {
            var request = new RestRequest("/objects", Method.Post);
            request.AddJsonBody(new
            {
                name = "Apple MacBook Pro 16",
                data = new
                {
                    year = 2019,
                    price = 1849.99,
                    CPU_model = "Intel Core i9",      // kept as-is to avoid big structural change
                    Hard_disk_size = "1 TB"
                }
            });

            var response = await _client.ExecuteAsync(request);

            Assert.Equal(200, (int)response.StatusCode);
            Assert.False(string.IsNullOrEmpty(response.Content));

            var json = JObject.Parse(response.Content!);
            Assert.NotNull(json["id"]);
            _createdObjectId = json["id"]!.ToString();

            // Basic checks
            Assert.Equal("Apple MacBook Pro 16", json["name"]);

            // Robust checks for fields in returned data:
            var data = json["data"] as JObject;
            // Accept either "CPU model" or "CPU_model" (or other minor variations)
            var cpuVal = GetDataValue(data, "CPU model", "CPU_model", "CPU model");
            Assert.Equal("Intel Core i9", cpuVal);

            var yearVal = GetDataValue(data, "year");
            Assert.Equal("2019", yearVal);

            var priceVal = GetDataValue(data, "price");
            // price may be returned as number or string; compare normalized string
            Assert.Equal("1849.99", priceVal?.ToString());

            var hddVal = GetDataValue(data, "Hard disk size", "Hard_disk_size");
            Assert.Equal("1 TB", hddVal);
        }

        [Fact(DisplayName = "3) Get single object by ID") , TestPriority(3)]
        public async Task Get_Object_By_Id_ShouldReturnCorrectObject()
        {
            Assert.False(string.IsNullOrEmpty(_createdObjectId), "Object ID was not created");

            var request = new RestRequest($"/objects/{_createdObjectId}", Method.Get);
            var response = await _client.ExecuteAsync(request);

            Assert.Equal(200, (int)response.StatusCode);
            Assert.False(string.IsNullOrEmpty(response.Content));

            var json = JObject.Parse(response.Content!);
            Assert.Equal(_createdObjectId, json["id"]?.ToString());
            Assert.Equal("Apple MacBook Pro 16", json["name"]);

            var data = json["data"] as JObject;
            var yearVal = GetDataValue(data, "year");
            Assert.Equal("2019", yearVal);

            var priceVal = GetDataValue(data, "price");
            Assert.Equal("1849.99", priceVal);

            // Check CPU model robustly (accept "CPU model" or "CPU_model")
            var cpuVal = GetDataValue(data, "CPU model", "CPU_model");
            Assert.Equal("Intel Core i9", cpuVal);
        }

        [Fact(DisplayName = "4) Update the object using PUT"), TestPriority(4)]
        public async Task Update_Object_ShouldChangeValues()
        {
            Assert.False(string.IsNullOrEmpty(_createdObjectId), "Object ID was not created");

            var request = new RestRequest($"/objects/{_createdObjectId}", Method.Put);
            request.AddJsonBody(new
            {
                name = "Apple MacBook Pro 16",
                data = new
                {
                    year = 2019,
                    price = 2049.99,
                    CPU_model = "Intel Core i9",
                    Hard_disk_size = "1 TB",
                    color = "silver"
                }
            });

            var response = await _client.ExecuteAsync(request);

            Assert.Equal(200, (int)response.StatusCode);
            Assert.False(string.IsNullOrEmpty(response.Content));

            var json = JObject.Parse(response.Content!);
            Assert.Equal("Apple MacBook Pro 16", json["name"]);
            var data = json["data"] as JObject;

            var colorVal = GetDataValue(data, "color");
            Assert.Equal("silver", colorVal);

            var priceVal = GetDataValue(data, "price");
            Assert.Equal("2049.99", priceVal);

            var yearVal = GetDataValue(data, "year");
            Assert.Equal("2019", yearVal);
        }

        [Fact(DisplayName = "5) Delete the object using DELETE"), TestPriority(5)]
        public async Task Delete_Object_ShouldReturnMessage()
        {
            Assert.False(string.IsNullOrEmpty(_createdObjectId), "Object ID was not created");

            var request = new RestRequest($"/objects/{_createdObjectId}", Method.Delete);
            var response = await _client.ExecuteAsync(request);

            Assert.Equal(200, (int)response.StatusCode);
            Assert.False(string.IsNullOrEmpty(response.Content));

            var json = JObject.Parse(response.Content!);
            Assert.Contains(_createdObjectId!, json["message"]!.ToString());
            Assert.Contains("deleted", json["message"]!.ToString().ToLower());
        }
    }
}
