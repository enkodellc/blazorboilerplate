using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBoilerplate.UI.Base.Shared.Components
{
    /// <summary>
    /// Displays a list of validation messages from a cascaded <see cref="EditContext"/>.
    /// </summary>
    public class ValidationSummaryBase : ComponentBase, IDisposable
    {
        private EditContext _previousEditContext;
        private readonly EventHandler<ValidationStateChangedEventArgs> _validationStateChangedHandler;

        /// <summary>
        /// Gets or sets the model to produce the list of validation messages for.
        /// When specified, this lists all errors that are associated with the model instance.
        /// </summary>
        [Parameter] public object Model { get; set; }

        /// <summary>
        /// Gets or sets a collection of additional attributes that will be applied to the created <c>ul</c> element.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        [CascadingParameter] protected EditContext CurrentEditContext { get; set; } = default!;

        /// <summary>`
        /// Constructs an instance of <see cref="ValidationSummary"/>.
        /// </summary>
        public ValidationSummaryBase()
        {
            _validationStateChangedHandler = (sender, eventArgs) => StateHasChanged();
        }

        /// <inheritdoc />
        protected override void OnParametersSet()
        {
            if (CurrentEditContext == null)
            {
                throw new InvalidOperationException($"{nameof(ValidationSummary)} requires a cascading parameter " +
                    $"of type {nameof(EditContext)}. For example, you can use {nameof(ValidationSummary)} inside " +
                    $"an {nameof(EditForm)}.");
            }

            if (CurrentEditContext != _previousEditContext)
            {
                DetachValidationStateChangedListener();
                CurrentEditContext.OnValidationStateChanged += _validationStateChangedHandler;
                _previousEditContext = CurrentEditContext;
            }

            var validationMessages = Model is null ?
                CurrentEditContext.GetValidationMessages() :
                CurrentEditContext.GetValidationMessages(new FieldIdentifier(Model, string.Empty));
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        void IDisposable.Dispose()
        {
            DetachValidationStateChangedListener();
            Dispose(disposing: true);
        }

        private void DetachValidationStateChangedListener()
        {
            if (_previousEditContext != null)
            {
                _previousEditContext.OnValidationStateChanged -= _validationStateChangedHandler;
            }
        }
    }
}