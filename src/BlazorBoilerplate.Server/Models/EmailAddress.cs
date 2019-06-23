namespace BlazorBoilerplate.Server.Models
{
    public class EmailAddress
    {
        private string _name;

        public string Name
        {
            get {
                if (string.IsNullOrEmpty(_name))
                {
                    return Address;
                }
                return _name;
            }
            set {
                _name = value;
            }
        }
        public string Address { get; set; }

        public EmailAddress(string name, string address)
        {
            _name = name;
            Address = address;
        }
    }
}
