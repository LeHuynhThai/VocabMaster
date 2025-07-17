using System;
using System.ComponentModel.DataAnnotations;

namespace VocabMaster.Models.ViewModels
{
    public class LearnedVocabularyViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Word { get; set; }

        [Required]
        public int UserId { get; set; }
    }
}