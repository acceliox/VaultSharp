using System.Net.Http;
using System.Threading.Tasks;
using VaultSharp.Core;
using VaultSharp.V1.AuthMethods.AppRole.Models;
using VaultSharp.V1.Commons;

namespace VaultSharp.V1.AuthMethods.AppRole
{
    internal class AppRoleAuthMethodProvider : IAppRoleAuthMethod
    {
        private readonly Polymath _polymath;

        public AppRoleAuthMethodProvider(Polymath polymath)
        {
            Checker.NotNull(polymath, "polymath");
            _polymath = polymath;
        }

        public async Task<Secret<AppRoleInfo>> ReadRoleAsync(string roleName,
            string mountPoint = AuthMethodDefaultPaths.AppRole)
        {
            Checker.NotNull(mountPoint, "mountPoint");
            Checker.NotNull(roleName, "roleName");
            return await _polymath
                .MakeVaultApiRequest<Secret<AppRoleInfo>>(
                    "v1/auth/" + mountPoint.Trim('/') + "/role/" + roleName.Trim('/'), HttpMethod.Get)
                .ConfigureAwait(_polymath.VaultClientSettings.ContinueAsyncTasksOnCapturedContext);
        }

        public async Task<string> WriteAppRoleRoleAsync(AppRoleRole role, string appRoleMount = "approle")
        {
            var requestData = new {role.role_name, role.token_policies};
            return await _polymath
                .MakeVaultApiRequest<string>($"v1/auth/{appRoleMount}/role/{role.role_name}", HttpMethod.Post,
                    requestData).ConfigureAwait(_polymath.VaultClientSettings.ContinueAsyncTasksOnCapturedContext);
        }

        public async Task<Secret<RoleId>> ReadRoleIdAsync(string roleName, string appRoleMount = "approle")
        {
            var requestData = new {roleName};
            return await _polymath
                .MakeVaultApiRequest<Secret<RoleId>>($"v1/auth/{appRoleMount}/role/{roleName}/role-id", HttpMethod.Get)
                .ConfigureAwait(_polymath.VaultClientSettings.ContinueAsyncTasksOnCapturedContext);
        }

        public async Task<Secret<SecretId>> CreateSecretId(string roleName, string appRoleMount = "approle")
        {
            var requestData = new {roleName};
            return await _polymath
                .MakeVaultApiRequest<Secret<SecretId>>($"v1/auth/{appRoleMount}/role/{roleName}/secret-id",
                    HttpMethod.Post)
                .ConfigureAwait(_polymath.VaultClientSettings.ContinueAsyncTasksOnCapturedContext);
        }
    }
}