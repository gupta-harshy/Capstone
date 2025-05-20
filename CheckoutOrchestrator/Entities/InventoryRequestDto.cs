using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckoutOrchestrator.Entities
{
    public class InventoryRequestDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
