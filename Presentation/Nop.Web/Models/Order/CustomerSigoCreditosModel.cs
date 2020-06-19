using System;
using System.Collections.Generic;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Common;

namespace Nop.Web.Models.Order
{
    public partial class CustomerSigoCreditosModel : BaseNopModel
    {
        public CustomerSigoCreditosModel()
        {
            SigoCreditos = new List<SigoCreditosModel>();
        }

        public IList<SigoCreditosModel> SigoCreditos { get; set; }
        public PagerModel PagerModel { get; set; }
        public int SigoCreditosBalance { get; set; }
        public string SigoCreditosAmount { get; set; }
        public int MinimumSigoCreditosBalance { get; set; }
        public string MinimumSigoCreditosAmount { get; set; }

        #region Nested classes

        public partial class SigoCreditosModel : BaseNopEntityModel
        {
            //[NopResourceDisplayName("RewardPoints.Fields.Points")]
            //public int Points { get; set; }

            //[NopResourceDisplayName("RewardPoints.Fields.PointsBalance")]
            //public string PointsBalance { get; set; }

            //[NopResourceDisplayName("RewardPoints.Fields.Message")]
            //public string Message { get; set; }

            //[NopResourceDisplayName("RewardPoints.Fields.CreatedDate")]
            //public DateTime CreatedOn { get; set; }

            //[NopResourceDisplayName("RewardPoints.Fields.EndDate")]
            //public DateTime? EndDate { get; set; }
        }

        #endregion
    }
}