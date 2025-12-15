using System;
using System.Collections.Generic;
using System.Text;

namespace DAM.Core.Entities
{
    public class Invoice
    {
        public int Id { get; set; }

        public required string SerialNumber { get; set; }

        public required DateTime Timestamp { get; set; }

        public required decimal TotalAmount { get; set; }

        public required string Description { get; set; }
    }
}
