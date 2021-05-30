using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Questionaire.Models
{
    [Table("Choice")]
    public class Choice
    {
        public static readonly string CHOICE_TYPE_RADIO_BUTTON = "Radio Button"; //Short list choose one
        public static readonly string CHOICE_TYPE_CHECKBOX = "Checkbox"; //Choose one or more
        public static readonly string CHOICE_TYPE_SELECT = "Select"; //Long list choose one
        public static readonly string CHOICE_TYPE_TEXT = "Text"; //Open end answer

        [Key]
        public int Id { get; set; }
        [Required]
        public int QuestionId { get; set; }
        [Required]
        public string Type { get; set; } // Radio Button, Checkbox, Select, Text
        public string Value { get; set; }
        public bool IsProhibited { get; set; }
    }
}
