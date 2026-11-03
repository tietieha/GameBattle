#nullable enable

using System;

namespace AbilityKit.Demo.Common.Rooms
{
    public sealed class DemoMultiplayerAccountState
    {
        private readonly string _defaultAccountPrefix;
        private readonly string _defaultGuestPrefix;
        private readonly string _reservedSessionToken;
        private string _loggedAccountId = string.Empty;
        private string _sessionAccountId = string.Empty;
        private string _identitySuffix = string.Empty;

        public DemoMultiplayerAccountState(string? defaultAccountPrefix, string? defaultGuestPrefix, string? reservedSessionToken = "")
        {
            _defaultAccountPrefix = NormalizePrefix(defaultAccountPrefix, "unity-account");
            _defaultGuestPrefix = NormalizePrefix(defaultGuestPrefix, "unity-guest");
            _reservedSessionToken = reservedSessionToken ?? string.Empty;
        }

        public string LoggedAccountId => _loggedAccountId;

        public void RecordLogin(string? accountId)
        {
            _loggedAccountId = accountId ?? string.Empty;
            _sessionAccountId = _loggedAccountId;
        }

        public bool HasSessionToken(string? sessionToken, string? accountId)
        {
            return !string.IsNullOrWhiteSpace(sessionToken)
                && !string.Equals(sessionToken, _reservedSessionToken, StringComparison.Ordinal)
                && string.Equals(_sessionAccountId, accountId, StringComparison.Ordinal);
        }

        public void ClearSession()
        {
            _loggedAccountId = string.Empty;
            _sessionAccountId = string.Empty;
        }

        public void EnsureUniqueDefaultIdentity(ref string accountId, ref string guestId)
        {
            EnsureIdentitySuffix();
            if (IsEmptyOrDefaultPrefix(accountId, _defaultAccountPrefix))
            {
                accountId = CreateDefaultAccountId();
            }

            if (IsEmptyOrDefaultPrefix(guestId, _defaultGuestPrefix))
            {
                guestId = CreateDefaultGuestId();
            }
        }

        public string CreateDefaultAccountId()
        {
            EnsureIdentitySuffix();
            return $"{_defaultAccountPrefix}-{_identitySuffix}";
        }

        public string CreateDefaultGuestId()
        {
            EnsureIdentitySuffix();
            return $"{_defaultGuestPrefix}-{_identitySuffix}";
        }

        private static string NormalizePrefix(string? value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static bool IsEmptyOrDefaultPrefix(string? value, string defaultPrefix)
        {
            return string.IsNullOrWhiteSpace(value)
                || string.Equals(value.Trim(), defaultPrefix, StringComparison.Ordinal);
        }

        private void EnsureIdentitySuffix()
        {
            if (string.IsNullOrWhiteSpace(_identitySuffix))
            {
                _identitySuffix = Guid.NewGuid().ToString("N").Substring(0, 8);
            }
        }
    }
}
