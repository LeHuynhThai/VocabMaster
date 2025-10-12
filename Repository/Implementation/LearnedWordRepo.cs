using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;

namespace Repository.Implementation
{
    public class LearnedWordRepo : ILearnedWordRepo
    {
        private readonly AppDbContext _context;
        public LearnedWordRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LearnedWord> GetById(int id)
        {
            return await _context.LearnedVocabularies.FindAsync(id);
        }

        public async Task<List<LearnedWord>> GetByUserId(int userId)
        {


                return await _context.LearnedVocabularies
                    .Where(lv => lv.UserId == userId)
                    .ToListAsync();
        }

        public async Task<bool> Add(LearnedWord learnedWord)
        {
                await _context.LearnedVocabularies.AddAsync(learnedWord);
                await _context.SaveChangesAsync();

                return true;
        }

        public async Task<bool> Delete(int id)
        {

                var learnedWord = await _context.LearnedVocabularies.FindAsync(id);
                _context.LearnedVocabularies.Remove(learnedWord);
                await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveByWord(int userId, string word)
        {
                var learnedWord = await _context.LearnedVocabularies
                    .FirstOrDefaultAsync(lv =>
                        lv.UserId == userId &&
                        EF.Functions.Like(lv.Word, word));

                _context.LearnedVocabularies.Remove(learnedWord);
                await _context.SaveChangesAsync();

                return true;
        }

        public async Task<(List<LearnedWord> Items, int TotalCount)> GetPaginatedByUserId(int userId, int pageNumber, int pageSize)
        {

                
                var query = _context.LearnedVocabularies
                    .Where(lw => lw.UserId == userId)
                    .OrderByDescending(lw => lw.LearnedAt);
                    
                int totalCount = await query.CountAsync();
                
                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                    
                return (items, totalCount);
        }
    }
}