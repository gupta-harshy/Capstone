using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckoutOrchestrator.Entities
{
    public class ReserveInventoryResultDto
    {
        public bool Success { get; set; }
        public List<InventoryShortageDto> Shortages { get; set; } = new();
    }

    public class InventoryShortageDto
    {
        public Guid ProductId { get; set; }
        public int RequestedQty { get; set; }
        public int AvailableQty { get; set; }   
    }
}
