using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Questionaire.DAL;
using Questionaire.Models;

namespace Questionaire.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public QuestionController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(QuestionViewModel questionViewModel)
        {
            var question = new Question { Title = questionViewModel.Title };
            await _dbContext.Questions.AddAsync(question);
            await _dbContext.SaveChangesAsync();

            List<Choice> choices = new List<Choice>();
            foreach (ChoiceViewModel choice in questionViewModel.Choices)
            {
                choices.Add(new Choice
                {
                    QuestionId = question.Id,
                    IsProhibited = choice.IsProhibited,
                    Type = choice.Type,
                    Value = choice.Value
                });
            }

            await _dbContext.Choices.AddRangeAsync(choices);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public List<QuestionViewModel> List()
        {
            List<QuestionViewModel> result = new List<QuestionViewModel>();

            List<Question> questions = _dbContext.Questions.ToList<Question>();
            foreach (var question in questions)
            {
                List<Choice> choices = _dbContext.Choices.Where<Choice>(c => c.QuestionId == question.Id).ToList<Choice>();
                result.Add(QuestionViewModel.FromDataModel(question, choices));
            }

            return result;
        }

        [HttpGet]
        [Route("{id:int}")]
        public ActionResult<QuestionViewModel> Get(int id)
        {
            try
            {
                Question question = _dbContext.Questions.Where<Question>(q => q.Id == id).Single<Question>();
                var choices = _dbContext.Choices.Where<Choice>(c => c.QuestionId == question.Id).ToList<Choice>();

                return QuestionViewModel.FromDataModel(question, choices);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> UpdateAsync(int id, QuestionViewModel questionViewModel)
        {
            _dbContext.Questions.Update(new Question
            {
                Id = id,
                Title = questionViewModel.Title
            });

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }

            // Replace all existing choices with the new one.
            _dbContext.RemoveRange(_dbContext.Choices.Where<Choice>(c => c.QuestionId == id));

            foreach (var choiceViewModel in questionViewModel.Choices)
            {
                _dbContext.Choices.Add(ChoiceViewModel.ToDataModel(id, choiceViewModel));
            }

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            _dbContext.RemoveRange(_dbContext.Questions.Where<Question>(q => q.Id == id).ToList<Question>());
            _dbContext.RemoveRange(_dbContext.Choices.Where<Choice>(c => c.QuestionId == id).ToList<Choice>());
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }

    public class QuestionViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public List<ChoiceViewModel> Choices { get; set; }

        public static QuestionViewModel FromDataModel(Question question, List<Choice> choices)
        {
            List<ChoiceViewModel> choiceViewModels = new List<ChoiceViewModel>();
            foreach (var choice in choices)
            {
                choiceViewModels.Add(ChoiceViewModel.FromDataModel(choice));
            }

            return new QuestionViewModel
            {
                Id = question.Id,
                Title = question.Title,
                Choices = choiceViewModels
            };
        }
    }

    public class ChoiceViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Type { get; set; } // Radio Button, Checkbox, Select, Text
        public string Value { get; set; }
        [DefaultValue(false)]
        public bool IsProhibited { get; set; }

        public static ChoiceViewModel FromDataModel(Choice choice)
        {
            return new ChoiceViewModel
            {
                Id = choice.Id,
                Type = choice.Type,
                Value = choice.Value,
                IsProhibited = choice.IsProhibited
            };
        }

        public static Choice ToDataModel(int questionId, ChoiceViewModel choiceViewModel)
        {
            return new Choice
            {
                QuestionId = questionId,
                Type = choiceViewModel.Type,
                IsProhibited = choiceViewModel.IsProhibited,
                Value = choiceViewModel.Value
            };
        }
    }

}
