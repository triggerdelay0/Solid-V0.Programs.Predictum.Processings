using System;
using System.Collections.Generic;

namespace DatasetProcessing.Datas
{
    /// <summary>
    /// Модель конфигурации датасета (YAML)
    /// </summary>
    public class V0Object
    {
        public string train { get; set; }
        public string val { get; set; }
        public int nc { get; set; }
        public List<string> names { get; set; } = new List<string>();
    }

    /// <summary>
    /// Настройки путей проекта
    /// </summary>
    public class V0Settings
    {
        public string V0SystemsPathsFileValues { get; set; }
        public string[] V0SystemsPathsFilesResults { get; set; }
        public string V0SystemsPathsFileAliasesResults { get; set; }
        public string V0SystemsPathsFilePriorities { get; set; }
    }

    /// <summary>
    /// Структура YAML-файла настроек
    /// </summary>
    public class V0SettingsYaml
    {
        public string Train { get; set; }
        public string Val { get; set; }
        public int Nc { get; set; }
        public string[] Names { get; set; } = Array.Empty<string>();
    }
}