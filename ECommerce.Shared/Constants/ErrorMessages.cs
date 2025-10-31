namespace ECommerce.Shared.Constants
{
    public static class ErrorMessages
    {
        #region Global
        public const string UnexpectedError = "global.unexpected.error";
        public const string UnexpectedAuthenticationError = "authentication.unexpected.error";
        public const string UnexpectedCacheError = "cache.unexpected.error";
        public const string UnexpectedHubError = "hub.unexpected.error";
        public const string UnexpectedElasticError = "elastic.unexpected.error";
        public const string NoTransactionInProgress = "global.no.transaction.in.progress";
        #endregion

        #region Utility
        public const string ErrorCalculatingDiscountedPrice = "math.error.calculating.discounted.price";
        #endregion

        #region Payment
        public const string PaymentFailed = "payment.failed";
        #endregion

        #region Queue
        public const string QueueConnectionStringNotConfigured = "queue.connection.string.not.configured";
        #endregion

        #region Cache
        public const string CacheConnectionStringNotConfigured = "cache.connection.string.not.configured";
        #endregion

        #region Hub
        public const string NotificationsNotFound = "hub.notifications.not.found";
        public const string NoUnreadNotifications = "hub.no.unread.notifications";
        public const string ErrorMarkingNotificationsAsRead = "hub.error.marking.notifications.as.read";
        #endregion

        #region Elastic
        public const string IndexFailure = "elastic.index.failure";
        public const string IndexCreationFailure = "elastic.index.creation.failure";
        public const string UpdateFailure = "elastic.update.failure";
        public const string DeleteFailure = "elastic.delete.failure";  
        public const string SearchFailure = "elastic.search.failure";
        #endregion

        #region UserAccount
        public const string AccountNotFound = "account.not.found";
        public const string AccountEmailNotFound = "account.email.not.found";
        public const string AccountBanned = "account.banned";
        public const string AccountAlreadyBanned = "account.already.banned";
        public const string AccountAlreadyUnbanned = "account.already.unbanned";
        public const string AccountCreationFailed = "account.creation.failed";
        public const string IdentityNumberAlreadyExists = "account.identity.number.already.exists";
        public const string AccountDeleted = "account.deleted";
        public const string AccountUnrestricted = "account.unrestricted";
        public const string InvalidEmailOrPassword = "account.invalid.email.or.password";
        public const string ErrorSeedingAdminUser = "account.error.seeding.admin.user";
        public const string AdminAccountAlreadyExists = "account.admin.already.exists";
        public const string UnauthorizedAction = "account.unauthorized.action";
        #endregion

        #region Authentication
        public const string ErrorLoggingIn = "authentication.error.logging.in";
        public const string ErrorValidatingLogin = "authentication.error.validating.login";
        public const string AccountNotAuthorized = "authentication.account.not.authorized";
        public const string InvalidCredentials = "authentication.invalid.credentials";
        #endregion

        #region Token
        public const string ErrorGeneratingTokens = "authentication.error.generating.tokens";
        public const string FailedToGenerateAccessToken = "authentication.failed.to.generate.access.token";
        public const string FailedToGenerateRefreshToken = "authentication.failed.to.generate.refresh.token";
        public const string FailedToRevokeToken = "authentication.failed.to.revoke.token";
        public const string NoActiveTokensFound = "authentication.no.active.tokens.found";
        #endregion

        #region Identity
        public const string ErrorCreatingRole = "account.error.creating.role";
        public const string ErrorAssigningRole = "account.error.assigning.role";
        public const string IdentityUserNotFound = "account.identity.user.not.found";
        #endregion

        #region Product
        public const string ProductNotFound = "product.not.found";
        public const string StockNotAvailable = "product.stock.not.available";
        public const string ProductExists = "product.exists";
        public const string ErrorCreatingProduct = "product.error.creating";
        public const string ErrorUpdatingProduct = "product.error.updating";
        public const string ErrorUpdatingProductStock = "product.error.updating.stock";
        #endregion

        #region Basket
        public const string BasketItemNotFound = "basket.item.not.found";
        public const string ErrorAddingItemToBasket = "basket.error.adding.item";
        #endregion

        #region Category
        public const string CategoryExists = "category.exists";
        public const string ErrorCreatingCategory = "category.error.creating";
        public const string CategoryNotFound = "category.not.found";
        public const string ErrorDeletingCategory = "category.error.deleting";
        public const string ErrorUpdatingCategory = "category.error.updating";
        #endregion

        #region Order
        public const string OrderNotFound = "order.not.found";
        public const string NoPendingOrders = "order.no.pending.orders";
        #endregion

        #region Search
        public const string NoSearchResults = "search.no.results";
        public const string QueryCannotBeEmpty = "search.query.cannot.be.empty";
        #endregion

        #region ObjectStorage
        public const string FileUploadFailed = "objectstorage.file.upload.failed";
        #endregion
    }
}
