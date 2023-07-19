using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NoteTake.Interfaces;
using NoteTake.Models;

namespace NoteTakeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoteController : ControllerBase
    {
        private readonly INoteService _noteService;
        private readonly ILogger<NoteController> _logger;


        public NoteController(INoteService noteService, ILogger<NoteController> logger)
        {
            _noteService = noteService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotes()
        {
            try
            {
                var notes = await _noteService.GetNotesAsync();
                return Ok(notes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving notes from OneDrive.");
                return StatusCode(500, "An error occurred while retrieving notes.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNoteById(Guid id)
        {
            try
            {
                var note = await _noteService.GetNoteByIdAsync(id);
                if (note == null)
                {
                    return NotFound();
                }

                return Ok(note);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving the note from OneDrive.");
                return StatusCode(500, "An error occurred while retrieving the note.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] Note note)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var createdNote = await _noteService.CreateNoteAsync(note);
                    return CreatedAtAction(nameof(GetNoteById), new { id = createdNote.Id }, createdNote);
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating the note in OneDrive.");
                return StatusCode(500, "An error occurred while creating the note.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(Guid id, [FromBody] Note note)
        {
            try
            {
                var updatedNote = await _noteService.UpdateNoteAsync(id, note);
                if (updatedNote == null)
                {
                    return NotFound();
                }

                return Ok(updatedNote);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating the note in OneDrive.");
                return StatusCode(500, "An error occurred while updating the note.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(Guid id)
        {
            try
            {
                await _noteService.DeleteNoteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting the note from OneDrive.");
                return StatusCode(500, "An error occurred while deleting the note.");
            }
        }
    }
}

