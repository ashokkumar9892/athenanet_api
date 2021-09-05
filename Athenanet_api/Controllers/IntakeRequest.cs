namespace Athenanet_api.Controllers
{
    public class IntakeRequest
    {
        private string firstname;

        private string lastname;

        private string email;

        private string gurantoremail;

        private string dob;

        private string ssn;

        public string Firstname { get => firstname; set => firstname = value; }
        public string Lastname { get => lastname; set => lastname = value; }
        public string Email { get => email; set => email = value; }
        public string Gurantoremail { get => gurantoremail; set => gurantoremail = value; }
        public string Dob { get => dob; set => dob = value; }
        public string Ssn { get => ssn; set => ssn = value; }
    }
}