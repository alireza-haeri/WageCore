using Microsoft.AspNetCore.Components.Forms;
using Web.Ui.Models;

namespace Web.Ui.Services;

public class FormHelper<TModel> where TModel : BaseModel, new()
{
    public TModel Model { get; set; }
    public EditContext EditContext { get; set; }
    private readonly ValidationMessageStore _validationMessageStore;
    private readonly Dictionary<string, string> _fieldMapping = new();

    public FormHelper(TModel? model = null, Dictionary<string, string>? fieldMapping = null)
    {
        Model = model ?? new TModel();
        EditContext = new EditContext(Model);
        EditContext.AddDataAnnotationsValidation();
        _validationMessageStore = new ValidationMessageStore(EditContext);
        
        if (fieldMapping == null)
        {
            foreach (var propertyInfo in Model.GetType().GetProperties())
            {
                _fieldMapping.Add(propertyInfo.Name, propertyInfo.Name);
            }
        }
        else
            _fieldMapping = fieldMapping;

        EditContext.OnFieldChanged += (sender, args) =>
        {
            _validationMessageStore.Clear(args.FieldIdentifier);
            EditContext.NotifyValidationStateChanged();
        };
    }

    public void SetServerErrors(IDictionary<string, ICollection<string>> errors)
    {
        foreach (var errorGroup in errors)
        {
            var fieldName = errorGroup.Key;
            var propertyName = _fieldMapping.GetValueOrDefault(fieldName, fieldName);
            var propertyInfo = typeof(TModel).GetProperty(propertyName,
                System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance);

            var fieldIdentifier = propertyInfo != null
                ? new FieldIdentifier(Model, propertyInfo.Name)
                : new FieldIdentifier(Model, string.Empty);

            _validationMessageStore.Clear(fieldIdentifier);

            foreach (var error in errorGroup.Value)
            {
                _validationMessageStore.Add(fieldIdentifier, error);
            }
        }

        EditContext.NotifyValidationStateChanged();
    }

    public void ClearErrors()
    {
        _validationMessageStore.Clear();
        EditContext.NotifyValidationStateChanged();
    }

    public bool Validate() => EditContext.Validate();

    public IEnumerable<string> GetErrorsForField(string fieldName)
    {
        var propertyInfo = typeof(TModel).GetProperty(fieldName,
            System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance);

        if (propertyInfo == null)
            return [];

        var fieldIdentifier = new FieldIdentifier(Model, propertyInfo.Name);

        var messages = _validationMessageStore[fieldIdentifier];
        return messages ?? [];
    }
}