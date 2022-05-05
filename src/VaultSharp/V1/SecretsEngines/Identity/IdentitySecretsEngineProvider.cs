using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VaultSharp.Core;
using VaultSharp.V1.Commons;
using VaultSharp.V1.SecretsEngines.Identity.Models;

namespace VaultSharp.V1.SecretsEngines.Identity;

internal class IdentitySecretsEngineProvider : IIdentitySecretsEngine
{
    private readonly Polymath _polymath;

    public IdentitySecretsEngineProvider(Polymath polymath)
    {
        _polymath = polymath;
    }

    public async Task<Secret<IdentityToken>> GetTokenAsync(string roleName, string mountPoint = null,
        string wrapTimeToLive = null)
    {
        Checker.NotNull(roleName, "roleName");

        return await _polymath
            .MakeVaultApiRequest<Secret<IdentityToken>>(
                mountPoint ?? _polymath.VaultClientSettings.SecretsEngineMountPoints.Identity,
                "/oidc/token/" + roleName.Trim('/'), HttpMethod.Get, wrapTimeToLive: wrapTimeToLive)
            .ConfigureAwait(_polymath.VaultClientSettings.ContinueAsyncTasksOnCapturedContext);
    }

    public async Task<Secret<bool>> IntrospectTokenAsync(string token, string clientId = null, string mountPoint = null,
        string wrapTimeToLive = null)
    {
        Checker.NotNull(token, "token");

        return await _polymath
            .MakeVaultApiRequest<Secret<bool>>(
                mountPoint ?? _polymath.VaultClientSettings.SecretsEngineMountPoints.Identity, "/oidc/introspect",
                HttpMethod.Post, new {token, client_id = clientId}, wrapTimeToLive: wrapTimeToLive)
            .ConfigureAwait(_polymath.VaultClientSettings.ContinueAsyncTasksOnCapturedContext);
    }

    public async Task<Secret<CreateOrUpdateEntityResponse>> CreateOrUpdateEntityById(
        CreateOrUpdateEntityByIdCommand entity,
        string mountPoint = null,
        string wrapTimeToLive = null)
    {
        var configWithoutNullProperties = JsonConvert
            .DeserializeObject(JsonConvert.SerializeObject(
                entity,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore}
            ))!;

        return await _polymath
            .MakeVaultApiRequest<Secret<CreateOrUpdateEntityResponse>>(
                mountPoint ?? _polymath.VaultClientSettings.SecretsEngineMountPoints.Identity, "/entity",
                HttpMethod.Post, configWithoutNullProperties, wrapTimeToLive: wrapTimeToLive)
            .ConfigureAwait(_polymath.VaultClientSettings.ContinueAsyncTasksOnCapturedContext);
    }

    public async Task<Secret<ReadEntityByIdResponse>> ReadEntityById(string id, string mountPoint = null,
        string wrapTimeToLive = null)
    {
        return await _polymath
            .MakeVaultApiRequest<Secret<ReadEntityByIdResponse>>(
                mountPoint ?? _polymath.VaultClientSettings.SecretsEngineMountPoints.Identity,
                $"/entity/id/{id}", HttpMethod.Get, wrapTimeToLive: wrapTimeToLive)
            .ConfigureAwait(_polymath.VaultClientSettings.ContinueAsyncTasksOnCapturedContext);
    }

    public async Task<Secret<ReadEntityByIdResponse>> ReadEntityByName(string name, string mountPoint = null,
        string wrapTimeToLive = null)
    {
        return await _polymath
            .MakeVaultApiRequest<Secret<ReadEntityByIdResponse>>(
                mountPoint ?? _polymath.VaultClientSettings.SecretsEngineMountPoints.Identity,
                $"/entity/name/{name}", HttpMethod.Get, wrapTimeToLive: wrapTimeToLive)
            .ConfigureAwait(_polymath.VaultClientSettings.ContinueAsyncTasksOnCapturedContext);
    }

    public async Task<Secret<CreateOrUpdateEntityResponse>> CreateOrUpdateEntityByName(
        CreateOrUpdateEntityByNameCommand entity,
        string mountPoint = null,
        string wrapTimeToLive = null)
    {
        var configWithoutNullProperties = JsonConvert
            .DeserializeObject(JsonConvert.SerializeObject(
                entity,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore}
            ))!;

        return await _polymath
            .MakeVaultApiRequest<Secret<CreateOrUpdateEntityResponse>>(
                mountPoint ?? _polymath.VaultClientSettings.SecretsEngineMountPoints.Identity, "/entity",
                HttpMethod.Post, configWithoutNullProperties, wrapTimeToLive: wrapTimeToLive)
            .ConfigureAwait(_polymath.VaultClientSettings.ContinueAsyncTasksOnCapturedContext);
    }

    public async Task<Secret<CreateAliasResponse>> CreateAlias(
        CreateAliasCommand alias,
        string mountPoint = null,
        string wrapTimeToLive = null)
    {
        var configWithoutNullProperties = JsonConvert
            .DeserializeObject(JsonConvert.SerializeObject(
                alias,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore}
            ))!;

        return await _polymath
            .MakeVaultApiRequest<Secret<CreateAliasResponse>>(
                mountPoint ?? _polymath.VaultClientSettings.SecretsEngineMountPoints.Identity, "/entity-alias",
                HttpMethod.Post, configWithoutNullProperties, wrapTimeToLive: wrapTimeToLive)
            .ConfigureAwait(_polymath.VaultClientSettings.ContinueAsyncTasksOnCapturedContext);
    }

    public async Task<Secret<ListInfo>> ListEntitiesByName(string mountPoint = null,
        string wrapTimeToLive = null)
    {
        return await _polymath
            .MakeVaultApiRequest<Secret<ListInfo>>(
                mountPoint ?? _polymath.VaultClientSettings.SecretsEngineMountPoints.Identity,
                "/entity/name" + "?list=true",
                HttpMethod.Get, wrapTimeToLive: wrapTimeToLive)
            .ConfigureAwait(_polymath.VaultClientSettings.ContinueAsyncTasksOnCapturedContext);
    }

    public async Task<Secret<ReadEntityAliasByIdResponse>> ReadEntityAliasById(string id, string mountPoint = null,
        string wrapTimeToLive = null)
    {
        return await _polymath
            .MakeVaultApiRequest<Secret<ReadEntityAliasByIdResponse>>(
                mountPoint ?? _polymath.VaultClientSettings.SecretsEngineMountPoints.Identity,
                $"/entity-alias/id/{id}", HttpMethod.Get, wrapTimeToLive: wrapTimeToLive)
            .ConfigureAwait(_polymath.VaultClientSettings.ContinueAsyncTasksOnCapturedContext);
    }
}