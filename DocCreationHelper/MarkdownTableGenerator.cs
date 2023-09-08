using System.Reflection;
using System.Text;
using Information.DbDocumentation.XmlDocExtractor;

namespace DocCreationHelper;

public class MarkdownTableGenerator : ITableGenerator
{
    private readonly XmlDocExtractor _xmlDocExtractor;

    public MarkdownTableGenerator(XmlDocExtractor xmlDocExtractor)
    {
        this._xmlDocExtractor = xmlDocExtractor ?? throw new ArgumentNullException(nameof(xmlDocExtractor));
    }

    public string GenerateTable(object parseObject)
    {
        var fullPathClass = parseObject.GetType();
        var outputBuilder = new StringBuilder();

        // Имя класса и его описание из XML-документации
        var className = fullPathClass.Name;
        var classDescription = _xmlDocExtractor.GetClassSummary(fullPathClass.FullName);

        // Выводим имя класса и его описание в формате Markdown
        outputBuilder.AppendLine($"Type: {className}");
        outputBuilder.AppendLine($"Description: {classDescription}");
        outputBuilder.AppendLine(); // Пустая строка для разделения

        // Заголовок таблицы Markdown
        outputBuilder.AppendLine("| Field | Type | isNullable | Description |");
        outputBuilder.AppendLine("| --- | --- | --- | --- |");

        foreach (var prop in fullPathClass.GetProperties(BindingFlags.Public | BindingFlags.Instance |
                                                         BindingFlags.GetProperty))
        {
            var fullPath = $"{fullPathClass.FullName}.{prop.Name}";
            var propertyTypeName = $"{prop.PropertyType.Name}";
            var propertyIsNullable = "";

            if (prop.PropertyType.IsGenericType &&
                prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                propertyTypeName = $"{prop.PropertyType.GetGenericArguments()[0]}";
                propertyIsNullable = "Yes";
            }

            var name = prop.Name;

            // Используем xmlDocExtractor для получения описания свойства из XML-документации
            var propertyDescription = _xmlDocExtractor.GetPropertySummary(fullPath);

            // Добавляем строку с информацией о свойстве в таблицу
            outputBuilder.AppendLine(
                $"| {name} | {propertyTypeName} | {propertyIsNullable} | {propertyDescription}");
        }

        return outputBuilder.ToString();
    }
}