using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notes.Api.Contracts;
using Notes.Api.Data;
using Notes.Api.Models;
using Microsoft.Extensions.Logging;

namespace Notes.Api.Tests
{
    public class NotesApiIntegrationTests
    {
        [Fact]
        public async Task CreateAndGetNote_ReturnsCreatedNote()
        {
            await using var factory = new NotesApiFactory();
            using var client = factory.CreateClient();

            var createRequest = new CreateNoteRequest
            {
                Title = "Shopping",
                Content = "Milk, Bread, Eggs"
            };

            var createResponse = await client.PostAsJsonAsync("/api/notes", createRequest);

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var created = await createResponse.Content.ReadFromJsonAsync<Note>();

            Assert.NotNull(created);
            Assert.True(created.Id > 0);
            Assert.Equal("Shopping", created.Title);
            Assert.Equal("Milk, Bread, Eggs", created.Content);

            var getResponse = await client.GetAsync($"/api/notes/{created.Id}");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var fetched = await getResponse.Content.ReadFromJsonAsync<Note>();

            Assert.NotNull(fetched);
            Assert.Equal(created.Id, fetched.Id);
            Assert.Equal("Shopping", fetched.Title);
        }

        [Fact]
        public async Task GetNotes_ReturnsTotalCountHeader()
        {
            await using var factory = new NotesApiFactory();
            using var client = factory.CreateClient();

            await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest
            {
                Title = "Alpha",
                Content = "First note"
            });

            await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest
            {
                Title = "Beta",
                Content = "Second note"
            });

            var response = await client.GetAsync("/api/notes?page=1&pageSize=1&sort=title&dir=asc");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Headers.TryGetValues("X-Total-Count", out var values));
            Assert.Equal("2", values.Single());

            var notes = await response.Content.ReadFromJsonAsync<List<Note>>();

            Assert.NotNull(notes);
            Assert.Single(notes);
            Assert.Equal("Alpha", notes[0].Title);
        }

        [Fact]
        public async Task UpdateAndDeleteNote_UpdatesThenRemovesNote()
        {
            await using var factory = new NotesApiFactory();
            using var client = factory.CreateClient();

            var createResponse = await client.PostAsJsonAsync("/api/notes", new CreateNoteRequest
            {
                Title = "Draft",
                Content = "Initial"
            });

            var created = await createResponse.Content.ReadFromJsonAsync<Note>();
            Assert.NotNull(created);

            var updateResponse = await client.PutAsJsonAsync($"/api/notes/{created.Id}", new UpdateNoteRequest
            {
                Title = "Final",
                Content = "Updated"
            });

            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

            var updated = await updateResponse.Content.ReadFromJsonAsync<Note>();

            Assert.NotNull(updated);
            Assert.Equal("Final", updated.Title);
            Assert.Equal("Updated", updated.Content);
            Assert.NotNull(updated.UpdatedAt);

            var deleteResponse = await client.DeleteAsync($"/api/notes/{created.Id}");

            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var getDeletedResponse = await client.GetAsync($"/api/notes/{created.Id}");

            Assert.Equal(HttpStatusCode.NotFound, getDeletedResponse.StatusCode);
        }

        [Fact]
        public async Task InvalidSort_ReturnsBadRequest()
        {
            await using var factory = new NotesApiFactory();
            using var client = factory.CreateClient();

            var response = await client.GetAsync("/api/notes?sort=invalid");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    internal sealed class NotesApiFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? _connection;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
            });

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(service =>
                    service.ServiceType == typeof(DbContextOptions<AppDb>));

                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                _connection = new SqliteConnection("Data Source=:memory:");
                _connection.Open();

                services.AddDbContext<AppDb>(options =>
                    options.UseSqlite(_connection));

                using var scope = services.BuildServiceProvider().CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDb>();

                db.Database.EnsureCreated();
            });
        }

        public override async ValueTask DisposeAsync()
        {
            if (_connection is not null)
            {
                await _connection.DisposeAsync();
            }

            await base.DisposeAsync();
        }
    }
}