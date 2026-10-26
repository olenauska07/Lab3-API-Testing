namespace RestfulBookerTests.Models
{
    public class BookingModel
    {
        public string firstname { get; set; } = string.Empty;
        public string lastname { get; set; } = string.Empty;
        public int totalprice { get; set; }
        public bool depositpaid { get; set; }
        public BookingDates bookingdates { get; set; } = new();
        public string additionalneeds { get; set; } = string.Empty;
    }

    public class BookingResponse
    {
        public int bookingid { get; set; }
        public BookingModel booking { get; set; } = new();
    }
}