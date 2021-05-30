using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Questionaire.DAL;
using Questionaire.Models;

namespace Questionaire.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AnswerController : ControllerBase
    {

        private readonly ApplicationDbContext _dbContext;

        public AnswerController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(AnswerViewModel answerViewModel)
        {
            if (answerViewModel.ChoiceIds.Count == 0)
            {
                return BadRequest("Answer must have at least one choice.");
            }

            var choices = _dbContext.Choices.Where<Choice>(c => answerViewModel.ChoiceIds.Contains(c.Id)).ToList();
            if (choices.Count != answerViewModel.ChoiceIds.Count)
            {
                return BadRequest("Invalid choice selected.");
            }

            if (choices.Where<Choice>(c => c.Type == Choice.CHOICE_TYPE_RADIO_BUTTON).Count() > 1)
            {
                return BadRequest($"Multiple choices selected for choice type Radio Button is not allowed.");
            }

            if (choices.Where<Choice>(c => c.Type == Choice.CHOICE_TYPE_SELECT).Count() > 1)
            {
                return BadRequest($"Multiple choices selected for choice type Select is not allowed.");
            }

            foreach (var choice in choices)
            {
                if (choice.IsProhibited)
                {
                    return BadRequest($"Choice {choice.Id}:{choice.Value} is not allowed.");
                }

                if (Choice.CHOICE_TYPE_TEXT == choice.Type && String.IsNullOrEmpty(answerViewModel.Text))
                {
                    return BadRequest($"Text is required for choice type Text.");
                }

                if (Choice.CHOICE_TYPE_TEXT == choice.Type)
                {
                    choice.Value = answerViewModel.Text;
                }
            }

            try
            {
                var question = _dbContext.Questions.Where(q => q.Id == answerViewModel.QuestionId).Single();
                _dbContext.Answers.Add(new Answer
                {
                    UserId = answerViewModel.UserId,
                    QuestionId = answerViewModel.QuestionId,
                    QuestionTitle = question.Title,
                    Value = String.Join("|", choices.Select(c => c.Value).ToList())
                });
            }
            catch(InvalidOperationException)
            {
                return NotFound();
            }

            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet] //CSV
        [Route("download")]
        public FileResult Download()
        {
            string fileName = "answers.csv";
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("answerId, userId, questionId, questionTitle, answer");

            //TODO: Select only participants that finished all the questions
            var answers = from a in _dbContext.Answers
                          orderby a.UserId, a.QuestionId
                          select a;

            foreach (var answer in answers)
            {
                string line = $"\"{answer.Id}\", \"{answer.UserId}\", \"{answer.QuestionId}\", \"{answer.QuestionTitle}\", \"{answer.Value}\"";
                stringBuilder.Append(line);
            }

            return File(Encoding.UTF8.GetBytes(stringBuilder.ToString()), "text/csv", fileName);
        }

        [HttpGet]
        [Route("{userId}/question/{questionId:int}")]
        public ActionResult<Answer> Get(string userId, int questionId)
        {
            try
            {
                return _dbContext.Answers.Where(a => a.UserId == Guid.Parse(userId) && a.QuestionId == questionId).Single();
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        [HttpPut]
        [Route("{userId}/question/{questionId:int}")]
        public async Task<IActionResult> UpdateAsync(string userId, int questionId, AnswerViewModel answerViewModel)
        {
            await DeleteAsync(userId, questionId);
            await CreateAsync(answerViewModel);
            return Ok();
        }

        [HttpDelete]
        [Route("{userId}/question/{questionId:int}")]
        public async Task<IActionResult> DeleteAsync(string userId, int questionId)
        {
            _dbContext.Remove(_dbContext.Answers.Where(a => a.UserId == Guid.Parse(userId) && a.QuestionId == questionId).Single());
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }

    public class AnswerViewModel
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public int QuestionId { get; set; }
        [Required]
        public List<int> ChoiceIds { get; set; }
        public string Text { get; set; }
    }
}
