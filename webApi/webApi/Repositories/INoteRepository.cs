﻿using webApi.Model;

namespace webApi.Repositories
{
    public interface INoteRepository
    {
        Task<Note> GetNoteByIdAsync(int id);
        Task<List<Note>> GetAllNotesAsync();
        Task AddNoteAsync(Note note);
        Task UpdateNoteAsync(Note note);
        Task DeleteNoteAsync(int id);
        Task<List<Note>> GetNotesByUserIdAsync(string userId);
        Task<List<Note>> GetNotesByLessonIdAsync(int lessonId);
    }
}
