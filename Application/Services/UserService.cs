using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Shared.Services;

namespace Application.Services
{
    public class UserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IGenericRepository<SavingsAccount> _accountRepo;
        private readonly ILoanRepository _loanRepo;
        private readonly IUserRepository _userRepo;
        private readonly IEmailService _emailService;
        private static readonly Random _random = new Random();

        public UserService(
            UserManager<AppUser> userManager,
            IGenericRepository<SavingsAccount> accountRepo,
            ILoanRepository loanRepo,
            IUserRepository userRepo,
            IEmailService emailService)
        {
            _userManager = userManager;
            _accountRepo = accountRepo;
            _loanRepo = loanRepo;
            _userRepo = userRepo;
            _emailService = emailService;
        }

        // ─── PRIVADO: genera número de cuenta único verificando cuentas Y préstamos ───
        private async Task<string> GenerateAccountNumber()
        {
            string number;
            bool exists;
            do
            {
                number = _random.Next(100_000_000, 999_999_999).ToString();

                var accounts = await _accountRepo.FindAsync(a => a.AccountNumber == number);
                bool inAccounts = accounts.Any();

                bool inLoans = await _loanRepo.IdentifierExistsAsync(number);

                exists = inAccounts || inLoans;
            } while (exists);
            return number;
        }

        // ─── PASO 1: Crear el usuario y retornar su Id (sin enviar correo) ───────────
        // El controlador usa este Id para generar el token y construir el link,
        // luego llama a SendActivationEmail con ese link ya armado.
        public async Task<string> CreateUserAndGetId(
            string firstName,
            string lastName,
            string email,
            string username,
            string cedula,
            string password,
            string role,
            decimal initialAmount)
        {
            var existingEmail = await _userManager.FindByEmailAsync(email);
            if (existingEmail != null)
                throw new Exception("El correo ya está en uso");

            var existingUser = await _userManager.FindByNameAsync(username);
            if (existingUser != null)
                throw new Exception("El nombre de usuario ya está en uso");

            var user = new AppUser
            {
                FirtsName = firstName,
                LastName = lastName,
                Email = email,
                IdentityNumber = cedula,
                UserName = username,
                IsActive = false,
                LockoutEnd = DateTimeOffset.MaxValue,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, role);

            if (role == "Cliente")
            {
                var accountNumber = await GenerateAccountNumber();
                var account = new SavingsAccount
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = user.Id,
                    IsPrincipal = true,
                    IsActive = true,
                    AccountNumber = accountNumber,
                    CreatedAt = DateTime.Now
                };
                account.SetBalance(initialAmount);
                await _accountRepo.AddAsync(account);
                await _accountRepo.SaveChangesAsync();
            }

            return user.Id;   // <-- el controlador usará este Id para generar el link
        }

        // ─── PASO 2: Enviar el correo de activación con el link ya generado ──────────
        // El link lo construye el controlador usando IUrlHelper (capa de presentación).
        // Así UserService no depende de IUrlHelper y se respeta la arquitectura Onion.
        public async Task SendActivationEmail(
            string email,
            string firstName,
            string activationLink)
        {
            await _emailService.SendEmailAsync(
                email,
                "Activa tu cuenta — Artemis Banking",
                $@"<h2>Bienvenido/a a Artemis Banking, {firstName}!</h2>
                   <p>Tu cuenta ha sido creada. Haz clic en el siguiente enlace para activarla:</p>
                   <p>
                     <a href='{activationLink}'
                        style='padding:10px 20px;background:#1a56db;color:white;
                               text-decoration:none;border-radius:4px;'>
                       Activar mi cuenta
                     </a>
                   </p>
                   <p>Si no reconoces esta actividad, ignora este mensaje.</p>");
        }

        // ─── Cambiar estado activo/inactivo ──────────────────────────────────────────
        public async Task ToggleUserStatus(string userId)
        {
            var isCurrentlyActive = await _userRepo.IsUserActiveAsync(userId);
            await _userRepo.SetUserActiveAsync(userId, !isCurrentlyActive);
        }

        // ─── Editar usuario ───────────────────────────────────────────────────────────
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
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("Usuario no encontrado");

            var existingEmail = await _userManager.FindByEmailAsync(email);
            if (existingEmail != null && existingEmail.Id != userId)
                throw new Exception("El correo ya está en uso por otro usuario");

            var existingUsername = await _userManager.FindByNameAsync(username);
            if (existingUsername != null && existingUsername.Id != userId)
                throw new Exception("El nombre de usuario ya está en uso por otro usuario");

            user.FirtsName = firstName;
            user.LastName = lastName;
            user.Email = email;
            user.UserName = username;
            user.IdentityNumber = cedula;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            if (!string.IsNullOrWhiteSpace(password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var pwResult = await _userManager.ResetPasswordAsync(user, token, password);
                if (!pwResult.Succeeded)
                    throw new Exception(string.Join(", ", pwResult.Errors.Select(e => e.Description)));
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