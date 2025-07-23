using System.ComponentModel.DataAnnotations;

namespace VocabMaster.Core.Entities
{
    public class Vocabulary
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string Word { get; set; }
    }
}
