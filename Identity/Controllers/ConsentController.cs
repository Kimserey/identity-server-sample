using Identity.ViewModels;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Controllers
{
    public class ConsentController : Controller
    {
        private readonly IClientStore _clientStore;
        private readonly IResourceStore _resourceStore;
        private readonly TestUserStore _users;
        private readonly IIdentityServerInteractionService _interaction;

        public ConsentController(IIdentityServerInteractionService interaction, IClientStore clientStore, IResourceStore resourceStore, TestUserStore users)
        {
            _interaction = interaction;
            _clientStore = clientStore;
            _resourceStore = resourceStore;
            _users = users;
        }
        private async Task<Client> GetClient(string returnUrl)
        {
            var authorizationContext = await _interaction
                .GetAuthorizationContextAsync(returnUrl);

            return authorizationContext?.ClientId != null
                ? await _clientStore.FindEnabledClientByIdAsync(authorizationContext.ClientId)
                : null;
        }

        private async Task<Resources> GetResources(string returnUrl)
        {
            var authorizationContext = await _interaction
                .GetAuthorizationContextAsync(returnUrl);

            return await _resourceStore.FindEnabledResourcesByScopeAsync(
                authorizationContext.ScopesRequested);
        }

        [HttpGet]
        public async Task<IActionResult> Index(string returnUrl)
        {
            var client = await GetClient(returnUrl);
            var resources = await GetResources(returnUrl);

            return View(new ConsentViewModel
            {
                ReturnUrl = returnUrl,
                ClientName = client.ClientName,
                ClientUrl = client.ClientUri,
                ClientLogoUrl = client.LogoUri,
                AllowRememberConsent = client.AllowRememberConsent,
                IdentityScopes = resources.IdentityResources.Select(id =>
                {
                    return new ScopeViewModel
                    {
                        Name = id.Name,
                        DisplayName = id.DisplayName,
                        Description = id.Description,
                        Emphasize = id.Emphasize,
                        Required = id.Required,
                        Checked = true
                    };
                }),
                ResourceScopes = resources.ApiResources.SelectMany(x => x.Scopes).Select(id =>
                {
                    return new ScopeViewModel
                    {
                        Name = id.Name,
                        DisplayName = id.DisplayName,
                        Description = id.Description,
                        Emphasize = id.Emphasize,
                        Required = id.Required,
                        Checked = true
                    };
                })
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ConsentInputModel model)
        {
            if (model.Consent)
            {
                if (!_interaction.IsValidReturnUrl(model.ReturnUrl)
                    || model.ScopesConsented == null
                    || !model.ScopesConsented.Any())
                {
                    return View("Error");
                }

                var authorizationContext = await _interaction
                    .GetAuthorizationContextAsync(model.ReturnUrl);

                if (authorizationContext == null)
                {
                    return View("Error");
                }

                var request = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
                await _interaction.GrantConsentAsync(request, new ConsentResponse
                {
                    RememberConsent = model.RememberConsent,
                    ScopesConsented = model.ScopesConsented
                });
                return Redirect(model.ReturnUrl);
            }
            else
            {
                var authorizationContext = await _interaction
                .GetAuthorizationContextAsync(model.ReturnUrl);

                var request = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

                await _interaction.GrantConsentAsync(request, ConsentResponse.Denied);
                return Redirect(model.ReturnUrl);
            }
        }
    }
}
