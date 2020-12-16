﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Floresta.Models
{
    public class Marker
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "The Title filed is required")]
        public string Title { get; set; }
        [Required(ErrorMessage = "The Lat filed is required")]
        public string Lat { get; set; }
        [Required(ErrorMessage = "The Lng filed is required")]
        public string Lng { get; set; }
    }
}
