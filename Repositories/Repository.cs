using Microsoft.EntityFrameworkCore;
using BanquetHall.Data;

namespace BanquetHall.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly BanquetHallDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(BanquetHallDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public IQueryable<T> Query() => _dbSet.AsQueryable();

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public void Update(T entity) => _dbSet.Update(entity);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
