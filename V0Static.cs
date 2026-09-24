using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DatasetProcessing.Datas;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DatasetProcessing
{
    public static partial class V0Static
    {
        public static void Main(string[] args)
        {
            var settingsJsonPath = SafeCreateAndPathGet("Sources/Resources/Datas/V0Settings.json", () =>
            {
                var systemsPathsDirectoryBase = "Sources/Resources/Datas";

                var defaultSettings = new V0Settings
                {
                    V0SystemsPathsFileValues = $"{systemsPathsDirectoryBase}/V0ClassesValues.txt",
                    V0SystemsPathsFilesResults = new string[]
                    {
                        $"{systemsPathsDirectoryBase}/V0ClassesResults.txt",
                        $"{systemsPathsDirectoryBase}/V0ClassesResults.yaml",
                    },
                    V0SystemsPathsFileAliasesResults = $"{systemsPathsDirectoryBase}/V0ClassesAliasesResults.txt",
                    V0SystemsPathsFilePriorities = $"{systemsPathsDirectoryBase}/V0Priorities.txt"
                };

                return JsonConvert.SerializeObject(defaultSettings, Formatting.Indented);
            });

            var settings = JsonConvert.DeserializeObject<V0Settings>(File.ReadAllText(settingsJsonPath));

            // Создание необходимых директорий
            EnsureDirectoryExists(settings.V0SystemsPathsFileValues);
            foreach (var item in settings.V0SystemsPathsFilesResults)
            {
                EnsureDirectoryExists(item);
            }
            EnsureDirectoryExists(settings.V0SystemsPathsFileAliasesResults);

            List<string> classes = new List<string>();
            V0SettingsYaml loadedYamlSettings = null;

            var systemsPathsFileClassesValues = SafeCreateAndPathGet(settings.V0SystemsPathsFileValues);
            var extension = Path.GetExtension(systemsPathsFileClassesValues).ToLowerInvariant();

            if (extension == ".txt")
            {
                classes = File.ReadAllLines(systemsPathsFileClassesValues).ToList();
            }
            else if (extension == ".yaml" || extension == ".yml")
            {
                var yamlDeserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();

                var yamlText = File.ReadAllText(systemsPathsFileClassesValues);
                loadedYamlSettings = yamlDeserializer.Deserialize<V0SettingsYaml>(yamlText);
                classes = loadedYamlSettings?.Names?.ToList() ?? new List<string>();
            }

            // Очистка от пустых строк и пробелов
            classes = V0BasesGet(classes).ToList();

            // Обработка через файл приоритетов
            if (File.Exists(settings.V0SystemsPathsFilePriorities))
            {
                var priorities = File.ReadAllLines(settings.V0SystemsPathsFilePriorities);
                priorities = V0BasesGet(priorities).ToArray();

                for (var i = priorities.Length - 1; i >= 0; i--)
                {
                    var priorityItem = priorities[i];
                    var classFindIndex = classes.IndexOf(priorityItem);

                    if (classFindIndex != -1)
                    {
                        classes.RemoveAt(classFindIndex);
                        classes.Insert(0, priorityItem);
                    }
                }
            }

            Console.WriteLine($"Classes after priorities: {classes.Count}");

            // Обработка окончаний (объединение единственного и множественного числа)
            var endingsAliases = V0EndingsProcessingsGet(classes);

            Console.WriteLine($"Classes after endings deduplication: {classes.Count}");

            // Экспорт результатов
            var yamlSerializer = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            foreach (var item in settings.V0SystemsPathsFilesResults)
            {
                var resultExtension = Path.GetExtension(item).ToLowerInvariant();

                if (resultExtension == ".txt")
                {
                    File.WriteAllLines(item, classes);
                }
                else if (resultExtension == ".yaml" || resultExtension == ".yml")
                {
                    var settingsYaml = loadedYamlSettings ?? new V0SettingsYaml();
                    settingsYaml.Names = classes.ToArray();
                    settingsYaml.Nc = settingsYaml.Names.Length;

                    File.WriteAllText(item, yamlSerializer.Serialize(settingsYaml));
                }
            }

            // Сохранение словаря псевдонимов
            File.WriteAllLines(settings.V0SystemsPathsFileAliasesResults, endingsAliases.V0AliasesArrayGet());

            Console.WriteLine($"Final classes: {classes.Count}, Aliases created: {endingsAliases.Count}");
        }

        /// <summary>
        /// Удаление диакритических знаков (акцентов)
        /// </summary>
        public static string V0DiacriticsRemoveAndGet(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var ch in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Дополнительная очистка и форматирование списков классов
        /// </summary>
        public static void V0Processing1()
        {
            var settingsSystemPath = "Sources/Resources/Datas/V0Settings.yaml";
            var jsonSettingsSystemPath = "Sources/Resources/Datas/V0Settings.json";
            var classesExtractedSystemPath = "Sources/Resources/Datas/V0ClassesExtracted.txt";

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var serializer = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            var classes = File.ReadAllLines(SafeCreateAndPathGet("Sources/Resources/Datas/V0Classes.txt"))
                              .Select(v => v.ToLower())
                              .ToList();

            var unique = new HashSet<string>();
            var classesExtracted = File.ReadAllLines(SafeCreateAndPathGet(classesExtractedSystemPath)).ToList();

            var settingsYamlText = File.ReadAllText(SafeCreateAndPathGet(settingsSystemPath));
            var settings = !string.IsNullOrWhiteSpace(settingsYamlText)
                ? deserializer.Deserialize<V0Object>(settingsYamlText)
                : new V0Object();

            var jsonSettingsText = File.ReadAllText(SafeCreateAndPathGet(jsonSettingsSystemPath));
            var jsonSettings = !string.IsNullOrWhiteSpace(jsonSettingsText)
                ? JsonConvert.DeserializeObject<V0CategoriesRoot>(jsonSettingsText)
                : new V0CategoriesRoot();

            for (var i = 0; i < classes.Count; i++)
            {
                var classItem = V0DiacriticsRemoveAndGet(classes[i]);
                var classExtractedItem = i < classesExtracted.Count ? V0DiacriticsRemoveAndGet(classesExtracted[i]) : classItem;

                classes[i] = classItem;
                if (i < classesExtracted.Count)
                {
                    classesExtracted[i] = classExtractedItem;
                }

                if (!unique.Add(classItem) || classItem.Contains(' ') || classItem.Contains('/') || classItem.Contains('_') || classItem.Any(char.IsDigit))
                {
                    classes.RemoveAt(i);
                    if (i < classesExtracted.Count)
                    {
                        classesExtracted.RemoveAt(i);
                    }
                    i--;
                }
                else
                {
                    classes[i] = char.ToUpper(classItem[0]) + classItem.Substring(1);
                    if (i < classesExtracted.Count)
                    {
                        classesExtracted[i] = char.ToUpper(classExtractedItem[0]) + classExtractedItem.Substring(1);
                    }
                }
            }

            settings.names = classes;
            settings.nc = settings.names.Count;

            jsonSettings.categories = settings.names.Select((name, index) => new V0Category
            {
                id = index,
                name = name,
            }).ToList();

            File.WriteAllText(settingsSystemPath, serializer.Serialize(settings));
            File.WriteAllLines(classesExtractedSystemPath, classesExtracted);
            File.WriteAllText(jsonSettingsSystemPath, JsonConvert.SerializeObject(jsonSettings, Formatting.Indented));
        }

        public static void V0Processing2()
        {
            var classes = File.ReadAllLines(SafeCreateAndPathGet("Sources/Resources/Datas/V0Classes.txt"))
                              .Select(v => v.ToLower())
                              .ToList();

            var textInfo = CultureInfo.InvariantCulture.TextInfo;
            var classesOutput = classes.Select(item => textInfo.ToTitleCase(item).Replace(" ", "_")).ToList();

            File.WriteAllLines("Sources/Resources/Datas/V0ClassesOutput.txt", classesOutput);
        }

        /// <summary>
        /// Поиск и слияние форм единственного и множественного числа
        /// </summary>
        public static Dictionary<string, string> V0EndingsProcessingsGet(this List<string> classes)
        {
            var r = new Dictionary<string, string>();

            for (var i1 = 0; i1 < classes.Count; i1++)
            {
                var classItem1 = classes[i1];

                for (var i2 = i1 + 1; i2 < classes.Count; i2++)
                {
                    var classItem2 = classes[i2];

                    if (V0EndingsConditionsIsGet(classItem1, classItem2))
                    {
                        classes.RemoveAt(i1);
                        r[classItem1] = classItem2;
                        i1--;
                        break;
                    }
                    else if (V0EndingsConditionsIsGet(classItem2, classItem1))
                    {
                        classes.RemoveAt(i2);
                        r[classItem2] = classItem1;
                        i2--;
                    }
                }
            }

            return r;
        }

        public static bool V0EndingsConditionsIsGet(this string v1, string v2)
        {
            if (string.IsNullOrEmpty(v1) || string.IsNullOrEmpty(v2)) return false;
            return v2.Contains(v1) && (v1.Length == v2.Length - 1 || v1.Length == v2.Length - 2) && v2.EndsWith("s", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Преобразование текстовых строк разметки сегментации в объекты масок
        /// </summary>
        public static List<V0Mask> V0ConvertFormatToScheme(this IEnumerable<string> lines)
        {
            var masks = new List<V0Mask>();

            foreach (var lineItem in lines)
            {
                if (string.IsNullOrWhiteSpace(lineItem)) continue;

                var tokens = lineItem.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var classConstructor = new List<string>();

                for (var i = 0; i < tokens.Length; i++)
                {
                    var tokenItem = tokens[i];
                    if (char.IsLetter(tokenItem[0]))
                    {
                        classConstructor.Add(tokenItem);
                    }
                    else
                    {
                        break;
                    }
                }

                if (classConstructor.Count == 0 && tokens.Length > 0)
                {
                    classConstructor.Add(tokens[0]);
                }

                var classItem = string.Join(" ", classConstructor);
                var coordTokens = tokens.Skip(classConstructor.Count).ToArray();
                var coords = new List<double[]>();

                for (var i = 0; i < coordTokens.Length - 1; i += 2)
                {
                    if (double.TryParse(coordTokens[i], NumberStyles.Any, CultureInfo.InvariantCulture, out var x) &&
                        double.TryParse(coordTokens[i + 1], NumberStyles.Any, CultureInfo.InvariantCulture, out var y))
                    {
                        coords.Add(new double[] { x, y });
                    }
                }

                var mask = new V0Mask
                {
                    V0Name = classItem,
                    V0Format = "Detection",
                    V0Values = coords.ToArray()
                };

                masks.Add(mask);
            }

            return masks;
        }

        #region Вспомогательные методы расширений

        public static IEnumerable<string> V0BasesGet(IEnumerable<string> items)
        {
            if (items == null) return Enumerable.Empty<string>();
            return items
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim())
                .Distinct();
        }

        public static string SafeCreateAndPathGet(string path, Func<string> defaultContentFactory = null)
        {
            EnsureDirectoryExists(path);

            if (!File.Exists(path))
            {
                var content = defaultContentFactory != null ? defaultContentFactory() : string.Empty;
                File.WriteAllText(path, content);
            }

            return path;
        }

        public static string[] V0AliasesArrayGet(this Dictionary<string, string> aliases)
        {
            return aliases.Select(kv => $"{kv.Key}: {kv.Value}").ToArray();
        }

        private static void EnsureDirectoryExists(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static void V0MasksSchemeClassesToIndexes(this IEnumerable<V0Mask> masks, List<string> classes)
        {
            foreach (var maskItem in masks)
            {
                var findIndex = classes.IndexOf(maskItem.V0Name);
                if (findIndex != -1)
                {
                    maskItem.V0Name = findIndex.ToString();
                }
            }
        }

        public static void V0MasksSchemeClassesAliasesToGenerals(this IEnumerable<V0Mask> masks, Dictionary<string, string> aliases)
        {
            foreach (var maskItem in masks)
            {
                if (aliases.TryGetValue(maskItem.V0Name, out var source))
                {
                    maskItem.V0Name = source;
                }
            }
        }

        #endregion
    }
}