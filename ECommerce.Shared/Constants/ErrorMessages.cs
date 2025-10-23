namespace ECommerce.Shared.Constants
{
    public static class ErrorMessages
    {
        #region Global
        public const string UnexpectedError = "global.unexpected.error";
        public const string UnexpectedAuthenticationError = "authentication.unexpected.error";
        public const string UnexpectedCacheError = "cache.unexpected.error";
        public const string UnexpectedHubError = "hub.unexpected.error";
        public const string NoTransactionInProgress = "global.no.transaction.in.progress";
        #endregion

        #region Utility
        public const string InvalidDiscountRate = "math.invalid.discount.rate";
        public const string ErrorCalculatingDiscountedPrice = "math.error.calculating.discounted.price";
        #endregion

        #region Payment
        public const string PaymentFailed = "payment.failed";
        public const string PaymentSuccessful = "payment.successful";
        #endregion

        #region Queue
        public const string QueueConnectionFailed = "queue.connection.failed";
        public const string QueueMessagePublishFailed = "queue.message.publish.failed";
        public const string QueueConnectionStringNotConfigured = "queue.connection.string.not.configured";
        #endregion

        #region Cache
        public const string CacheConnectionStringNotConfigured = "cache.connection.string.not.configured";
        #endregion

        #region Hub
        public const string NotificationsNotFound = "hub.notifications.not.found";
        public const string NoUnreadNotifications = "hub.no.unread.notifications";
        public const string ErrorMarkingNotificationsAsRead = "hub.error.marking.notifications.as.read";
        public const string FailedToDeleteNotification = "hub.failed.to.delete.notification";
        #endregion

        #region Account
        public const string AccountExists = "account.exists";
        public const string AccountNotFound = "account.not.found";
        public const string AccountEmailNotFound = "account.email.not.found";
        public const string AccountBanned = "account.banned";
        public const string AccountAlreadyBanned = "account.already.banned";
        public const string AccountAlreadyUnbanned = "account.already.unbanned";
        public const string AccountCreated = "account.created";
        public const string AccountCreationFailed = "account.creation.failed";
        public const string AccountEmailAlreadyExists = "account.email.already.exists";
        public const string IdentityNumberAlreadyExists = "account.identity.number.already.exists";
        public const string AccountDeleted = "account.deleted";
        public const string AccountUnrestricted = "account.unrestricted";
        public const string UserNotAuthenticated = "account.user.not.authenticated";
        public const string InvalidEmailOrPassword = "account.invalid.email.or.password";
        public const string ErrorSeedingAdminUser = "account.error.seeding.admin.user";
        public const string AdminAccountAlreadyExists = "account.admin.already.exists";
        #endregion

        #region Authentication
        public const string ErrorLoggingIn = "authentication.error.logging.in";
        public const string UserIsNotLoggedIn = "authentication.user.is.not.logged.in";
        public const string ErrorValidatingLogin = "authentication.error.validating.login";
        #endregion

        #region Token
        public const string ErrorGeneratingTokens = "authentication.error.generating.tokens";
        public const string FailedToGenerateAccessToken = "authentication.failed.to.generate.access.token";
        public const string FailedToGenerateRefreshToken = "authentication.failed.to.generate.refresh.token";
        public const string InvalidToken = "authentication.invalid.token";
        public const string FailedToRevokeToken = "authentication.failed.to.revoke.token";
        public const string ErrorValidatingToken = "authentication.error.validating.token";
        public const string NoActiveTokensFound = "authentication.no.active.tokens.found";
        public const string FailedToFetchUserTokens = "authentication.failed.to.fetch.user.tokens";
        public const string FailedToCleanupExpiredTokens = "authentication.failed.to.cleanup.expired.tokens";
        public const string InvalidTokenAlgorithm = "authentication.invalid.token.algorithm";
        #endregion

        #region Identity
        public const string ErrorCreatingRole = "account.error.creating.role";
        public const string ErrorAssigningRole = "account.error.assigning.role";
        public const string IdentityUserNotFound = "account.identity.user.not.found";
        public const string ErrorRegisteringUser = "account.error.registering.user";
        #endregion

        #region Product
        public const string ProductNotFound = "product.not.found";
        public const string StockNotAvailable = "product.stock.not.available";
        public const string ProductExists = "product.exists";
        public const string ErrorCreatingProduct = "product.error.creating";
        public const string ErrorUpdatingProduct = "product.error.updating";
        public const string ErrorUpdatingProductStock = "product.error.updating.stock";
        public const string UpdateProductStockSuccess = "product.update.stock.success";
        #endregion

        #region Basket
        public const string BasketItemNotFound = "basket.item.not.found";
        public const string ErrorAddingItemToBasket = "basket.error.adding.item";
        public const string ErrorUpdatingBasketItem = "basket.error.updating.item";
        #endregion

        #region Category
        public const string CategoryExists = "category.exists";
        public const string ErrorCreatingCategory = "category.error.creating";
        public const string CategoryCreated = "category.created";
        public const string CategoryNotFound = "category.not.found";
        public const string CategoryDeleted = "category.deleted";
        public const string ErrorDeletingCategory = "category.error.deleting";
        public const string ErrorUpdatingCategory = "category.error.updating";
        #endregion

        #region Order
        public const string OrderNotFound = "order.not.found";
        public const string NoPendingOrders = "order.no.pending.orders";
        public const string OrderCancelled = "order.cancelled";
        public const string OrderDeleted = "order.deleted";
        public const string OrderStatusUpdated = "order.status.updated";
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
