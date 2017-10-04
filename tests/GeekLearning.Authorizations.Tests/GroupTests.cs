﻿namespace GeekLearning.Authorizations.Tests
{
    using EntityFrameworkCore;
    using EntityFrameworkCore.Data;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class GroupTests
    {
        [Fact]
        public async Task CreateGroup_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await authorizationsFixture.AuthorizationsProvisioningClient.CreateGroupAsync("group2", parentGroupName: "group1");
             
                await authorizationsFixture.Context.SaveChangesAsync();

                var group = authorizationsFixture.Context
                    .Groups()
                    .FirstOrDefault(r => r.Name == "group2");
                Assert.NotNull(group);

                var membership = authorizationsFixture.Context.Memberships().FirstOrDefault(m => m.Group.Name == "group1");
                Assert.Equal("group2", ((Group)membership.Principal).Name);
            }
        }

        [Fact]
        public async Task AddPrincipalToGroup_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                var parentGroup = new Group { Name = "group1" };
                var childGroup = new Group { Name = "group2" };
                authorizationsFixture.Context.Groups().Add(parentGroup);
                authorizationsFixture.Context.Groups().Add(childGroup);
                authorizationsFixture.Context.Memberships().Add(new Membership
                {
                    Group = parentGroup,
                    Principal = childGroup
                });

                var scope = new Scope { Name = "Scope1", Description = "Scope1" };
                authorizationsFixture.Context.Scopes().Add(scope);

                var right = new Right { Name = "Right1" };
                authorizationsFixture.Context.Rights().Add(right);

                var role = new Role { Name = "Role1" };
                role.Rights.Add(new RoleRight { Right = right });
                authorizationsFixture.Context.Roles().Add(role);

                authorizationsFixture.Context.Authorizations().Add(new Authorization
                {
                    Scope = scope,
                    Role = role,
                    Principal = childGroup
                });

                await authorizationsFixture.Context.SaveChangesAsync();

                var group = authorizationsFixture.Context
                    .Groups()
                    .FirstOrDefault(r => r.Name == "group2");

                await authorizationsFixture.AuthorizationsProvisioningClient
                    .AddPrincipalToGroupAsync(authorizationsFixture.Context.CurrentUserId, "group2");

                await authorizationsFixture.Context.SaveChangesAsync();

                var membership = authorizationsFixture.Context.Memberships().FirstOrDefault(m => m.PrincipalId == authorizationsFixture.Context.CurrentUserId);
                Assert.NotNull(membership);

                await authorizationsFixture.AuthorizationsEventQueuer.CommitAsync();

                Assert.NotNull(authorizationsFixture.AuthorizationsImpactClient.UserDenormalizedRights[authorizationsFixture.Context.CurrentUserId]);
            }
        }

        [Fact]
        public async Task DeleteGroup_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                var parent = new Group { Name = "group1" };
                var child = new Group { Name = "group2" };
                authorizationsFixture.Context.Groups().Add(parent);
                authorizationsFixture.Context.Groups().Add(child);
                authorizationsFixture.Context.Memberships().Add(new Membership
                {
                    Group = parent,
                    Principal = child
                });
                await authorizationsFixture.Context.SaveChangesAsync();

                var group1FromDb = await authorizationsFixture.Context.Groups().FirstAsync(g => g.Name == "group1");
                var group2FromDb = await authorizationsFixture.Context.Groups().FirstAsync(g => g.Name == "group2");
                await authorizationsFixture.AuthorizationsProvisioningClient.DeleteGroupAsync("group1", withChildren: false);
                await authorizationsFixture.Context.SaveChangesAsync();
                
                Assert.Null(authorizationsFixture.Context.Groups().FirstOrDefault(r => r.Name == "group1"));
                Assert.Null(await authorizationsFixture.Context.Principals().FirstOrDefaultAsync(p => p.Id == group1FromDb.Id));
                Assert.NotNull(authorizationsFixture.Context.Groups().FirstOrDefault(r => r.Name == "group2"));
                Assert.NotNull(await authorizationsFixture.Context.Principals().FirstOrDefaultAsync(p => p.Id == group2FromDb.Id));
            }
        }

        [Fact]
        public async Task DeleteGroupWithChildren_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                var parent = new Group { Name = "group1" };
                var child = new Group { Name = "group2" };
                authorizationsFixture.Context.Groups().Add(parent);
                authorizationsFixture.Context.Groups().Add(child);
                authorizationsFixture.Context.Memberships().Add(new Membership
                {
                    Group = parent,
                    Principal = child
                });
                await authorizationsFixture.Context.SaveChangesAsync();

                var group1FromDb = await authorizationsFixture.Context.Groups().FirstAsync(g => g.Name == "group1");
                var group2FromDb = await authorizationsFixture.Context.Groups().FirstAsync(g => g.Name == "group2");
                await authorizationsFixture.AuthorizationsProvisioningClient.DeleteGroupAsync("group1");
                await authorizationsFixture.Context.SaveChangesAsync();

                Assert.Null(authorizationsFixture.Context.Groups().FirstOrDefault(r => r.Name == "group1"));
                Assert.Null(await authorizationsFixture.Context.Principals().FirstOrDefaultAsync(p => p.Id == group1FromDb.Id));
                Assert.Null(authorizationsFixture.Context.Groups().FirstOrDefault(r => r.Name == "group2"));
                Assert.Null(await authorizationsFixture.Context.Principals().FirstOrDefaultAsync(p => p.Id == group1FromDb.Id));
            }
        }
    }
}
