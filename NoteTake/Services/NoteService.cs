// NoteService.cs
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using NoteTake.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NoteTake.Models;

namespace NoteTake.Services
{
    public class NoteService : INoteService
    {
        private readonly IConfidentialClientApplication _confidentialClientApp;
        private readonly GraphServiceClient _graphServiceClient;
        private readonly ILogger<NoteService> _logger;

        public NoteService(
            IConfidentialClientApplication confidentialClientApp,
            GraphServiceClient graphServiceClient,
            ILogger<NoteService> logger)
        {
            _confidentialClientApp = confidentialClientApp;
            _graphServiceClient = graphServiceClient;
            _logger = logger;
        }

        public async Task<IEnumerable<Note>> GetNotesAsync()
        {
            try
            {
                var notes = new List<Note>();

                var driveItems = await _graphServiceClient.Me.Drive.Root.ItemWithPath("Notes").Children.Request().GetAsync();

                foreach (var item in driveItems)
                {
                    var note = new Note
                    {
                        Id = Guid.Parse(item.Id),
                        Title = item.Name,
                    };

                    notes.Add(note);
                }

                return notes;
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Failed to retrieve notes from OneDrive.");
                throw;
            }
        }

        public async Task<Note> GetNoteByIdAsync(Guid id)
        {
            try
            {
                var driveItem = await _graphServiceClient.Me.Drive.Items[id.ToString()].Request().GetAsync();

                var note = new Note
                {
                    Id = Guid.Parse(driveItem.Id),
                    Title = driveItem.Name,
                    Content = driveItem.Content.ToString(),
                };

                return note;
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Failed to retrieve the note from OneDrive.");
                throw;
            }
        }

        public async Task<Note> CreateNoteAsync(Note note)
        {
            try
            {
                byte[] contentBytes = Encoding.UTF8.GetBytes(note.Content);


                var driveItem = new DriveItem
                {
                    Name = note.Title,
                    File = new Microsoft.Graph.File { MimeType = "text/plain" },
                    AdditionalData = new Dictionary<string, object>
            {
                {"@microsoft.graph.conflictBehavior", "rename"}
            }
                };


                var createdItem = await _graphServiceClient.Me.Drive.Root.Children
                    .Request()
                    .AddAsync(driveItem);


                using (var stream = new MemoryStream(contentBytes))
                {
                    await _graphServiceClient.Drives[createdItem.ParentReference.DriveId]
                        .Items[createdItem.Id]
                        .Content
                        .Request()
                        .PutAsync<DriveItem>(stream);
                }

                var createdNote = new Note
                {
                    Id = Guid.Parse(createdItem.Id),
                    Title = createdItem.Name,
                    Content = note.Content,
                };

                return createdNote;
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Failed to create the note in OneDrive.");
                throw;
            }
        }


        public async Task<Note> UpdateNoteAsync(Guid id, Note note)
        {
            try
            {
                var existingNote = await _graphServiceClient.Me.Drive.Items[id.ToString()].Request().GetAsync();

                existingNote.Name = note.Title;

                var updatedItem = await _graphServiceClient.Me.Drive.Items[id.ToString()].Request().UpdateAsync(existingNote);

                var updatedNote = new Note
                {
                    Id = Guid.Parse(updatedItem.Id),
                    Title = updatedItem.Name,
                };

                return updatedNote;
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Failed to update the note in OneDrive.");
                throw;

            }
        }

        public async Task DeleteNoteAsync(Guid id)
        {
            try
            {

                await _graphServiceClient.Me.Drive.Items[id.ToString()]
                    .Request()
                    .DeleteAsync();
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Failed to delete the note from OneDrive.");
                throw;
            }
        }

    }
}
