// Licensed to acceliox GmbH under one or more agreements.
// See the LICENSE file in the project root for more information.Copyright (c) acceliox GmbH. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using VaultSharp.Core;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.AppRole;
using VaultSharp.V1.AuthMethods.AppRole.Models;
using VaultSharp.V1.AuthMethods.GitHub;
using VaultSharp.V1.AuthMethods.GitHub.Models;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.SecretsEngines;
using VaultSharp.V1.SystemBackend;
using Xunit;

namespace VaultSharp.Samples;

public class VaultContainerTestsWithoutCli
{
    [Fact]
    public async Task VaultServer_ReadWriteKv2_ReturnsSameInput()
    {
        const string rootTokenId = "testRoot";
        const string containerName = "VaultTestsWithoutCLI";
        const int port = 8220;
        await using var container =
            VaultTestServer.BuildVaultServerContainer(port, rootTokenId: rootTokenId, containerName: containerName);
        await container.StartAsync();
        var rootClient = await CreateVaultRootClient(port);
        await MountKv2SecretsEngine(rootClient, "testSecrets");
        var secretData = new Dictionary<string, object>
        {
            {"password", "testPass"}, {"additionalInfo", "testInfo"}, {"user", "someUser"}
        };


        await rootClient.V1.Secrets.KeyValue.V2.WriteSecretAsync(
            "/test/kv2",
            secretData,
            mountPoint: "testSecrets"
        );


        (await rootClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(
                "/test/kv2",
                mountPoint: "testSecrets"
            ))
            .Data.Data.Should()
            .BeEquivalentTo(secretData);
    }

    [Fact]
    public async Task VaultServer_WriteDtoToKv2_ReturnsSameInput()
    {
        const string rootTokenId = "testRoot";
        const string containerName = "VaultTestsWithoutCLI";
        const int port = 8220;
        await using var container =
            VaultTestServer.BuildVaultServerContainer(port, rootTokenId: rootTokenId, containerName: containerName);
        await container.StartAsync();
        var rootClient = await CreateVaultRootClient(port);
        await MountKv2SecretsEngine(rootClient, "testSecrets");
        var data = new TestDto
        {
            TestString1 = "Hallo",
            TestString2 = "TestValues",
            TestUint = 123,
            TestInt = 456,
            TestDouble = 78.910
        };
        var secretData = new Dictionary<string, object> {{"password", data}};

        await rootClient.V1.Secrets.KeyValue.V2.WriteSecretAsync(
            "/test/kv2",
            secretData,
            mountPoint: "testSecrets"
        );

        var resultDictionary = (await rootClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(
            "/test/kv2",
            mountPoint: "testSecrets"
        )).Data.Data;
        var result = resultDictionary.ToType<TestDto>("password");
        result.Should().BeEquivalentTo(data);
    }

    [Fact]
    public async Task VaultServer_WriteMultiLevelDtoToKv2_ReturnsSameInput()
    {
        const string rootTokenId = "testRoot";
        const string containerName = "VaultTestsWithoutCLI";
        const int port = 8220;
        await using var container =
            VaultTestServer.BuildVaultServerContainer(port, rootTokenId: rootTokenId, containerName: containerName);
        await container.StartAsync();
        var rootClient = await CreateVaultRootClient(port);
        await MountKv2SecretsEngine(rootClient, "testSecrets");
        var data = new MultiLevelDto
        {
            SecondLevelDto =
                new TestDto
                {
                    TestString1 = "Hallo",
                    TestString2 = "TestValues",
                    TestUint = 123,
                    TestInt = 456,
                    TestDouble = 78.910
                },
            TestString = "TestValues",
            TestUint = 123,
            TestInt = 456,
            TestDouble = 78.910,
            RecursiveMultiLevelDto = new MultiLevelDto {SecondLevelDto = new TestDto {TestDouble = 999, TestInt = -1}}
        };
        var secretData = new Dictionary<string, object> {{"password", data}};

        await rootClient.V1.Secrets.KeyValue.V2.WriteSecretAsync(
            "/test/kv2",
            secretData,
            mountPoint: "testSecrets"
        );

        var resultDictionary = (await rootClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(
            "/test/kv2",
            mountPoint: "testSecrets"
        )).Data.Data;
        var result = resultDictionary.ToType<MultiLevelDto>("password");
        result.Should().BeEquivalentTo(data);
    }

    [Fact]
    public async Task VaultServer_ReadWriteKv1_ReturnsSameInput()
    {
        const string rootTokenId = "testRoot";
        const string containerName = "VaultTestsWithoutCLI";
        const int port = 8220;
        await using var container =
            VaultTestServer.BuildVaultServerContainer(port, rootTokenId: rootTokenId, containerName: containerName);
        await container.StartAsync();
        var rootClient = await CreateVaultRootClient(port);
        await MountKv1SecretsEngine(rootClient, "testSecrets");


        var secretData = new Dictionary<string, object>
        {
            {"password", "testPass"}, {"additionalInfo", "testInfo"}, {"user", "someUser"}
        };


        await rootClient.V1.Secrets.KeyValue.V1.WriteSecretAsync(
            "/test/kv1",
            secretData,
            "testSecrets"
        );


        (await rootClient.V1.Secrets.KeyValue.V1.ReadSecretAsync(
                "/test/kv1",
                "testSecrets"
            ))
            .Data.Should()
            .BeEquivalentTo(secretData);
    }

    [Fact]
    public async Task VaultServer_SetAppRolePermissions_TokenYieldsPolicy()
    {
        const string roleName = "testRole";
        const string appRolePath = "testAppRole";
        const string testSecretPath = "testSecrets";
        const string policyName = "testpolicy";
        const string rootTokenId = "testRoot";
        const string containerName = "VaultTestsWithoutCLI";
        const int port = 8220;
        await using var container =
            VaultTestServer.BuildVaultServerContainer(port, rootTokenId: rootTokenId, containerName: containerName);
        await container.StartAsync();
        var rootClient = await CreateVaultRootClient(port);
        await rootClient.V1.System.MountAuthBackendAsync(new AuthMethod
        {
            Path = appRolePath, Type = AuthMethodType.AppRole
        });
        // create read only permission policy
        await rootClient.V1.System.WritePolicyAsync(new Policy
        {
            Name = policyName,
            Rules =
                $"# read only permission for test secrets:\r\npath \"{testSecretPath}/data/*\" {{\r\n  capabilities = [\"read\"]\r\n}}"
        });
        await rootClient.V1.Auth.AppRole.WriteAppRoleRoleAsync(
            new AppRoleRole {role_name = roleName, token_policies = new[] {policyName}},
            appRolePath);
        var roleIdResultTest = await rootClient.V1.Auth.AppRole.ReadRoleIdAsync(roleName, appRolePath);
        var secretIdResultTest = await rootClient.V1.Auth.AppRole.CreateSecretId(roleName, appRolePath);

        // login with appRole Auth
        IAuthMethodInfo appRoleAuthMethodInfo =
            new AppRoleAuthMethodInfo(appRolePath, roleIdResultTest.Data.Role_Id, secretIdResultTest.Data.Secret_Id);
        var vaultClientSettings = new VaultClientSettings(
            $"http://127.0.0.1:{port}",
            appRoleAuthMethodInfo);
        var appRoleClient = new VaultClient(vaultClientSettings);
        await appRoleClient.V1.Auth.PerformImmediateLogin();

        var token = (await appRoleClient.V1.Auth.Token.LookupSelfAsync()).Data.Policies;

        token.Should().ContainMatch("testpolicy");
    }

    [Fact]
    public async Task VaultServer_CreateCustomRoleId_ReturnsCustomId()
    {
        const string roleNameA = "testrolea";
        const string roleNameB = "testroleb";
        const string appRolePath = "dev__testAppRole";
        const string rootTokenId = "testRoot";
        const string containerName = "VaultTestsWithoutCLI";
        const int port = 8220;
        await using var container =
            VaultTestServer.BuildVaultServerContainer(port, rootTokenId: rootTokenId, containerName: containerName);
        await container.StartAsync();
        var rootClient = await CreateVaultRootClient(port);

        await rootClient.V1.System.MountAuthBackendAsync(new AuthMethod
        {
            Path = appRolePath, Type = AuthMethodType.AppRole
        });

        await rootClient.V1.Auth.AppRole.WriteAppRoleRoleAsync(
            new AppRoleRole {role_name = roleNameA},
            appRolePath);

        await rootClient.V1.Auth.AppRole.WriteAppRoleRoleAsync(
            new AppRoleRole {role_name = roleNameB},
            appRolePath);

        var appRolesSecret = await rootClient.V1.Auth.AppRole.ReadAllAppRoles(appRolePath);
        var appRoles = appRolesSecret.Data;

        appRoles.Keys.Should().ContainMatch(roleNameA);
        appRoles.Keys.Should().ContainMatch(roleNameB);
    }

    [Fact]
    public async Task VaultServer_ReadAllAppRoles_ReturnsAllRoles()
    {
        const string roleName = "testRole";
        const string appRolePath = "dev__testAppRole";
        const string rootTokenId = "testRoot";
        const string containerName = "VaultTestsWithoutCLI";
        const string customRoleId = "historian";
        const int port = 8220;
        await using var container =
            VaultTestServer.BuildVaultServerContainer(port, rootTokenId: rootTokenId, containerName: containerName);
        await container.StartAsync();
        var rootClient = await CreateVaultRootClient(port);

        await rootClient.V1.System.MountAuthBackendAsync(new AuthMethod
        {
            Path = appRolePath, Type = AuthMethodType.AppRole
        });

        await rootClient.V1.Auth.AppRole.WriteAppRoleRoleAsync(
            new AppRoleRole {role_name = roleName},
            appRolePath);
        await rootClient.V1.Auth.AppRole.WriteCustomAppRoleId(roleName, customRoleId, appRolePath);

        var roleIdResultTest = await rootClient.V1.Auth.AppRole.ReadRoleIdAsync(roleName, appRolePath);
        roleIdResultTest.Data.Role_Id.Should().Match(customRoleId);
    }


    [Fact]
    public async Task VaultServer_AppRoleAuthWithResponseWrappedToken_TokenIsValid()
    {
        const string roleName = "testRole";
        const string appRolePath = "testAppRole";
        const string testSecretPath = "testSecrets";
        const string policyName = "testpolicy";
        const string rootTokenId = "testRoot";
        const string containerName = "VaultTestsWithoutCLI";
        const int port = 8220;
        const string vaultAdr = "http://127.0.0.1";

        await using var container =
            VaultTestServer.BuildVaultServerContainer(port, rootTokenId: rootTokenId, containerName: containerName);
        await container.StartAsync();
        var rootClient = await CreateVaultRootClient(port);

        await rootClient.V1.System.MountAuthBackendAsync(new AuthMethod
        {
            Path = appRolePath, Type = AuthMethodType.AppRole
        });
        // create read only permission policy
        await rootClient.V1.System.WritePolicyAsync(new Policy
        {
            Name = policyName,
            Rules =
                $"# read only permission for test secrets:\r\npath \"{testSecretPath}/data/*\" {{\r\n  capabilities = [\"read\"]\r\n}}"
        });

        // create test Role
        await rootClient.V1.Auth.AppRole.WriteAppRoleRoleAsync(
            new AppRoleRole {role_name = roleName, token_policies = new[] {policyName}},
            appRolePath);


        var roleId = (await rootClient.V1.Auth.AppRole.ReadRoleIdAsync(roleName, appRolePath)).Data.Role_Id;
        var responseWrappedTokenResponse =
            await rootClient.V1.Auth.AppRole.CreateResponseWrappedSecretId("10s", roleName, appRolePath);

        var responseWrappedToken = responseWrappedTokenResponse.WrapInfo.Token;

        var secretIdResultTest = await rootClient.V1.Auth.AppRole.CreateSecretId(roleName, appRolePath);

        // Authenticate with wrapping token
        IAuthMethodInfo wrappedTokenAuthMethod = new TokenAuthMethodInfo(responseWrappedToken);
        var vaultClientSettingsForUnwrapping =
            new VaultClientSettings($"{vaultAdr}:{port}", wrappedTokenAuthMethod);
        IVaultClient vaultClientForUnwrapping = new VaultClient(vaultClientSettingsForUnwrapping);
        // Use Wrapping token auth to unwrap secretId
        var secretIdData = await vaultClientForUnwrapping.V1.System
            .UnwrapWrappedResponseDataAsync<Dictionary<string, object>>(null);
        var secretId = secretIdData.Data["secret_id"].As<string>();

        IEnumerable<string> apiErrors = new List<string>();
        // try unwrap token a second time --> needs to throw exception
        try
        {
            wrappedTokenAuthMethod = new TokenAuthMethodInfo(responseWrappedToken);
            vaultClientSettingsForUnwrapping =
                new VaultClientSettings($"{vaultAdr}:{port}", wrappedTokenAuthMethod);
            vaultClientForUnwrapping = new VaultClient(vaultClientSettingsForUnwrapping);
            secretIdData = await vaultClientForUnwrapping.V1.System
                .UnwrapWrappedResponseDataAsync<Dictionary<string, object>>(null);
            var secondSecretId = secretIdData.Data["secret_id"].As<string>();
        }
        catch (VaultApiException e)
        {
            apiErrors = e.ApiErrors;
        }

        // login with appRole Auth
        IAuthMethodInfo appRoleAuthMethodInfo = new AppRoleAuthMethodInfo(appRolePath, roleId, secretId);
        var vaultClientSettings = new VaultClientSettings(
            $"{vaultAdr}:{port}",
            appRoleAuthMethodInfo);
        var appRoleClient = new VaultClient(vaultClientSettings);
        await appRoleClient.V1.Auth.PerformImmediateLogin();

        var token = (await appRoleClient.V1.Auth.Token.LookupSelfAsync()).Data.Policies;

        token.Should().ContainMatch("testpolicy");
        apiErrors.Should().Contain("wrapping token is not valid or does not exist");
    }

    [Fact(Skip = "Manual Token creation process")]
    //[Fact]
    public async Task VaultServer_UseUseGitHubAuth_TokenYieldsPolicy()
    {
        const string githubPath = "testGithub";
        const string teamName = "acceliox-developers";
        const string userName = "AEAcceliox";

        const string testSecretPath = "testSecrets";
        const string policyName = "testpolicy";
        const string tempToken = "PERSONAL TOKEN";
        const string rootTokenId = "testRoot";
        const string containerName = "VaultTestsWithoutCLI";
        const int port = 8220;
        const string vaultAdr = "http://127.0.0.1";

        await using var container =
            VaultTestServer.BuildVaultServerContainer(port, rootTokenId: rootTokenId, containerName: containerName);
        await container.StartAsync();
        var rootClient = await CreateVaultRootClient(port);
        await rootClient.V1.System.MountAuthBackendAsync(new AuthMethod
        {
            Path = githubPath, Type = AuthMethodType.GitHub
        });
        // create read only permission policy
        await rootClient.V1.System.WritePolicyAsync(new Policy
        {
            Name = policyName,
            Rules =
                $"# read only permission for test secrets:\r\npath \"{testSecretPath}/data/*\" {{\r\n  capabilities = [\"read\"]\r\n}}"
        });
        // CLI Interactions
        await rootClient.V1.Auth.GitHub.WriteGitHubConfig(
            new GitHubConfig {organization = "acceliox", token_no_default_policy = true}, githubPath);

        var readConfig = await rootClient.V1.Auth.GitHub.ReadGitHubConfig("acceliox", githubPath);

        await rootClient.V1.Auth.GitHub.WriteGitHubTeamMap(new GitHubTeamMap {team_name = teamName, value = policyName},
            githubPath);
        var readTeamMap = await rootClient.V1.Auth.GitHub.ReadGitHubTeamMap(teamName, githubPath);

        await rootClient.V1.Auth.GitHub.WriteGitHubUserMap(new GitHubUserMap {user_name = userName, value = policyName},
            githubPath);
        var readUserMap = await rootClient.V1.Auth.GitHub.ReadGitHubUserMap(userName, githubPath);

        // login with appRole Auth
        IAuthMethodInfo gitHubAuthMethodInfo = new GitHubAuthMethodInfo(githubPath, tempToken);
        var vaultClientSettings = new VaultClientSettings(
            $"{vaultAdr}:{port}",
            gitHubAuthMethodInfo);
        var gitHubClient = new VaultClient(vaultClientSettings);
        await gitHubClient.V1.Auth.PerformImmediateLogin();

        var token = (await gitHubClient.V1.Auth.Token.LookupSelfAsync()).Data.Policies;

        token.Should().ContainMatch("testpolicy");
    }


    private static async Task<IVaultClient> CreateVaultRootClient(int port = 8200, string rootTokenId = "testRoot")
    {
        IAuthMethodInfo authMethod = new TokenAuthMethodInfo(rootTokenId);
        var vaultClientSettings = new VaultClientSettings(
            $"http://127.0.0.1:{port}",
            authMethod);
        return new VaultClient(vaultClientSettings);
    }

    private static async Task MountKv2SecretsEngine(IVaultClient client, string path)
    {
        var kv2SecretsEngine = new SecretsEngine
        {
            Type = SecretsEngineType.KeyValueV2,
            Config = new Dictionary<string, object> {{"version", "2"}},
            Path = path
        };

        await client.V1.System.MountSecretBackendAsync(kv2SecretsEngine);
    }

    private static async Task MountKv1SecretsEngine(IVaultClient client, string path)
    {
        var kv1SecretsEngine = new SecretsEngine
        {
            Type = SecretsEngineType.KeyValueV1,
            Config = new Dictionary<string, object> {{"version", "1"}},
            Path = path
        };

        await client.V1.System.MountSecretBackendAsync(kv1SecretsEngine);
    }
}