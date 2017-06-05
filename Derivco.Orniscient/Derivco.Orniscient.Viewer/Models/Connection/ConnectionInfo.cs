using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Derivco.Orniscient.Viewer.Models.Connection
{
    public class ConnectionInfo
    {
        [Display(Name ="IP Address of the silo")]
        public string Address { get; set; }

        [Display(Name ="Port")]
        public int Port { get; set; }
    }
}