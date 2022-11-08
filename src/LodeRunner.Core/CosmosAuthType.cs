// TODO:  Move this to a better location

namespace LodeRunner.Core
{
    /// <summary>
    /// CosmosAuthTYpe
    /// </summary>
    public enum CosmosAuthType
    {
        /// <summary>
        /// Indicates to use secret key for Cosmos.
        /// </summary>
        SecretKey,

        /// <summary>
        /// Indicates to use Managed Idenity
        /// </summary>
        ManagedIdentity,
    }
}