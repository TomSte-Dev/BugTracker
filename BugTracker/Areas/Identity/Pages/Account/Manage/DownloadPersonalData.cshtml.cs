// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.Text.Json;
using BugTracker.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BugTracker.Areas.Identity.Pages.Account.Manage;

public class DownloadPersonalDataModel : PageModel
{
    private readonly UserManager<BugTrackerUser> _userManager;
    private readonly ILogger<DownloadPersonalDataModel> _logger;

    // Constructor for DownloadPersonalDataModel, accepting UserManager and ILogger through dependency injection
    public DownloadPersonalDataModel(
        UserManager<BugTrackerUser> userManager,
        ILogger<DownloadPersonalDataModel> logger)
    {
        _userManager = userManager; // Assigning the injected UserManager
        _logger = logger; // Assigning the injected ILogger
    }

    // Handles HTTP GET requests
    public IActionResult OnGet()
    {
        return NotFound();
    }

    // HandlesHTTP POST requests, used for downloading personal data
    public async Task<IActionResult> OnPostAsync()
    {
        // Retrieve the current user
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            // If the user is not found, return a 404 Not Found response with an error message
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        // Log the user's request for their personal data
        _logger.LogInformation("User with ID '{UserId}' asked for their personal data.", _userManager.GetUserId(User));

        // Prepare a dictionary to store the user's personal data
        var personalData = new Dictionary<string, string>();

        // Retrieve properties marked with the PersonalDataAttribute and add them to the personalData dictionary
        var personalDataProps = typeof(BugTrackerUser).GetProperties().Where(
                        prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
        foreach (var p in personalDataProps)
        {
            personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
        }

        // Retrieve external logins associated with the user and add them to the personalData dictionary
        var logins = await _userManager.GetLoginsAsync(user);
        foreach (var l in logins)
        {
            personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
        }

        // Add the authenticator key (if enabled) to the personalData dictionary
        personalData.Add($"Authenticator Key", await _userManager.GetAuthenticatorKeyAsync(user));

        // Set the response headers to indicate that a file download is being returned
        Response.Headers.TryAdd("Content-Disposition", "attachment; filename=PersonalData.json");

        // Return a FileContentResult containing the personalData dictionary serialized as JSON
        return new FileContentResult(JsonSerializer.SerializeToUtf8Bytes(personalData), "application/json");
    }
}