using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a bulk edit product model
    /// </summary>
    public partial class BulkEditProductModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Catalog.BulkEdit.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.BulkEdit.Fields.SKU")]
        public string Sku { get; set; }

        [NopResourceDisplayName("Admin.Catalog.BulkEdit.Fields.SKU")]
        public string GTIN { get; set; }

        [NopResourceDisplayName("Admin.Catalog.BulkEdit.Fields.Price")]
        public decimal Price { get; set; }

        [NopResourceDisplayName("Admin.Catalog.BulkEdit.Fields.OldPrice")]
        public decimal OldPrice { get; set; }

        [NopResourceDisplayName("Admin.Catalog.BulkEdit.Fields.ManageInventoryMethod")]
        public string ManageInventoryMethod { get; set; }

        [NopResourceDisplayName("Admin.Catalog.BulkEdit.Fields.StockQuantity")]
        public int StockQuantity { get; set; }

        [NopResourceDisplayName("Admin.Catalog.BulkEdit.Fields.MinStockQuantity")]
        public int MinStockQuantity { get; set; }
        [NopResourceDisplayName("Admin.Catalog.BulkEdit.Fields.MinStockQuantity")]
        public decimal ResultQuantity { get; set; }

        [NopResourceDisplayName("Admin.Catalog.BulkEdit.Fields.Published")]
        public bool Published { get; set; }

        //[NopResourceDisplayName("Admin.Catalog.BulkEdit.Fields.CantidadMinima")]
        //public int CantidadMinima { get; set; }

        //[NopResourceDisplayName("Admin.Catalog.BulkEdit.Fields.IdWarehouse")]
        //public int IdWarehouse { get; set; }

        //[NopResourceDisplayName("Admin.Catalog.BulkEdit.Fields.StoreId")]
        //public int StoreId { get; set; }


        //[NopResourceDisplayName("Admin.Catalog.BulkEdit.Fields.DisableBuyButton")]
        //public bool DisableBuyButton { get; set; }

        #endregion
    }
}