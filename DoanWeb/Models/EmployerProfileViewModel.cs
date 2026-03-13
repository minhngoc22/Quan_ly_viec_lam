using System;
using System.ComponentModel.DataAnnotations;

namespace DoanWeb.Models
{
    public class EmployerProfileViewModel
    {
        public EmployerPersonalViewModel Personal { get; set; }
        public CompanyViewModel Company { get; set; }
    }
}
