using System.Reflection;
using Information.DbDocumentation.XmlDocExtractor;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DocCreationHelper
{
    public class YamlTableGenerator : ITableGenerator
    {
        private readonly XmlDocExtractor _xmlDocExtractor;

        public YamlTableGenerator(XmlDocExtractor xmlDocExtractor)
        {
            this._xmlDocExtractor = xmlDocExtractor ?? throw new ArgumentNullException(nameof(xmlDocExtractor));
        }

        public string GenerateTable(object parseObject)
        {
            if (parseObject == null)
            {
                throw new ArgumentNullException(nameof(parseObject));
            }

            var fullPathClass = parseObject.GetType();
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            // Создаем объект YAML с узлом info
            var yamlObject = new
            {
                info = new
                {
                    type = fullPathClass.Name,
                    description = _xmlDocExtractor.GetClassSummary(fullPathClass.FullName)
                },
                properties = GetPropertyDescriptions(fullPathClass, parseObject)
            };

            // Сериализуем объект YAML
            var yaml = serializer.Serialize(yamlObject);

            // Возвращаем описание класса parseObject в формате YAML
            return yaml;
        }

        public string GenerateTableFromYaml(string yaml)
        {
            if (string.IsNullOrEmpty(yaml))
            {
                throw new ArgumentException("YAML cannot be null or empty.", nameof(yaml));
            }

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var yamlObject = deserializer.Deserialize<YamlObject>(yaml);

            var asciidocBuilder = new System.Text.StringBuilder();

            // Добавляем заголовок документа Asciidoc
            asciidocBuilder.AppendLine("= " + yamlObject.info.type);
            asciidocBuilder.AppendLine(yamlObject.info.description);
            asciidocBuilder.AppendLine();

            // Создаем таблицу Asciidoc для свойств
            asciidocBuilder.AppendLine("[options=\"header\"]");
            asciidocBuilder.AppendLine("|===");
            asciidocBuilder.AppendLine("| Field | Type | isNullable | Description | Example");

            foreach (var property in yamlObject.properties)
            {
                asciidocBuilder.AppendLine(
                    $"| {property.Key} |{property.Value.type} | {property.Value.isNullable} | {property.Value.description} | {property.Value.example}");
            }

            asciidocBuilder.AppendLine("|===");

            return asciidocBuilder.ToString();
        }

        private Dictionary<string, PropertyInfoDescription> GetPropertyDescriptions(Type type, object parseObject)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
            var propertyDescriptions = new Dictionary<string, PropertyInfoDescription>();

            foreach (var prop in properties)
            {
                var fullPath = $"{type.FullName}.{prop.Name}";
                var propertyTypeName = $"{prop.PropertyType.Name}";
                var propertyIsNullable = "";

                if (prop.PropertyType.IsGenericType &&
                    prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    propertyTypeName = $"{prop.PropertyType.GetGenericArguments()[0]}";
                    propertyIsNullable = "Yes";
                }

                var name = ToCamelCase(prop.Name);

                // Используем xmlDocExtractor для получения описания свойства из XML-документации
                var propertyDescription = _xmlDocExtractor.GetPropertySummary(fullPath);

                // Получаем значение поля из parseObject и преобразуем в JSON строку с учетом настроек
                var example = JsonConvert.SerializeObject(prop.GetValue(parseObject), new JsonSerializerSettings
                {
                    Formatting = Formatting.None,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    NullValueHandling = NullValueHandling.Ignore,
                });

                // Создаем объект для описания свойства и добавляем его в словарь
                propertyDescriptions[name] = new PropertyInfoDescription
                {
                    type = propertyTypeName,
                    isNullable = propertyIsNullable,
                    description = propertyDescription,
                    example = example
                };
            }

            return propertyDescriptions;
        }

// Метод для преобразования строки в формат CamelCase
        private string ToCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return char.ToLower(input[0]) + input.Substring(1);
        }

        private class PropertyInfoDescription
        {
            public string type { get; set; } = string.Empty;
            public string isNullable { get; set; } = string.Empty;
            public string description { get; set; } = string.Empty;
            public string example { get; set; } = string.Empty; // Свойство для примера
        }

        private class YamlObject
        {
            public Info info { get; set; }
            public Dictionary<string, PropertyInfoDescription> properties { get; set; }
        }

        private class Info
        {
            public string type { get; set; }
            public string description { get; set; } = string.Empty;
        }
    }
}