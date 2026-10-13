
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SinkDrive.Models;

namespace SinkDrive.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Informe seu nome de usuário.")]
        [StringLength(256, MinimumLength = 3,
            ErrorMessage = "O nome de usuário deve ter entre 3 e 256 caracteres.")]
        [RegularExpression(@"^[a-zA-Z0-9_.-]+$",
            ErrorMessage = "Use apenas letras, números, ponto, hífen e sublinhado.")]
        [Display(Name = "Nome de usuário")]
        public string UserName { get; set; } = string.Empty;

        [StringLength(100,
            ErrorMessage = "O nome de exibição pode ter no máximo 100 caracteres.")]
        [Display(Name = "Nome de exibição (opcional)")]
        public string? DisplayName { get; set; }

        [Required(ErrorMessage = "Informe sua senha.")]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme sua senha.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password),
            ErrorMessage = "As senhas não coincidem.")]
        [Display(Name = "Confirmar senha")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = new ApplicationUser
        {
            UserName = Input.UserName,
            DisplayName = string.IsNullOrWhiteSpace(Input.DisplayName)
                ? null
                : Input.DisplayName.Trim()
        };

        var result = await _userManager.CreateAsync(user, Input.Password);

        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToPage("/Index");
        }

        foreach (var error in result.Errors)
        {
            var message = error.Code switch
            {
                "DuplicateUserName" =>
                    "Esse nome de usuário já está em uso.",
                "PasswordTooShort" =>
                    "A senha não atende ao tamanho mínimo exigido.",
                "PasswordRequiresDigit" =>
                    "A senha deve conter pelo menos um número.",
                "PasswordRequiresUpper" =>
                    "A senha deve conter pelo menos uma letra maiúscula.",
                "PasswordRequiresLower" =>
                    "A senha deve conter pelo menos uma letra minúscula.",
                "PasswordRequiresNonAlphanumeric" =>
                    "A senha deve conter pelo menos um caractere especial.",
                _ => error.Description
            };

            ModelState.AddModelError(string.Empty, message);
        }

        return Page();
    }
}
