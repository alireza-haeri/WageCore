using Microsoft.AspNetCore.Components.Forms;
using System.Reflection;
using Web.Ui.Models;

namespace Web.Ui.Services;

public class FormHelper<TModel> : IDisposable where TModel : BaseModel, new()
{
    public TModel Model { get; set; }
    public EditContext? EditContext { get; private set; } = null!;
    
    // ✅ حذف readonly برای اینکه در Rebuild دوباره مقداردهی شود
    private ValidationMessageStore _validationMessageStore = null!; 
    private readonly Dictionary<string, string> _fieldMapping = new();

    public FormHelper(TModel? model = null, Dictionary<string, string>? fieldMapping = null)
    {
        Model = model ?? new TModel();
        
        if (fieldMapping == null)
        {
            foreach (var propertyInfo in Model.GetType().GetProperties())
            {
                _fieldMapping[propertyInfo.Name] = propertyInfo.Name;
            }
        }
        else
        {
            _fieldMapping = fieldMapping;
        }

        InitializeEditContext();
    }

    private void InitializeEditContext()
    {
        if (EditContext != null)
        {
            EditContext.OnFieldChanged -= HandleFieldChanged;
        }

        EditContext = new EditContext(Model);
        EditContext.AddDataAnnotationsValidation(); // فعال‌سازی ولیدیشن توکار
        _validationMessageStore = new ValidationMessageStore(EditContext);
        
        EditContext.OnFieldChanged += HandleFieldChanged;
    }

    private void HandleFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        _validationMessageStore.Clear(args.FieldIdentifier);
        EditContext?.NotifyValidationStateChanged();
    }

    public void RebuildEditContext()
    {
        InitializeEditContext();
    }

    public void SetServerErrors(IDictionary<string, ICollection<string>> errors)
    {
        foreach (var errorGroup in errors)
        {
            var fieldName = errorGroup.Key;
            var propertyName = _fieldMapping.GetValueOrDefault(fieldName, fieldName);

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                propertyName = "General";
            }

            var propertyInfo = typeof(TModel).GetProperty(propertyName,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            var fieldIdentifier = propertyInfo != null
                ? new FieldIdentifier(Model, propertyInfo.Name)
                : new FieldIdentifier(Model, "General");

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
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (propertyInfo == null)
            return Array.Empty<string>();

        var fieldIdentifier = new FieldIdentifier(Model, propertyInfo.Name);
        return _validationMessageStore[fieldIdentifier];
    }

    public void Dispose()
    {
        if (EditContext != null)
        {
            EditContext.OnFieldChanged -= HandleFieldChanged;
        }
    }
}