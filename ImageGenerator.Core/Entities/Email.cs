namespace ImageGenerator.API.DTOs
{
    public class Email
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string To { get; set; }
        public string Body { get; set; }
    }
}
