﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChapeauModel
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Role Role { get; set; }
        public int LoginCode { get; set; }

        public string FullName { get { return FirstName + " " + LastName; } }

        // default constructor
        public Employee()
        {

        }
    }

    public enum Role { Manager=1, Waiter, KitchenStaff, Barman, UnDefined }
}
