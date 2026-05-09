using IoAssistant.Database.Models;
using SQLite;

namespace IoAssistant.Database.Repositories;

public class ProjectRepository
{
    private readonly SQLiteAsyncConnection db;

    public ProjectRepository(string dbPath)
    {
        db = new SQLiteAsyncConnection(dbPath);
    }

    public async Task InitializeAsync()
    {
        await db.CreateTableAsync<ProjectEntity>().ConfigureAwait(false);
    }

    public Task AddAsync(ProjectEntity project) =>
        db.InsertAsync(project);

    public Task DeleteAsync(ProjectEntity project) =>
        db.DeleteAsync(project);

    public Task DeleteByIdAsync(Guid id) =>
        db.DeleteAsync<ProjectEntity>(id);

    public Task DeleteAllAsync() =>
        db.DeleteAllAsync<ProjectEntity>();

    public Task UpsertAsync(ProjectEntity project) =>
        db.InsertOrReplaceAsync(project);

    public Task<List<ProjectEntity>> GetAllAsync() =>
        db.Table<ProjectEntity>().ToListAsync();

    public Task<ProjectEntity> GetByIdAsync(Guid id) =>
        db.FindAsync<ProjectEntity>(id);
}
