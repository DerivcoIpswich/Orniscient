using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Derivco.Orniscient.Viewer.Models.Connection
{
    public class ConnectionInfo
    {
        [Display(Name = "Silo IP Address")]
        public string Address { get; set; }

        [Display(Name ="Port number")]
        public int Port { get; set; }
    }
}