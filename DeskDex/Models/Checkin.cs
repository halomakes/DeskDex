﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;

namespace DeskDex.Models
{
    public class Checkin
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Last Check-in")]
        public DateTime LastUpdate { get; set; }
    }
    public class Station
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Display(Name = "MAC Address")]
        public string PhysicalAddress { get; set; }

        [Display(Name = "Station Number")]
        public string Location { get; set; }

        [Display(Name = "Available Equipment")]
        public List<Equipment> Equipment { get; set; }

        public Checkin LastCheckin { get; set; }

        public int Capacity { get; set; }

        [Display(Name = "Type")]
        public WorkStyle Type { get; set; }

        public float x1 { get; set; }
        public float y1 { get; set; }
        public float x2 { get; set; }
        public float y2 { get; set; }
    }

    public class WorkStyle
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public WorkStyle(string Name)
        {
            this.Name = Name;
        }
        public string Name { get; set; }
    }

    public class Equipment
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public Equipment(string Name, string Description)
        {
            this.Name = Name;
            this.Description = Description;
        }

        public string Name { get; set; }
        public string Description { get; set; }
    }
}