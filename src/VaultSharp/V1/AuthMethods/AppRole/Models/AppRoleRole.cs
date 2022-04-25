﻿using System;

namespace VaultSharp.V1.AuthMethods.AppRole.Models
{
    public class AppRoleRole
    {
        /// <summary>
        ///     Require secret_id to be presented when logging in using this AppRole.
        /// </summary>
        public bool bind_secret_id = true;

        /// <summary>
        ///     If set, the secret IDs generated using this role will be cluster local.
        ///     This can only be set during role creation and once set, it can't be reset later.
        /// </summary>
        public bool local_secret_ids = false;

        /// <summary>
        ///     Name of the AppRole. Required
        /// </summary>
        public string role_name;

        /// <summary>
        ///     Comma-separated string or list of CIDR blocks; if set, specifies blocks of IP addresses which can perform the login
        ///     operation.
        /// </summary>
        public string[] secret_id_bound_cidrs = Array.Empty<string>();

        /// <summary>
        ///     Number of times any particular SecretID can be used to fetch a token from this AppRole,
        ///     after which the SecretID will expire. A value of zero will allow unlimited uses.
        /// </summary>
        public int secret_id_num_uses = 0;

        /// <summary>
        ///     Duration in either an integer number of seconds (3600) or an integer time unit (60m) after which any SecretID
        ///     expires
        /// </summary>
        public string secret_id_ttl = "";

        /// <summary>
        ///     List of CIDR blocks; if set, specifies blocks of IP addresses which can authenticate successfully,
        ///     and ties the resulting token to these blocks as well.
        /// </summary>
        public string[] token_blound_cidrs = Array.Empty<string>();

        /// <summary>
        ///     If set, will encode an explicit max TTL onto the token.
        ///     This is a hard cap even if token_ttl and token_max_ttl would otherwise allow a renewal.
        /// </summary>
        public string token_explicit_max_ttl = "";

        /// <summary>
        ///     The maximum lifetime for generated tokens. This current value of this will be referenced at renewal time.
        /// </summary>
        public string token_max_ttl = "";

        /// <summary>
        ///     If set, the default policy will not be set on generated tokens;
        ///     otherwise it will be added to the policies set in token_policies.
        /// </summary>
        public bool token_no_default_policy = false;

        /// <summary>
        ///     The maximum number of times a generated token may be used (within its lifetime); 0 means unlimited.
        ///     If you require the token to have the ability to create child tokens, you will need to set this value to 0.
        /// </summary>
        public int token_num_uses = 0;

        /// <summary>
        ///     The period, if any, to set on the token.
        /// </summary>
        public string token_period = "";

        /// <summary>
        ///     List of policies to encode onto generated tokens. Depending on the auth method, this list may be supplemented by
        ///     user/group/other values.
        /// </summary>
        public string[] token_policies = Array.Empty<string>();

        /// <summary>
        ///     The incremental lifetime for generated tokens. This current value of this will be referenced at renewal time.
        /// </summary>
        public string token_ttl = "";

        /// <summary>
        ///     The type of token that should be generated. Can be service, batch, or default to use the mount's tuned default
        ///     (which unless changed will be service tokens). For token store roles, there are two additional possibilities:
        ///     default-service and default-batch which specify the type to return unless the client requests a different type at
        ///     generation time.
        /// </summary>
        public string token_type = "";
    }
}