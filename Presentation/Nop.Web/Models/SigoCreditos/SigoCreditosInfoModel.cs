using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Models.SigoCreditos
{
    public class SigoCreditosInfoModel
    {
      
        public long SigoClubId { get; set; }

        /// <summary>
        /// /Corresponde a la entidad del usuario
        /// </summary>
      
        public long EntityId { get; set; }

       
        public int CustomerDocumentType { get; set; }

        
        public string CustomerDocumentValue { get; set; }


        public string CostumerName { get; set; }

        
        public string CostumerLastName { get; set; }

        
        public string CostumerPhone { get; set; }

        public long NumberAccount { get; set; }

        public decimal OldAmount { get; set; }

        public decimal Amount { get; set; }





    }
}
