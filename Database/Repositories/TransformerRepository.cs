using CommunityToolkit.Mvvm.Messaging;
using IoAssistant.Database.Models;
using IoAssistant.Infrastructure.Services;
using IoAssistant.PnP.Interfaces;
using Microsoft.Extensions.Logging;
using SQLite;

namespace IoAssistant.Database.Repositories;

public class TransformerRepository
{
    private readonly SQLiteAsyncConnection db;
    private readonly ILogger<TransformerRepository> logger;

    public TransformerRepository(string dbPath)
    {
        db = new SQLiteAsyncConnection(dbPath);

        logger = AppService.GetRequiredService<ILogger<TransformerRepository>>();

        WeakReferenceMessenger.Default.Register<IOnTransformerPropertyChangedMessage>(this, async void (recipient, m) =>
        {
            try
            {
                var record = new TransformerRecord
                {
                    BelongToId = m.CalculationEngine.BelongToId,
                    Id = m.CalculationEngine.Id,
                    Name = m.CalculationEngine.Name,
                    Description = m.CalculationEngine.Description,
                    ProjectId = m.CalculationEngine.ProjectId,
                    Data = m.CalculationEngine.ToJson()
                };
            
                await UpsertAsync(record);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error updating transformer");
                throw new InvalidOperationException("Error updating transformer", e);
            }
        });
    }

    public async Task InitializeAsync()
    {
        await db.CreateTableAsync<TransformerEntity>().ConfigureAwait(false);
    }

    public Task AddAsync(TransformerRecord transformer) =>
        db.InsertAsync(ToEntity(transformer));

    public Task DeleteAsync(TransformerRecord transformer) =>
        db.DeleteAsync<TransformerEntity>(transformer.Id);

    public Task DeleteByIdAsync(Guid id) =>
        db.DeleteAsync<TransformerEntity>(id);

    public Task DeleteAllAsync() =>
        db.DeleteAllAsync<TransformerEntity>();

    public Task UpsertAsync(TransformerRecord transformer) =>
        db.InsertOrReplaceAsync(ToEntity(transformer));

    public async Task<List<TransformerRecord>> GetByProjectAsync(Guid projectId)
    {
        var entities = await db.Table<TransformerEntity>()
            .Where(t => t.ProjectId == projectId)
            .ToListAsync()
            .ConfigureAwait(false);

        return entities.Select(ToRecord).ToList();
    }

    public async Task<List<TransformerRecord>> GetAllAsync()
    {
        var entities = await db.Table<TransformerEntity>()
            .ToListAsync()
            .ConfigureAwait(false);

        return entities.Select(ToRecord).ToList();
    }

    public async Task<TransformerRecord?> GetByIdAsync(Guid id)
    {
        var entity = await db.FindAsync<TransformerEntity>(id).ConfigureAwait(false);
        return entity is null ? null : ToRecord(entity);
    }

    private static TransformerEntity ToEntity(TransformerRecord record) => new()
    {
        Id = record.Id,
        Name = record.Name,
        Description = record.Description,
        BelongToId = record.BelongToId,
        Data = record.Data,
        ProjectId = record.ProjectId,
    };

    private static TransformerRecord ToRecord(TransformerEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        BelongToId = entity.BelongToId,
        Data = entity.Data,
        ProjectId = entity.ProjectId,
    };
}
