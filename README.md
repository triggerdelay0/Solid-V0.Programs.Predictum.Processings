# V0.Programs.Predictum.Processings

Утилита командной строки на C# / .NET для автоматизированной подготовки, очистки, нормализации и дедупликации меток (классов) датасетов компьютерного зрения (Object Detection & Segmentation).

Проект предназначен для обработки исходных списков классов: приводит метки к единому регистру, устраняет формы множественного числа, сортирует классы по заданному приоритету и генерирует конфигурационные файлы для обучения моделей и разметки.

---

## Возможности

1. **Дедупликация единственного и множественного числа:**
   * Автоматически находит пары слов (например, `car` / `cars`, `box` / `boxes`).
   * Удаляет дубликат из итогового списка классов и сохраняет соответствие в таблицу псевдонимов (alias map).

2. **Приоритизация меток:**
   * Позволяет через файл приоритетов зафиксировать выбранные классы на начальных индексах (ID 0, 1, 2...), что необходимо для фиксации порядка идентификаторов классов в модели.

3. **Очистка текста и удаление диакритики:**
   * Удаляет акценты и диакритические знаки (например, `cafe` вместо `café`).
   * Вспомогательные методы фильтруют строки с пробелами, спецсимволами (`/`, `_`) и цифрами.

4. **Синхронизация форматов:**
   * **YAML:** формирует файл конфигурации датасета (`nc`, `names`, `train`, `val`).
   * **JSON:** генерирует структуру категорий стандарта COCO (`id`, `name`).
   * **TXT:** сохраняет очищенный плоский список меток и список псевдонимов (`alias: canonical_name`).

5. **Парсинг разметки:**
   * Преобразует текстовые строки сегментационных полигонов в типизированные объекты координат `V0Mask`.

---

## Требования к окружению

* .NET 8.0 SDK (или .NET 6.0+)
* Зависимости NuGet:
  * `Newtonsoft.Json` (версия 13.0.3 или выше)
  * `YamlDotNet` (версия 16.0.0 или выше)

---

## Структура файлов проекта

```text
├── V0.Programs.Predictum.Processings.csproj # Файл проекта .NET
├── V0Static.cs                             # Точка входа Main и методы обработки данных
├── V0Object.cs                             # Модели конфигураций датасета (YAML и пути Settings)
├── V0CategoriesRoot.cs                     # Модели COCO (JSON) и модель масок (V0Mask)
└── Sources/
    └── Resources/
        └── Datas/                         # Рабочая директория (создается автоматически)
            ├── V0Settings.json             # Конфигурация путей
            ├── V0ClassesValues.txt         # Входной список классов
            └── V0Priorities.txt            # (Опционально) Список приоритетных классов
```

---

## Файл проекта (V0.Programs.Predictum.Processings.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>disable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageReference Include="YamlDotNet" Version="16.0.0" />
  </ItemGroup>

</Project>
```

---

## Сборка и запуск

### 1. Подготовка
Разместите проект в требуемую рабочую директорию и перейдите в неё в терминале.

### 2. Восстановление зависимостей
```bash
dotnet restore
```

### 3. Сборка проекта
```bash
dotnet build -c Release
```

### 4. Запуск утилиты
```bash
dotnet run
```

При первом запуске программа автоматически создаст структуру каталогов `Sources/Resources/Datas/` и сгенерирует файл настроек по умолчанию `V0Settings.json`.

---

## Конфигурация (V0Settings.json)

Файл создается автоматически со следующими параметрами:

```json
{
  "V0SystemsPathsFileValues": "Sources/Resources/Datas/V0ClassesValues.txt",
  "V0SystemsPathsFilesResults": [
    "Sources/Resources/Datas/V0ClassesResults.txt",
    "Sources/Resources/Datas/V0ClassesResults.yaml"
  ],
  "V0SystemsPathsFileAliasesResults": "Sources/Resources/Datas/V0ClassesAliasesResults.txt",
  "V0SystemsPathsFilePriorities": "Sources/Resources/Datas/V0Priorities.txt"
}
```

### Описание параметров:

| Параметр | Описание |
| :--- | :--- |
| `V0SystemsPathsFileValues` | Путь к входному файлу со списком исходных классов (`.txt` или `.yaml`). |
| `V0SystemsPathsFilesResults` | Массив путей, куда сохраняются результирующие списки классов (`.txt`, `.yaml`). |
| `V0SystemsPathsFileAliasesResults` | Путь к файлу, куда записывается таблица соответствий псевдонимов. |
| `V0SystemsPathsFilePriorities` | Путь к файлу со списком приоритетных классов. |

---

## Пример работы

### 1. Входной файл (V0ClassesValues.txt):
```text
pedestrian
cars
car
traffic_light
bicycles
bicycle
```

### 2. Файл приоритетов (V0Priorities.txt):
```text
traffic_light
```

### 3. Результаты работы:

* **V0ClassesResults.txt** (классы очищены от дубликатов множественного числа, приоритетный класс перемещен в начало):
  ```text
  traffic_light
  pedestrian
  cars
  bicycles
  ```

* **V0ClassesAliasesResults.txt** (таблица замены устаревших имен на канонические):
  ```text
  car: cars
  bicycle: bicycles
  ```

* **V0ClassesResults.yaml** (готовый конфигурационный файл):
  ```yaml
  nc: 4
  names:
  - traffic_light
  - pedestrian
  - cars
  - bicycles
  ```