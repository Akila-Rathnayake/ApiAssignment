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

        [Fact(DisplayName = "1) Get list of all objects"), TestPriority(1)]
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
                data = new Dictionary<string, object>
                {
                    { "year", 2019 },
                    { "price", 1849.99 },
                    { "CPU model", "Intel Core i9" },
                    { "Hard disk size", "1 TB" }
                }
            });

            var response = await _client.ExecuteAsync(request);

            Assert.Equal(200, (int)response.StatusCode);
            Assert.False(string.IsNullOrEmpty(response.Content));

            var json = JObject.Parse(response.Content!);
            Assert.NotNull(json["id"]);
            _createdObjectId = json["id"]!.ToString();

            // Validate exact values
            Assert.Equal("Apple MacBook Pro 16", json["name"]);
            var data = json["data"] as JObject;
            Assert.Equal("Intel Core i9", data?["CPU model"]?.ToString());
            Assert.Equal("1 TB", data?["Hard disk size"]?.ToString());
            Assert.Equal("2019", data?["year"]?.ToString());
            Assert.Equal("1849.99", data?["price"]?.ToString());
        }

        [Fact(DisplayName = "3) Get single object by ID"), TestPriority(3)]
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
            Assert.Equal("Intel Core i9", data?["CPU model"]?.ToString());
            Assert.Equal("1 TB", data?["Hard disk size"]?.ToString());
            Assert.Equal("2019", data?["year"]?.ToString());
            Assert.Equal("1849.99", data?["price"]?.ToString());
        }

        [Fact(DisplayName = "4) Update the object using PUT"), TestPriority(4)]
        public async Task Update_Object_ShouldChangeValues()
        {
            Assert.False(string.IsNullOrEmpty(_createdObjectId), "Object ID was not created");

            var request = new RestRequest($"/objects/{_createdObjectId}", Method.Put);
            request.AddJsonBody(new
            {
                name = "Apple MacBook Pro 16",
                data = new Dictionary<string, object>
                {
                    { "year", 2019 },
                    { "price", 2049.99 },
                    { "CPU model", "Intel Core i9" },
                    { "Hard disk size", "1 TB" },
                    { "color", "silver" }
                }
            });

            var response = await _client.ExecuteAsync(request);

            Assert.Equal(200, (int)response.StatusCode);
            Assert.False(string.IsNullOrEmpty(response.Content));

            var json = JObject.Parse(response.Content!);
            Assert.Equal("Apple MacBook Pro 16", json["name"]);

            var data = json["data"] as JObject;
            Assert.Equal("Intel Core i9", data?["CPU model"]?.ToString());
            Assert.Equal("1 TB", data?["Hard disk size"]?.ToString());
            Assert.Equal("2019", data?["year"]?.ToString());
            Assert.Equal("2049.99", data?["price"]?.ToString());
            Assert.Equal("silver", data?["color"]?.ToString());
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
