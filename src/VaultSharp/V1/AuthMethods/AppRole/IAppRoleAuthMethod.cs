using System.Threading.Tasks;
using VaultSharp.V1.AuthMethods.AppRole.Models;
using VaultSharp.V1.Commons;

namespace VaultSharp.V1.AuthMethods.AppRole
{
    /// <summary>
    ///     Non login operations.
    /// </summary>
    public interface IAppRoleAuthMethod
    {
        /// <summary>
        ///     Reads the properties of an existing AppRole.
        /// </summary>
        /// <param name="roleName">Name of the Role.</param>
        /// <param name="mountPoint">Mount point of the AppRole Auth method</param>
        /// <returns></returns>
        Task<Secret<AppRoleInfo>> ReadRoleAsync(string roleName, string mountPoint = "approle");

        /// <summary>
        ///     Writes or updates a Approle Role
        /// </summary>
        /// <param name="role"></param>
        /// <para>[required]</para>
        /// Role description.
        /// <returns></returns>
        Task<string> WriteAppRoleRoleAsync(AppRoleRole role, string appRoleMount = "approle");

        /// <summary>
        ///     Reads the Role_Id of the given AppRole
        /// </summary>
        Task<Secret<RoleId>> ReadRoleIdAsync(string roleName, string appRoleMount = "approle");

        /// <summary>
        ///     Creates a SecretId for the given RoleName
        /// </summary>
        /// <param name="role"></param>
        /// <para>[required]</para>
        /// Role description.
        /// <returns></returns>
        Task<Secret<SecretId>> CreateSecretId(string roleName, string appRoleMount = "approle");
    }
}