using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;

namespace Repository.Implementation
{
    public class DictionaryDetailsRepo : IDictionaryDetailsRepo
    {
        private readonly AppDbContext _context;

        public DictionaryDetailsRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Vocabulary> GetByWord(string word)
        {
                return await _context.Vocabularies
                    .FirstOrDefaultAsync(v => v.Word.ToLower() == word.ToLower());
        }

        public async Task<Vocabulary> AddOrUpdate(Vocabulary details)
        {
                var existingDetails = await _context.Vocabularies
                    .FirstOrDefaultAsync(v => v.Word.ToLower() == details.Word.ToLower());

                if (existingDetails != null)
                {

                    existingDetails.PhoneticsJson = details.PhoneticsJson;
                    existingDetails.MeaningsJson = details.MeaningsJson;
                    existingDetails.UpdatedAt = DateTime.UtcNow;

                    _context.Vocabularies.Update(existingDetails);
                    await _context.SaveChangesAsync();

                    return existingDetails;
                }
                else
                {
                    await _context.Vocabularies.AddAsync(details);
                    await _context.SaveChangesAsync();

                    return details;
                }
        }

        public async Task<List<Vocabulary>> GetAll()
        {
            return await _context.Vocabularies.ToListAsync();
        }

        public async Task<bool> Exists(string word)
        {

                return await _context.Vocabularies
                    .AnyAsync(v => v.Word.ToLower() == word.ToLower());
        }
    }
}