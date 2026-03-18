using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public class UserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IGenericRepository<SavingsAccount> _accountRepo;
        private static readonly Random _random = new Random();

        public UserService(
            UserManager<AppUser> userManager,
            IGenericRepository<SavingsAccount> accountRepo)
        {
            _userManager = userManager;
            _accountRepo = accountRepo;
        }

        private async Task<string> GenerateAccountNumber()
        {
            string number;
            bool exists;

            do
            {
                number = _random.Next(100000000, 999999999).ToString();

                var accounts = await _accountRepo.FindAsync(a => a.AccountNumber == number);
                exists = accounts.Any();

            } while (exists);

            return number;
        }
        public async Task CreateUser(
            string firstName,
            string lastName,
            string email,
            string username,
            string password,
            string role,
            decimal initialAmount = 0)
        {
            var existingEmail = await _userManager.FindByEmailAsync(email);
            if (existingEmail != null)
                throw new Exception("El correo ya está en uso");
            var existingUser = await _userManager.FindByNameAsync(username);
            if (existingUser != null)
                throw new Exception("El usuario ya existe");

            var user = new AppUser
            {
                FirtsName = firstName,
                LastName = lastName,
                Email = email,
                UserName = username,
                LockoutEnd = DateTimeOffset.MaxValue,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, role);

            if (role == "Cliente")
            {
                var account = new SavingsAccount
                {
                    UserId = user.Id,
                    IsPrincipal = true,
                    AccountNumber = await GenerateAccountNumber()
                };

                account.SetBalance(initialAmount);

                await _accountRepo.AddAsync(account);
                await _accountRepo.SaveChangesAsync();
            }
        }

        public async Task ToggleUserStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new Exception("Usuario no encontrado");

            user.LockoutEnd = user.LockoutEnd == null
                ? DateTimeOffset.MaxValue
                : null;

            await _userManager.UpdateAsync(user);
        }

        public async Task UpdateUser(
            string userId,
            string firstName,
            string lastName,
            string email,
            string username,
            string cedula,
            string password,
            decimal? additionalAmount)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new Exception("Usuario no encontrado");

            var existingEmail = await _userManager.FindByEmailAsync(email);
            if (existingEmail != null && existingEmail.Id != userId)
                throw new Exception("El correo ya está en uso");

            var existingUsername = await _userManager.FindByNameAsync(username);
            if (existingUsername != null && existingUsername.Id != userId)
                throw new Exception("El usuario ya existe");

            user.FirtsName = firstName;
            user.LastName = lastName;
            user.Email = email;
            user.UserName = username;
            user.IdentityNumber = cedula;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            if (!string.IsNullOrEmpty(password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, token, password);
            }

            if (additionalAmount.HasValue && additionalAmount > 0)
            {
                var account = (await _accountRepo.FindAsync(a =>
                    a.UserId == userId && a.IsPrincipal)).FirstOrDefault();

                if (account != null)
                {
                    account.SetBalance(account.Balance + additionalAmount.Value);
                    _accountRepo.Update(account);
                    await _accountRepo.SaveChangesAsync();
                }
            }
        }
    }
}