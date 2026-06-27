using System.Linq;
using CattleFarm.Authorization;
using CattleFarm.Models;
using Xunit;

namespace CattleFarm.Tests
{
    public class AuthorizationTests
    {
        [Fact]
        public void AppRoles_AllContainsDefinedRoles()
        {
            Assert.Contains(AppRoles.Admin, AppRoles.All);
            Assert.Contains(AppRoles.Manager, AppRoles.All);
            Assert.Contains(AppRoles.Owner, AppRoles.All);
            Assert.Contains(AppRoles.Doctor, AppRoles.All);
            Assert.Contains(AppRoles.Worker, AppRoles.All);
            Assert.Contains(AppRoles.Customer, AppRoles.All);
        }

        [Fact]
        public void AppRoles_FarmRolesContainsValidFarmingRoles()
        {
            Assert.Contains(AppRoles.Admin, AppRoles.FarmRoles);
            Assert.Contains(AppRoles.Manager, AppRoles.FarmRoles);
            Assert.Contains(AppRoles.Owner, AppRoles.FarmRoles);
            Assert.DoesNotContain(AppRoles.Worker, AppRoles.FarmRoles);
            Assert.DoesNotContain(AppRoles.Customer, AppRoles.FarmRoles);
        }

        [Theory]
        [InlineData("Admin", true)]
        [InlineData("Owner", true)]
        [InlineData("Manager", true)]
        [InlineData("Worker", false)]
        [InlineData("Customer", false)]
        public void AppRoles_VerifyFarmOperatorPrivilege(string roleName, bool isOperator)
        {
            var operatorsList = AppRoles.FarmOperators.Split(',');
            var contains = operatorsList.Contains(roleName);
            Assert.Equal(isOperator, contains);
        }
    }
}
