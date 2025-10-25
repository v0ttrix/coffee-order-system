// ------------------------------------------------------------
// ValidationResult.cs  — quick overview
// goal: tiny return type for validation work. Now holds:
// - IsValid: did it pass validation rules?
// - Errors: reasons it failed (empty if valid)
// - Warnings: softer "heads up" notes (like allergen info)
// the why: I want to warn about tree nuts (almond milk) without
// blocking a valid order. Keeping API additions non-breaking.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace CoffeeOrder.Validation
{
    public sealed class ValidationResult
    {
        //true = I passed validation (no hard errors)
        public bool IsValid { get; }

        //strict reasons I failed validation (empty if valid)
        public IReadOnlyList<string> Errors { get; }

        //softer messages that don’t fail the item (like allergens for example)
        public IReadOnlyList<string> Warnings { get; }

        //existing ctor kept so older calls still compile fine
        public ValidationResult(bool isValid, IEnumerable<string> errors)
            : this(isValid, errors, Array.Empty<string>()) // chain to the new ctor with empty warnings
        {
            //nothing else here. I will centralize the real work in the 3-arg ctor
        }

        //new ctor that also accepts warnings
        public ValidationResult(bool isValid, IEnumerable<string> errors, IEnumerable<string> warnings)
        {
            //store flags and normalize nulls so callers dont have to do checks
            IsValid  = isValid;
            Errors   = (errors   ?? Array.Empty<string>()).ToArray();
            Warnings = (warnings ?? Array.Empty<string>()).ToArray();
        }

        //success with no warnings
        public static ValidationResult Ok()
            => new ValidationResult(true, Array.Empty<string>(), Array.Empty<string>());

        //success with warnings (new helper)
        public static ValidationResult OkWithWarnings(IEnumerable<string> warnings)
            => new ValidationResult(true, Array.Empty<string>(), warnings ?? Array.Empty<string>());

        //failure with errors (no warnings)
        public static ValidationResult Fail(params string[] errors)
            => new ValidationResult(false, errors ?? Array.Empty<string>(), Array.Empty<string>());

        //failure with errors + warnings (new helper)
        public static ValidationResult FailWithWarnings(IEnumerable<string> errors, IEnumerable<string> warnings)
            => new ValidationResult(false, errors ?? Array.Empty<string>(), warnings ?? Array.Empty<string>());
    }
}