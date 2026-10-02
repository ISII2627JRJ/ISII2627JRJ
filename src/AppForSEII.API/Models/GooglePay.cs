using System;
namespace AppForSEII.API.Models
{
    public class GooglePay : MetodoPago
    {
        public string Email { get; set; }

        public GooglePay(string email)
        {
            Email = email;
        }

        public GooglePay()
        {
        }
    }
}