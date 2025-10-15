using Repository.Entities;
using Repository.Interfaces;
using Service.Interfaces;

namespace Service.Implementation
{
    public class AdminDashBoardService : IAdminDashBoardService
    {
        private readonly IAdminDashBoardRepo _adminDashBoardRepo;

        public AdminDashBoardService(IAdminDashBoardRepo adminDashBoardRepo)
        {
            _adminDashBoardRepo = adminDashBoardRepo;
        }

        public async Task<Vocabulary> AddVocabulary(Vocabulary vocabulary)
        {
            return await _adminDashBoardRepo.AddVocabulary(vocabulary);
        }

        public async Task<bool> DeleteVocabulary(int vocabularyId)
        {
            return await _adminDashBoardRepo.DeleteVocabulary(vocabularyId);
        }
    }
}
