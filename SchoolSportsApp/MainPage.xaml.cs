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

            // ����� ���� � ���� ������ ��� ��������
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "school_sports.db");
            Console.WriteLine($"Database Path: {dbPath}");

            try
            {
                LoadData(); // �������� ������ ��� �������������
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
                DisplayAlert("������", "�� ������� ��������� ������ �� ���� ������.", "OK");
            }

            // ������������� ������ ��� Picker
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
                    // ��������� ������ ��� Picker
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
            // ��������� ���������
            BranchPicker.ItemsSource = new List<string>
            {
                "�������", "���������������", "�����������������", "��������", "�������"
            };

            // ��������� ������
            ClassPicker.ItemsSource = new List<string>
            {
                "5-�", "5-�", "6-�", "6-�", "7-�"
            };

            // ��������� ��������
            StudentPicker.ItemsSource = new List<string>
            {
                "������", "������", "�������"
            };
        }

        private async void OnImportExcelClicked(object sender, EventArgs e)
        {
            try
            {
                // ����������� ������ ��� ������ ������ ������ Excel
                var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.Android, new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } }, // .xlsx
            { DevicePlatform.iOS, new[] { "com.microsoft.excel.xlsx" } }, // iOS MIME ���
            { DevicePlatform.WinUI, new[] { ".xlsx" } } // Windows
        });

                var pickOptions = new PickOptions
                {
                    PickerTitle = "�������� ���� Excel",
                    FileTypes = customFileType
                };

                // ��������� ������ ��� ������ �����
                var fileResult = await FilePicker.Default.PickAsync(pickOptions);

                if (fileResult != null)
                {
                    // �������� ���� � �����
                    var filePath = fileResult.FullPath;

                    // ��������� ������ �� Excel
                    using var workbook = new XLWorkbook(filePath);
                    var worksheet = workbook.Worksheet(1); // ������ �������

                    var students = new List<Student>();

                    // ������������, ��� ������ ���������� � ������ ������
                    foreach (var row in worksheet.RowsUsed().Skip(1)) // ���������� ���������
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

                    // ��������� �������� � ���� ������
                    foreach (var student in students)
                    {
                        await _databaseService.SaveStudentAsync(student);
                    }

                    await DisplayAlert("�����", $"������������� ��������: {students.Count}", "OK");
                    LoadData(); // ��������� ������ �� ������
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"������ �������: {ex.Message}");
                await DisplayAlert("������", "�� ������� ������������� ������ �� Excel.", "OK");
            }
        }

        private async void OnAddNormativeClicked(object sender, EventArgs e)
        {
            try
            {
                if (BranchPicker.SelectedItem == null || ClassPicker.SelectedItem == null || StudentPicker.SelectedItem == null)
                {
                    await DisplayAlert("������", "��������� ��� ���� ��� ���������� ���������.", "OK");
                    return;
                }

                // ����� ����� ����������� �������� ����� �������� ��� ���������� ���������
                await DisplayAlert("����������", "������� ���������� ��������� ���� �� �����������.", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding normative: {ex.Message}");
                await DisplayAlert("������", "��������� ������ ��� ���������� ���������.", "OK");
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
                    await DisplayAlert("������", "��������� ��� ���� ����� ����������� ����������.", "OK");
                    return;
                }

                // �������� ���������� �������
                var students = await _databaseService.GetStudentsAsync();
                var selectedStudent = students[selectedStudentIndex];

                // ����� ����� ����������� ���������� ���������� � ���� ������
                Console.WriteLine($"Saving result: Student={selectedStudent.LastName}, Normative={selectedNormative}, Result={result}");

                await DisplayAlert("�����", "��������� ������� ��������.", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving result: {ex.Message}");
                await DisplayAlert("������", "�� ������� ��������� ���������.", "OK");
            }
        }
    }
}