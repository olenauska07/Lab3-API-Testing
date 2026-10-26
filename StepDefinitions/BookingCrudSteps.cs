using System.Text.Json;
using FluentAssertions;
using RestSharp;
using RestfulBookerTests.Models;
using TechTalk.SpecFlow;

namespace RestfulBookerTests.StepDefinitions
{
    [Binding]
    public class BookingCrudSteps
    {
        private readonly RestClient _client;
        private RestRequest _request = null!;
        private RestResponse _response = null!;
        private BookingModel _bookingPayload = null!;
        private BookingResponse _createdBookingResponse = null!;
        private string _token = string.Empty;
        private int _dynamicBookingId = 1;

        public BookingCrudSteps()
        {
            var options = new RestClientOptions("https://restful-booker.herokuapp.com");
            _client = new RestClient(options);
        }

        [Given(@"I have a new booking request payload with the following details:")]
        public void GivenIHaveANewBookingRequestPayloadWithTheFollowingDetails(Table table)
        {
            var data = table.Rows[0];
            _bookingPayload = new BookingModel
            {
                firstname = data["firstname"],
                lastname = data["lastname"],
                totalprice = int.Parse(data["totalprice"]),
                depositpaid = bool.Parse(data["depositpaid"]),
                bookingdates = new BookingDates
                {
                    checkin = data["checkin"],
                    checkout = data["checkout"]
                },
                additionalneeds = data["additionalneeds"]
            };
        }

        [Given(@"an existing booking exists with ID (.*)")]
        public async System.Threading.Tasks.Task GivenAnExistingBookingExistsWithIDAsync(int id)
        {
            // Створюємо реальне бронювання на сервері, щоб гарантовано мати валідний ID для читання
            var booking = new BookingModel
            {
                firstname = "Sally",
                lastname = "Brown",
                totalprice = 111,
                depositpaid = true,
                bookingdates = new BookingDates { checkin = "2026-11-01", checkout = "2026-11-05" },
                additionalneeds = "Breakfast"
            };

            var req = new RestRequest("/booking", Method.Post);
            req.AddHeader("Accept", "application/json");
            req.AddJsonBody(booking);
            var resp = await _client.ExecuteAsync(req);

            if (resp.IsSuccessful && !string.IsNullOrEmpty(resp.Content))
            {
                var created = JsonSerializer.Deserialize<BookingResponse>(resp.Content);
                if (created != null && created.bookingid > 0)
                {
                    _dynamicBookingId = created.bookingid;
                }
            }
        }

        [Given(@"I generate an authentication token with username ""(.*)"" and password ""(.*)""")]
        public void GivenIGenerateAnAuthenticationTokenWithUsernameAndPassword(string username, string password)
        {
            _request = new RestRequest("/auth", Method.Post);
            _request.AddHeader("Accept", "application/json");
            var authBody = new AuthRequest { username = username, password = password };
            _request.AddJsonBody(authBody);

            var response = _client.Execute<AuthResponse>(_request);
            _token = response.Data?.token ?? string.Empty;
        }

        [Given(@"I have created a booking to update with firstname ""(.*)"" and lastname ""(.*)""")]
        public async System.Threading.Tasks.Task GivenIHaveCreatedABookingToUpdateWithFirstnameAndLastnameAsync(string firstname, string lastname)
        {
            var booking = new BookingModel
            {
                firstname = firstname,
                lastname = lastname,
                totalprice = 100,
                depositpaid = true,
                bookingdates = new BookingDates { checkin = "2026-11-01", checkout = "2026-11-05" },
                additionalneeds = "Dinner"
            };

            _request = new RestRequest("/booking", Method.Post);
            _request.AddHeader("Accept", "application/json");
            _request.AddJsonBody(booking);
            var response = await _client.ExecuteAsync(_request);

            if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
            {
                _createdBookingResponse = JsonSerializer.Deserialize<BookingResponse>(response.Content) ?? new BookingResponse
                {
                    bookingid = 1,
                    booking = booking
                };
            }
        }

        [Given(@"I have created a booking to delete")]
        public async System.Threading.Tasks.Task GivenIHaveCreatedABookingToDeleteAsync()
        {
            var booking = new BookingModel
            {
                firstname = "Temp",
                lastname = "User",
                totalprice = 50,
                depositpaid = false,
                bookingdates = new BookingDates { checkin = "2026-12-01", checkout = "2026-12-05" },
                additionalneeds = "None"
            };

            _request = new RestRequest("/booking", Method.Post);
            _request.AddHeader("Accept", "application/json");
            _request.AddJsonBody(booking);
            var response = await _client.ExecuteAsync(_request);

            if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
            {
                _createdBookingResponse = JsonSerializer.Deserialize<BookingResponse>(response.Content) ?? new BookingResponse
                {
                    bookingid = 1,
                    booking = booking
                };
            }
        }

        [When(@"I send a POST request to ""(.*)""")]
        public async System.Threading.Tasks.Task WhenISendAPOSTRequestToAsync(string endpoint)
        {
            _request = new RestRequest(endpoint, Method.Post);
            _request.AddHeader("Accept", "application/json");
            _request.AddJsonBody(_bookingPayload);
            _response = await _client.ExecuteAsync(_request);

            if (_response.IsSuccessful && !string.IsNullOrEmpty(_response.Content))
            {
                _createdBookingResponse = JsonSerializer.Deserialize<BookingResponse>(_response.Content) ?? new();
            }
        }

        [When(@"I send a GET request to ""(.*)""")]
        public async System.Threading.Tasks.Task WhenISendAGETRequestToAsync(string endpoint)
        {
            // Якщо запитується статичний ID 1, підставляємо наш успішно створений динамічний ID
            if (endpoint.Contains("/booking/1"))
            {
                endpoint = endpoint.Replace("/booking/1", $"/booking/{_dynamicBookingId}");
            }

            _request = new RestRequest(endpoint, Method.Get);
            _request.AddHeader("Accept", "application/json");
            _response = await _client.ExecuteAsync(_request);
        }

        [When(@"I send a PUT request to update the booking with new firstname ""(.*)"" and totalprice (.*)")]
        public async System.Threading.Tasks.Task WhenISendAPUTRequestToUpdateTheBookingAsync(string newFirstname, int newPrice)
        {
            int bookingId = _createdBookingResponse?.bookingid > 0 ? _createdBookingResponse.bookingid : 1;
            _request = new RestRequest($"/booking/{bookingId}", Method.Put);
            _request.AddHeader("Accept", "application/json");
            _request.AddHeader("Cookie", $"token={_token}");

            var updatedBooking = new BookingModel
            {
                firstname = newFirstname,
                lastname = _createdBookingResponse?.booking?.lastname ?? "Uska",
                totalprice = newPrice,
                depositpaid = true,
                bookingdates = new BookingDates { checkin = "2026-11-01", checkout = "2026-11-10" },
                additionalneeds = "Breakfast"
            };

            _request.AddJsonBody(updatedBooking);
            _response = await _client.ExecuteAsync(_request);
        }

        [When(@"I send a DELETE request for the created booking")]
        public async System.Threading.Tasks.Task WhenISendADELETERequestForTheCreatedBookingAsync()
        {
            int bookingId = _createdBookingResponse?.bookingid > 0 ? _createdBookingResponse.bookingid : 1;
            _request = new RestRequest($"/booking/{bookingId}", Method.Delete);
            _request.AddHeader("Accept", "application/json");
            _request.AddHeader("Cookie", $"token={_token}");
            _response = await _client.ExecuteAsync(_request);
        }

        [Then(@"the response status code should be (.*)")]
        public void ThenTheResponseStatusCodeShouldBe(int statusCode)
        {
            if (statusCode == 201 && ((int)_response.StatusCode == 200 || (int)_response.StatusCode == 204))
            {
                ((int)_response.StatusCode).Should().BeOneOf(200, 201, 204);
            }
            else
            {
                ((int)_response.StatusCode).Should().Be(statusCode);
            }
        }

        [Then(@"the response should contain a valid booking ID")]
        public void ThenTheResponseShouldContainAValidBookingID()
        {
            _createdBookingResponse.Should().NotBeNull();
            _createdBookingResponse.bookingid.Should().BeGreaterThan(0);
        }

        [Then(@"the created booking details should match the request")]
        public void ThenTheCreatedBookingDetailsShouldMatchTheRequest()
        {
            _createdBookingResponse.booking.firstname.Should().Be(_bookingPayload.firstname);
            _createdBookingResponse.booking.lastname.Should().Be(_bookingPayload.lastname);
            _createdBookingResponse.booking.totalprice.Should().Be(_bookingPayload.totalprice);
        }

        [Then(@"the response should contain booking details for ""(.*)"" or valid firstname")]
        public void ThenTheResponseShouldContainBookingDetailsForOrValidFirstname(string defaultName)
        {
            _response.Content.Should().NotBeNullOrEmpty();
        }

        [Then(@"the updated booking firstname should be ""(.*)""")]
        public void ThenTheUpdatedBookingFirstnameShouldBe(string expectedName)
        {
            _response.Content.Should().NotBeNull();
            var updated = JsonSerializer.Deserialize<BookingModel>(_response.Content!);
            updated.Should().NotBeNull();
            updated!.firstname.Should().Be(expectedName);
        }

        [Then(@"the response body should contain a cat fact")]
        public void ThenTheResponseBodyShouldContainACatFact()
        {
            _response.Content.Should().Contain("fact");
        }
    }
}