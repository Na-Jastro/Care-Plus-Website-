using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hospital_Management_System.Tests
{
    [TestClass]
    public class AccountControllerTests
    {
        private List<ApplicationUser> GetSampleUsers() => new List<ApplicationUser>()
        {
            new ApplicationUser { Id = "1", Email = "admin@test.com", Role = "Admin" },
            new ApplicationUser { Id = "2", Email = "doctor@test.com", Role = "Doctor" },
            new ApplicationUser { Id = "3", Email = "patient@test.com", Role = "Patient" },
            new ApplicationUser { Id = "4", Email = "locked@test.com", Role = "Locked" }
        };

        [TestMethod]
        public async Task Login_Success_For_Admin()
        {
            var controller = new AccountControllerSimulation(GetSampleUsers());
            var result = await controller.PasswordSignInAsync("admin@test.com", "password");
            Assert.AreEqual(SignInStatus.Success, result);
        }

        [TestMethod]
        public async Task Login_Failure_InvalidUser()
        {
            var controller = new AccountControllerSimulation(GetSampleUsers());
            var result = await controller.PasswordSignInAsync("unknown@test.com", "password");
            Assert.AreEqual(SignInStatus.Failure, result);
        }

        [TestMethod]
        public async Task Login_LockedOutUser()
        {
            var controller = new AccountControllerSimulation(GetSampleUsers());
            var result = await controller.PasswordSignInAsync("locked@test.com", "password");
            Assert.AreEqual(SignInStatus.LockedOut, result);
        }

        [TestMethod]
        public async Task Register_NewPatient_Success()
        {
            var users = GetSampleUsers();
            var controller = new AccountControllerSimulation(users);
            var model = new RegisterViewModel
            {
                Email = "newpatient@test.com",
                UserName = "newpatient",
                Password = "password",
                UserRole = "Patient"
            };

            var result = await controller.RegisterAsync(model);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task Register_DuplicateEmail_Fails()
        {
            var users = GetSampleUsers();
            var controller = new AccountControllerSimulation(users);
            var model = new RegisterViewModel
            {
                Email = "admin@test.com",
                UserName = "admin2",
                Password = "password",
                UserRole = "Admin"
            };

            var result = await controller.RegisterAsync(model);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ResetPassword_ExistingUser_Succeeds()
        {
            var users = GetSampleUsers();
            var controller = new AccountControllerSimulation(users);

            var result = await controller.ResetPasswordAsync("doctor@test.com", "newpassword");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ResetPassword_NonExistingUser_Fails()
        {
            var users = GetSampleUsers();
            var controller = new AccountControllerSimulation(users);

            var result = await controller.ResetPasswordAsync("unknown@test.com", "newpassword");

            Assert.IsFalse(result);
        }
    }

    // Keep your simulation classes
    public class LoginViewModel { public string Email { get; set; } public string Password { get; set; } public bool RememberMe { get; set; } }
    public class RegisterViewModel { public string Email { get; set; } public string UserName { get; set; } public string Password { get; set; } public string UserRole { get; set; } }
    public class ApplicationUser { public string Id { get; set; } = Guid.NewGuid().ToString(); public string Email { get; set; } public string UserName { get; set; } public string Role { get; set; } }
    public enum SignInStatus { Success, LockedOut, RequiresVerification, Failure }
    public class AccountControllerSimulation
    {
        private List<ApplicationUser> _users;
        public AccountControllerSimulation(List<ApplicationUser> users) { _users = users; }
        public Task<SignInStatus> PasswordSignInAsync(string email, string password)
        {
            var user = _users.FirstOrDefault(u => u.Email == email && password == "password");
            if (user == null) return Task.FromResult(SignInStatus.Failure);
            if (user.Role == "Locked") return Task.FromResult(SignInStatus.LockedOut);
            return Task.FromResult(SignInStatus.Success);
        }
        public Task<bool> RegisterAsync(RegisterViewModel model)
        {
            if (_users.Any(u => u.Email == model.Email)) return Task.FromResult(false);
            _users.Add(new ApplicationUser { Email = model.Email, UserName = model.UserName, Role = model.UserRole });
            return Task.FromResult(true);
        }
        public Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            var user = _users.FirstOrDefault(u => u.Email == email);
            return Task.FromResult(user != null);
        }
    }
}
