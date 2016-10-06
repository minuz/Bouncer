﻿using GeekLearning.Authorizations.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GeekLearning.Authorizations.Tests
{
    public class AuthorizationsTest
    {
        [Fact]
        public async Task AffectRoleOnScope_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                authorizationsFixture.Context.Set<Role>().Add(new Role { Name = "role1" });

                authorizationsFixture.Context.Set<Scope>().Add(new Scope { Name = "scope1", Description = "Scope 1"});

                authorizationsFixture.Context.SaveChanges();

                await authorizationsFixture.AuthorizationsProvisioningClient
                                           .AffectRoleToPrincipalOnScopeAsync(
                                                "role1",
                                                authorizationsFixture.Context.CurrentUserId,
                                                "scope1");

                var authorization = authorizationsFixture.Context.Set<Authorization>()
                                                                 .Include(a => a.Scope)
                                                                 .Include(a => a.Role)
                                                                 .FirstOrDefault(a => a.PrincipalId == authorizationsFixture.Context.CurrentUserId);

                Assert.NotNull(authorization);
                Assert.Equal("role1", authorization.Role.Name);
                Assert.Equal("scope1", authorization.Scope.Name);
            }
        }
    }
}
