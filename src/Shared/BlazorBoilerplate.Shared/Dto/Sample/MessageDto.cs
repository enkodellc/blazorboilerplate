namespace BlazorBoilerplate.Shared.Dto.Sample
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Text { get; set; }
        public DateTime When { get; set; }
        public Guid UserID { get; set; }

        public bool Mine { get; set; }

        public MessageDto() { }

        public MessageDto(int id, string userName, string text, bool mine)
        {
            Id = id;
            UserName = userName;
            Text = text;
            Mine = mine;
        }

        /// <summary>
        /// Determine CSS classes to use for message div
        /// TODO: disambiguate between your and other members
        /// </summary>
        public string CSS
        {
            get
            {
                return Mine ? "sent" : "received";
            }
        }
    }
}