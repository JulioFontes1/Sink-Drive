
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SinkDrive.Models;

namespace SinkDrive.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Informe seu nome de usuário.")]
        [Display(Name = "Nome de usuário")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe sua senha.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Manter conectado")]
        public bool RememberMe { get; set; }
    }

    public void OnGet()
    {
        ReturnUrl ??= Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await _signInManager.PasswordSignInAsync(
            Input.UserName,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: false
        );

        if (result.Succeeded)
        {
            var destination = Url.IsLocalUrl(ReturnUrl)
                ? ReturnUrl!
                : Url.Content("~/");

            return LocalRedirect(destination);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                string.Empty,
                "Sua conta está temporariamente bloqueada."
            );
        }
        else
        {
            ModelState.AddModelError(
                string.Empty,
                "Nome de usuário ou senha inválidos."
            );
        }

        return Page();
    }
}
