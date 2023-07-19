using NoteTake.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteTake.Interfaces
{
    public interface INoteService
    {
        Task<IEnumerable<Note>> GetNotesAsync();
        Task<Note> GetNoteByIdAsync(Guid id);
        Task<Note> CreateNoteAsync(Note note);
        Task<Note> UpdateNoteAsync(Guid id, Note note);
        Task DeleteNoteAsync(Guid id);
    }
}
