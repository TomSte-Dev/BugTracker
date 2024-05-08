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
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<BugTrackerUser> _userManager;
        private readonly SignInManager<BugTrackerUser> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;

        public ChangePasswordModel(
            UserManager<BugTrackerUser> userManager,
            SignInManager<BugTrackerUser> signInManager,
            ILogger<ChangePasswordModel> logger)
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
        [TempData]
        public string StatusMessage { get; set; }

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
            [Display(Name = "Current password")]
            public string OldPassword { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        // Handles HTTP GET requests asynchronously to load the page for changing the user's password.
        public async Task<IActionResult> OnGetAsync()
        {
            // Retrieves the current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Returns a 404 Not Found response if the user cannot be found
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Checks if the user has a password set
            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                // Redirects to the "SetPassword" page if the user does not have a password set
                return RedirectToPage("./SetPassword");
            }

            // Returns the page for changing the password
            return Page();
        }

        // Handles HTTP POST requests asynchronously to change the user's password.
        public async Task<IActionResult> OnPostAsync()
        {
            // Checks if the model state is valid
            if (!ModelState.IsValid)
            {
                // Returns the page with validation errors if the model state is not valid
                return Page();
            }

            // Retrieves the current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Returns a 404 Not Found response if the user cannot be found
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Attempts to change the user's password
            // As the stored passwords are hashed this hashes the inputed passwords for checking and storing
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, PasswordHashing.Sha256(Input.OldPassword), PasswordHashing.Sha256(Input.NewPassword));
            if (!changePasswordResult.Succeeded)
            {
                // Adds model errors for each password change error
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                // Returns the page with model errors if the password change fails
                return Page();
            }

            // Refreshes the user's sign-in cookie
            await _signInManager.RefreshSignInAsync(user);
            // Logs information about the successful password change
            _logger.LogInformation("User changed their password successfully.");
            // Sets a status message indicating that the password has been changed
            StatusMessage = "Your password has been changed.";

            // Redirects to the current page
            return RedirectToPage();
        }
    }
}