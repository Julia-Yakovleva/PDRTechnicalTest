using System.Net.Mail;

namespace PDR.PatientBooking.Service.Validation.Email
{
    public class EmailValidator : IEmailValidator
    {
        public bool Validate(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
