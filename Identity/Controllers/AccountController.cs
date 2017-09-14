using Identity.Models;
using Identity.ViewModels;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Controllers
{
    public class AccountController : Controller
    {
        private readonly IClientStore _clientStore;
        private readonly TestUserStore _users;
        private readonly IIdentityServerInteractionService _interaction;

        public AccountController(IIdentityServerInteractionService interaction, IClientStore clientStore, TestUserStore users)
        {
            _interaction = interaction;
            _clientStore = clientStore;
            _users = users;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            var authorizationContext = await _interaction
                .GetAuthorizationContextAsync(returnUrl);

            var client = authorizationContext?.ClientId != null
                ? await _clientStore.FindEnabledClientByIdAsync(authorizationContext.ClientId)
                : null;

            var providers = authorizationContext?.IdP != null ?
                new ExternalProvider[] { new ExternalProvider { AuthenticationScheme = authorizationContext.IdP } }
                : HttpContext
                    .Authentication
                    .GetAuthenticationSchemes()
                    .Where(x => x.DisplayName != null)
                    .Where(x => client == null || client != null && client.IdentityProviderRestrictions.Contains(x.AuthenticationScheme))
                    .Select(x => new ExternalProvider { DisplayName = x.DisplayName, AuthenticationScheme = x.AuthenticationScheme });

            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalProviders = providers
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model)
        {
            if (ModelState.IsValid)
            {
                var result = _users.ValidateCredentials(model.Username, model.Password);
                if (result)
                {
                    var user = _users.FindByUsername(model.Username);

                    await HttpContext.Authentication.SignInAsync(
                        user.SubjectId, 
                        user.Username,
                        model.RememberLogin ? new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.Add(TimeSpan.FromHours(1))
                        }
                        : null
                    );

                    if (_interaction.IsValidReturnUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }

                ModelState.AddModelError("", "Wrong username or password.");
            }

            var authorizationContext = await _interaction
                .GetAuthorizationContextAsync(model.ReturnUrl);

            var client = authorizationContext?.ClientId != null
                ? await _clientStore.FindEnabledClientByIdAsync(authorizationContext.ClientId)
                : null;

            var providers = authorizationContext?.IdP != null ?
                new ExternalProvider[] { new ExternalProvider { AuthenticationScheme = authorizationContext.IdP } }
                : HttpContext
                    .Authentication
                    .GetAuthenticationSchemes()
                    .Where(x => x.DisplayName != null)
                    .Where(x => client == null || client != null && client.IdentityProviderRestrictions.Contains(x.AuthenticationScheme))
                    .Select(x => new ExternalProvider { DisplayName = x.DisplayName, AuthenticationScheme = x.AuthenticationScheme });


            return View(new LoginViewModel
            {
                Username = model.Username,
                Password = model.Password,
                ReturnUrl = model.ReturnUrl,
                RememberLogin = model.RememberLogin,
                ExternalProviders = providers
            });
        }
    }
}
