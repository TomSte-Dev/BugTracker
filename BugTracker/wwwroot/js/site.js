// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    $('#sidebar-toggler').click(function () {
        $('#left-sidebar').toggle();
    });

    $(window).resize(function () {
        if ($(window).width() > 768) { // Adjust the width threshold as needed
            $('#left-sidebar').show();
        }
    });
});