using System;
using System.Collections.Generic;
using System.Reflection;
using Information.DbDocumentation.XmlDocExtractor;
using YamlDotNet.Serialization;

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
            var serializer = new SerializerBuilder().Build();

            // Создаем объект YAML с узлом info
            var yamlObject = new
            {
                info = new
                {
                    type = fullPathClass.Name,
                    description = _xmlDocExtractor.GetClassSummary(fullPathClass.FullName)
                },
                properties = GetPropertyDescriptions(fullPathClass)
            };

            // Сериализуем объект YAML
            var yaml = serializer.Serialize(yamlObject);

            // Возвращаем описание класса parseObject в формате YAML
            return yaml;
        }

        private Dictionary<string, PropertyInfoDescription> GetPropertyDescriptions(Type type)
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

                var name = prop.Name;

                // Используем xmlDocExtractor для получения описания свойства из XML-документации
                var propertyDescription = _xmlDocExtractor.GetPropertySummary(fullPath);

                // Создаем объект для описания свойства и добавляем его в словарь
                propertyDescriptions[name] = new PropertyInfoDescription
                {
                    type = propertyTypeName,
                    isNullable = propertyIsNullable,
                    description = propertyDescription
                };
            }

            return propertyDescriptions;
        }

        private class PropertyInfoDescription
        {
            public string type { get; set; } = string.Empty;
            public string isNullable { get; set; } = string.Empty;
            public string description { get; set; } = string.Empty;
        }
    }
}