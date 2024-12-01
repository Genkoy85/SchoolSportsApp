using ClosedXML.Excel;
using System.Linq;
using System.Collections.Generic;
using SchoolSportsApp.Models;
using SchoolSportsApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace SchoolSportsApp
{
    public partial class MainPage : ContentPage
    {
        private DatabaseService _databaseService;

        public MainPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();

            // Вывод пути к базе данных для проверки
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "school_sports.db");
            Console.WriteLine($"Database Path: {dbPath}");

            try
            {
                LoadData(); // Загрузка данных при инициализации
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
                DisplayAlert("Ошибка", "Не удалось загрузить данные из базы данных.", "OK");
            }

            // Инициализация данных для Picker
            InitializePickers();
        }

        private async void LoadData()
        {
            try
            {
                var students = await _databaseService.GetStudentsAsync();
                StudentListView.ItemsSource = students;

                if (students != null && students.Any())
                {
                    // Обновляем данные для Picker
                    StudentPicker.ItemsSource = students.Select(s => $"{s.LastName} {s.FirstName} {s.MiddleName}").ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading students: {ex.Message}");
            }
        }

        private void InitializePickers()
        {
            // Заполняем отделения
            BranchPicker.ItemsSource = new List<string>
            {
                "Ясенево", "Университетский", "Молодогвардейская", "Фотиевой", "Донская"
            };

            // Примерные классы
            ClassPicker.ItemsSource = new List<string>
            {
                "5-А", "5-Б", "6-А", "6-Б", "7-А"
            };

            // Примерные учеников
            StudentPicker.ItemsSource = new List<string>
            {
                "Петров", "Иванов", "Соколов"
            };
        }

        private async void OnImportExcelClicked(object sender, EventArgs e)
        {
            try
            {
                // Настраиваем фильтр для выбора только файлов Excel
                var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.Android, new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } }, // .xlsx
            { DevicePlatform.iOS, new[] { "com.microsoft.excel.xlsx" } }, // iOS MIME тип
            { DevicePlatform.WinUI, new[] { ".xlsx" } } // Windows
        });

                var pickOptions = new PickOptions
                {
                    PickerTitle = "Выберите файл Excel",
                    FileTypes = customFileType
                };

                // Открываем диалог для выбора файла
                var fileResult = await FilePicker.Default.PickAsync(pickOptions);

                if (fileResult != null)
                {
                    // Получаем путь к файлу
                    var filePath = fileResult.FullPath;

                    // Загружаем данные из Excel
                    using var workbook = new XLWorkbook(filePath);
                    var worksheet = workbook.Worksheet(1); // Первая вкладка

                    var students = new List<Student>();

                    // Предполагаем, что данные начинаются с первой строки
                    foreach (var row in worksheet.RowsUsed().Skip(1)) // Пропускаем заголовки
                    {
                        var student = new Student
                        {
                            LastName = row.Cell(1).GetValue<string>(),
                            FirstName = row.Cell(2).GetValue<string>(),
                            MiddleName = row.Cell(3).GetValue<string>(),
                            DateOfBirth = row.Cell(4).GetValue<DateTime>(),
                            Class = row.Cell(5).GetValue<string>(),
                            Branch = row.Cell(6).GetValue<string>()
                        };

                        students.Add(student);
                    }

                    // Сохраняем учеников в базу данных
                    foreach (var student in students)
                    {
                        await _databaseService.SaveStudentAsync(student);
                    }

                    await DisplayAlert("Успех", $"Импортировано учеников: {students.Count}", "OK");
                    LoadData(); // Обновляем список на экране
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка импорта: {ex.Message}");
                await DisplayAlert("Ошибка", "Не удалось импортировать данные из Excel.", "OK");
            }
        }

        private async void OnAddNormativeClicked(object sender, EventArgs e)
        {
            try
            {
                if (BranchPicker.SelectedItem == null || ClassPicker.SelectedItem == null || StudentPicker.SelectedItem == null)
                {
                    await DisplayAlert("Ошибка", "Заполните все поля для добавления норматива.", "OK");
                    return;
                }

                // Здесь можно реализовать открытие новой страницы для добавления норматива
                await DisplayAlert("Информация", "Функция добавления норматива пока не реализована.", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding normative: {ex.Message}");
                await DisplayAlert("Ошибка", "Произошла ошибка при добавлении норматива.", "OK");
            }
        }

        private async void OnSaveResultClicked(object sender, EventArgs e)
        {
            try
            {
                var selectedStudentIndex = StudentPicker.SelectedIndex;
                var selectedNormative = NormativePicker.SelectedItem?.ToString();
                var result = ResultEntry.Text;

                if (selectedStudentIndex == -1 || string.IsNullOrEmpty(selectedNormative) || string.IsNullOrEmpty(result))
                {
                    await DisplayAlert("Ошибка", "Заполните все поля перед сохранением результата.", "OK");
                    return;
                }

                // Получаем выбранного ученика
                var students = await _databaseService.GetStudentsAsync();
                var selectedStudent = students[selectedStudentIndex];

                // Здесь можно реализовать сохранение результата в базе данных
                Console.WriteLine($"Saving result: Student={selectedStudent.LastName}, Normative={selectedNormative}, Result={result}");

                await DisplayAlert("Успех", "Результат успешно сохранен.", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving result: {ex.Message}");
                await DisplayAlert("Ошибка", "Не удалось сохранить результат.", "OK");
            }
        }
    }
}