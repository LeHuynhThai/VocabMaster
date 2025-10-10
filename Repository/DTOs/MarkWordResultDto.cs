using VocabMaster.Core.Entities;

namespace VocabMaster.Core.DTOs
{
    public class MarkWordResultDto
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public LearnedWord Data { get; set; }
    }
}