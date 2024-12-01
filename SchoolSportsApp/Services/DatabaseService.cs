using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SQLite;
using SchoolSportsApp.Models;

namespace SchoolSportsApp.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService()
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "school_sports.db");
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Student>().ConfigureAwait(false); // Без блокировки потока
        }

        public Task<List<Student>> GetStudentsAsync() => _database.Table<Student>().ToListAsync();

        public Task<int> SaveStudentAsync(Student student) => _database.InsertAsync(student);

    }
}