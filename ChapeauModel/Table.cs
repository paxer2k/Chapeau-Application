﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChapeauModel
{
    public class Table
    {
        public int TableID { get; set; }
        public TableStatus TableStatus { get; set; }
        public string CurrentOrderInfo { get; set; }

        public DateTime? TimeStamp { get; set; }

        // default constructor
        public Table()
        {

        }

        public Table(int tableID)
        {
            this.TableID = tableID;
        }
    }

    public enum TableStatus { Free = 1, Occupied = 2, Reserved = 3 }
}
