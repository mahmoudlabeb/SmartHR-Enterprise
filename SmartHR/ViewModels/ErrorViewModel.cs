using System;

using System.ComponentModel.DataAnnotations;

namespace SmartHR.Models
{
    /// <summary>
    /// Error View Model - Used to display error information to users
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Unique identifier for the request that caused the error
        /// Used for logging and debugging purposes
        /// </summary>
        [Display(Name = "Request ID")]
        public string? RequestId { get; set; }

        /// <summary>
        /// Determines whether to show the request ID in error messages
        /// Only displayed if RequestId is not null or empty
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}