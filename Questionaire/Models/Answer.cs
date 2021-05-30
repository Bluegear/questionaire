using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Questionaire.Models
{
    [Table("Answer")]
    public class Answer
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public int QuestionId { get; set; }
        [Required]
        public string QuestionTitle { get; set; }
        [Required]
        public string Value { get; set; } // Multiple choices answer will store in | (pipe) separated format
    }
}
