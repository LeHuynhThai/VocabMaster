using Microsoft.EntityFrameworkCore;
using VocabMaster.Data;
using VocabMaster.Entities;
using VocabMaster.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace VocabMaster.Repositories.Implementations
{
    public class LearnedVocabularyRepository : ILearnedVocabularyRepository
    {
        private readonly AppDbContext _context;
        
        public LearnedVocabularyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LearnedVocabulary> GetByIdAsync(int id)
        {
            return await _context.LearnedVocabularies
                .Include(lv => lv.User)
                .FirstOrDefaultAsync(lv => lv.Id == id);
        }

        public async Task<List<LearnedVocabulary>> GetByUserIdAsync(int userId)
        {
            return await _context.LearnedVocabularies
                .Where(lv => lv.Id == userId)
                .OrderByDescending(lv => lv.Id)
                .ToListAsync();
        }

        public async Task<bool> AddAsync(LearnedVocabulary learnedVocabulary)
        {
            try
            {
                await _context.LearnedVocabularies.AddAsync(learnedVocabulary);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var learnedVocabulary = await _context.LearnedVocabularies.FindAsync(id);
                if (learnedVocabulary == null)
                    return false;

                _context.LearnedVocabularies.Remove(learnedVocabulary);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}