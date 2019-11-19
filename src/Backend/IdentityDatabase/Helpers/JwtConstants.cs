using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityDatabase.Helpers
{
    public static class JwtClaimIdentifiers
    {
        public const string Rol = "rol", Id = "id";
    }

    public static class JwtClaims
    {
        public const string DefaultRole = "guest";

        public const string AdminRole = "admin";
    }

}

