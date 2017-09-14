using IdentityServerSample.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerSample.ViewModels
{
    public class LoginInputModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public bool RememberLogin { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class LoginViewModel : LoginInputModel
    {
        public IEnumerable<ExternalProvider> ExternalProviders { get; set; }
    }
}
