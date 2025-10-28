namespace ECommerce.Shared.Constants
{
    public static class CacheKeys
    {
        #region Basket
        public const string AllBasketItems = "cache:basketItems:all";
        #endregion

        #region Product
        public const string ProductById = "cache:products:{0}";
        public const string AllProducts = "cache:products:all";
        #endregion

        #region Category
        public const string CategoryById = "cache:categories:{0}";
        #endregion

        #region Profile
        public const string Profile = "cache:profile";
        #endregion
    }
}
