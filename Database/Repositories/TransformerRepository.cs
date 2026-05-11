using IoAssistant.Database.Models;
using SQLite;

namespace IoAssistant.Database.Repositories;

public class TransformerRepository
{
    private readonly SQLiteAsyncConnection db;

    public TransformerRepository(string dbPath)
    {
        db = new SQLiteAsyncConnection(dbPath);
    }

    public async Task InitializeAsync()
    {
        await db.CreateTableAsync<TransformerEntity>().ConfigureAwait(false);
    }

    public Task AddAsync(TransformerEntity transformer) =>
        db.InsertAsync(transformer);

    public Task DeleteAsync(TransformerEntity transformer) =>
        db.DeleteAsync(transformer);

    public Task DeleteByIdAsync(Guid id) =>
        db.DeleteAsync<TransformerEntity>(id);

    public Task DeleteAllAsync() =>
        db.DeleteAllAsync<TransformerEntity>();

    public Task UpsertAsync(TransformerEntity transformer) =>
        db.InsertOrReplaceAsync(transformer);

    public Task<List<TransformerEntity>> GetByProjectAsync(Guid projectId) =>
        db.Table<TransformerEntity>().Where(t => t.ProjectId == projectId).ToListAsync();

    public Task<List<TransformerEntity>> GetAllAsync() =>
        db.Table<TransformerEntity>().ToListAsync();

    public Task<TransformerEntity> GetByIdAsync(Guid id) =>
        db.FindAsync<TransformerEntity>(id);
}
