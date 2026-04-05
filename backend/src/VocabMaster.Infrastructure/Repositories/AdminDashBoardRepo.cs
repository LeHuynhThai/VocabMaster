using Microsoft.EntityFrameworkCore;
using VocabMaster.Domain.Entities;
using VocabMaster.Domain.Interfaces;
using VocabMaster.Infrastructure.Persistence;

namespace VocabMaster.Infrastructure.Repositories
{
    public class AdminDashBoardRepo : IAdminDashBoardRepo
    {
        private readonly ApplicationDbContext _context;

        public AdminDashBoardRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Vocabulary> AddVocabulary(Vocabulary vocabulary)
        {
            _context.Vocabularies.Add(vocabulary);
            await _context.SaveChangesAsync();
            return vocabulary;
        }

        public async Task<bool> DeleteVocabulary(int vocabularyId)
        {
            var vocabulary = await _context.Vocabularies.FindAsync(vocabularyId);
            if (vocabulary == null)
            {
                return false;
            }

            _context.Vocabularies.Remove(vocabulary);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Vocabulary>> GetVocabularies()
        {
            return await _context.Vocabularies
                .OrderBy(v => v.Word)
                .ToListAsync();
        }
    }
}
