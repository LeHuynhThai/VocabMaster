using Microsoft.EntityFrameworkCore;
using VocabMaster.Data;
using VocabMaster.Entities;

namespace VocabMaster.Repositories.Implementations;

public class VocabularyRepository : IVocabularyRepository
{
    private readonly AppDbContext _context; // DbContext

    // Constructor
    public VocabularyRepository(AppDbContext context)
    {
        _context = context;
    }

    // Get random vocabulary
    public async Task<Vocabulary> GetRandomAsync()
    {
        var count = await GetTotalCountAsync(); // Get total count of vocabularies
        if (count == 0) return null; // If no vocabularies, return null

        var random = new Random(); // Create a random number generator
        var skipCount = random.Next(0, count); // Create a random number in the range of 0 to count

        // Get a random vocabulary
        return await _context.Vocabularies  
            .Skip(skipCount) // Skip the number of vocabularies to skip
            .Take(1) // Take 1 vocabulary
            .FirstOrDefaultAsync(); // Get the first vocabulary
    }

    // Get total count of vocabularies
    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Vocabularies.CountAsync(); // Count vocabularies
    }

    // Get all vocabularies
    public async Task<IEnumerable<Vocabulary>> GetAllAsync()
    {
        return await _context.Vocabularies.ToListAsync(); // Get all vocabularies
    }
}


