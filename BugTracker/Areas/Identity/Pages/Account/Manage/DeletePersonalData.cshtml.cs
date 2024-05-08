// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BugTracker.Areas.Identity.Data;
using BugTracker.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BugTracker.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<BugTrackerUser> _userManager;
        private readonly SignInManager<BugTrackerUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;

        public DeletePersonalDataModel(
            UserManager<BugTrackerUser> userManager,
            SignInManager<BugTrackerUser> signInManager,
            ILogger<DeletePersonalDataModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        
        // Property to indicate if password is required for certain actions
        public bool RequirePassword { get; set; }

        // Handles the HTTP GET request for the delete account page
        public async Task<IActionResult> OnGet()
        {
            // Get the current user
            var user = await _userManager.GetUserAsync(User);
            // Return not found if user not found
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Check if user has a password set
            RequirePassword = await _userManager.HasPasswordAsync(user);
            // Return the page
            return Page();
        }

        // Handles the HTTP POST request for deleting the account
        public async Task<IActionResult> OnPostAsync()
        {
            // Get the current user
            var user = await _userManager.GetUserAsync(User);
            // Return not found if user not found
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Check if user has a password set
            RequirePassword = await _userManager.HasPasswordAsync(user);
            // If password is required
            if (RequirePassword)
            {
                // Check if password provided matches user's password
                // Sha 256 turns the password into a hash as the stored password is hashed
                if (!await _userManager.CheckPasswordAsync(user, PasswordHashing.Sha256(Input.Password)))
                {
                    // Add model error if password is incorrect
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    // Return the page with error
                    return Page();
                }
            }

            // Delete the user
            var result = await _userManager.DeleteAsync(user);
            // Get the user ID
            var userId = await _userManager.GetUserIdAsync(user);
            // If deletion fails, throw exception
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user.");
            }

            // Sign out the user
            await _signInManager.SignOutAsync();
            // Log user deletion
            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            // Redirect to home page after deletion
            return Redirect("~/");
        }
    }
}