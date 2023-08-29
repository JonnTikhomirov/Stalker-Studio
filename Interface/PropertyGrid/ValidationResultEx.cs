﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Stalker_Studio
{
    /// <summary>
    /// Adds severity in ValidationResult
    /// </summary>
    /// <seealso cref="System.ComponentModel.DataAnnotations.ValidationResult" />
    public class ValidationResultEx : ValidationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResultEx"/> class.
        /// </summary>
        /// <param name="errorMessage"></param>
        public ValidationResultEx(string errorMessage) : base(errorMessage) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResultEx"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="memeberNames">The memeber names.</param>
        public ValidationResultEx(string errorMessage, IEnumerable<string> memeberNames) : base(errorMessage, memeberNames) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResultEx"/> class.
        /// </summary>
        /// <param name="validationResult">The validation result object.</param>
        public ValidationResultEx(ValidationResult validationResult) : base(validationResult) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResultEx"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="severity">The severity, default is Error.</param>
        public ValidationResultEx(string errorMessage, Severity severity = Severity.Error) : base(errorMessage)
        {
            this.Severity = severity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResultEx"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="memeberNames">The memeber names.</param>
        /// <param name="severity">The severity, default is Error</param>
        public ValidationResultEx(string errorMessage, IEnumerable<string> memeberNames, Severity severity = Severity.Error) : base(errorMessage, memeberNames)
        {
            this.Severity = severity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResultEx"/> class.
        /// </summary>
        /// <param name="validationResult">The validation result.</param>
        /// <param name="severity">The severity, default is error</param>
        public ValidationResultEx(ValidationResult validationResult, Severity severity = Severity.Error) : base(validationResult)
        {
            this.Severity = severity;
        }

        /// <summary>
        /// Gets or sets the severity.
        /// </summary>
        /// <value>
        /// The severity.
        /// </value>
        public Severity Severity { get; set; } = Severity.Error;
    }

    /// <summary>
    /// Severity for validation message
    /// </summary>
    public enum Severity
    {
        /// <summary>
        /// The error
        /// </summary>
        Error,

        /// <summary>
        /// The warning
        /// </summary>
        Warning,

        /// <summary>
        /// The information
        /// </summary>
        Information //TODO: can be removed if not needed
    }
}
