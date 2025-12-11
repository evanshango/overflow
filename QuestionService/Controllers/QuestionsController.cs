using System.Security.Claims;
using Contracts;
using FastExpressionCompiler;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionService.Data;
using QuestionService.DTOs;
using QuestionService.Models;
using QuestionService.Services;
using Wolverine;

namespace QuestionService.Controllers;

[ApiController, Route("/api/v1/questions"), Produces("application/json"), Tags("Questions")]
public class QuestionsController(
    QuestionDbContext context,
    TagService tagService,
    IMessageBus bus
) : ControllerBase
{
    [HttpPost(""), Authorize]
    public async Task<ActionResult<Question>> CreateQuestion([FromBody] CreateQuestionDto dto)
    {
        if (!await tagService.AreTagsValidAsync(dto.Tags)) return BadRequest("Invalid tags");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var name = User.FindFirstValue("name");

        if (userId is null || name is null) return BadRequest("Cannot get user details");

        var sanitizer = new HtmlSanitizer();

        var question = new Question
        {
            Title = dto.Title,
            Content = sanitizer.Sanitize(dto.Content),
            TagSlugs = dto.Tags,
            AskerId = userId,
            AskerDisplayName = name
        };

        context.Questions.Add(question);

        var result = await context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Could not save changes to the DB");

        await bus.PublishAsync(new QuestionCreated(
            question.Id, question.Title, question.Content, question.CreatedAt, question.TagSlugs
        ));

        return Created($"/questions/{question.Id}", question);
    }

    [HttpGet("")]
    public async Task<ActionResult<List<Question>>> GetQuestions([FromQuery] string? tag)
    {
        var query = context.Questions.AsQueryable();

        if (!string.IsNullOrEmpty(tag))
        {
            query = query.Where(x => x.TagSlugs.Contains(tag));
        }

        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    [HttpGet("{questionId}")]
    public async Task<ActionResult<Question>> GetQuestion([FromRoute] string questionId)
    {
        var question = await context.Questions
            .Include(x => x.Answers)
            .FirstOrDefaultAsync(x => x.Id == questionId);

        if (question is null) return NotFound();

        await context.Questions
            .Where(x => x.Id == questionId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.ViewCount, x => x.ViewCount + 1)
            );

        return question;
    }

    [HttpPut("{questionId}"), Authorize]
    public async Task<ActionResult> UpdateQuestion([FromRoute] string questionId, [FromBody] CreateQuestionDto dto)
    {
        var question = await context.Questions.FindAsync(questionId);

        if (question is null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId != question.AskerId) return Forbid();

        if (!await tagService.AreTagsValidAsync(dto.Tags)) return BadRequest("Invalid tags");

        var sanitizer = new HtmlSanitizer();

        question.Title = dto.Title;
        question.Content = sanitizer.Sanitize(dto.Content);
        question.TagSlugs = dto.Tags;
        question.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        await bus.PublishAsync(new QuestionUpdated(
            question.Id, question.Title, question.Content, question.TagSlugs.AsArray()
        ));

        return NoContent();
    }

    [HttpDelete("{questionId}"), Authorize]
    public async Task<ActionResult> DeleteQuestion([FromRoute] string questionId)
    {
        var question = await context.Questions.FindAsync(questionId);

        if (question is null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId != question.AskerId) return Forbid();

        context.Questions.Remove(question);
        await context.SaveChangesAsync();

        await bus.PublishAsync(new QuestionDeleted(question.Id));

        return NoContent();
    }

    [HttpPost("{questionId}/answers"), Authorize]
    public async Task<ActionResult> PostAnswer([FromRoute] string questionId, [FromBody] CreateAnswerDto dto)
    {
        var question = await context.Questions.FindAsync(questionId);

        if (question is null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var name = User.FindFirstValue("name");

        if (userId is null || name is null) return BadRequest("Cannot get user details");

        var sanitizer = new HtmlSanitizer();

        var answer = new Answer
        {
            Content = sanitizer.Sanitize(dto.Content),
            UserId = userId,
            UserDisplayName = name,
            QuestionId = question.Id
        };

        question.Answers.Add(answer);
        question.AnswerCount++;

        await context.SaveChangesAsync();

        await bus.PublishAsync(new AnswerCountUpdated(question.Id, question.AnswerCount));

        return Created($"/questions/{question.Id}", answer);
    }

    [HttpPut("{questionId}/answers/{answerId}"), Authorize]
    public async Task<ActionResult> UpdateAnswer(
        [FromRoute] string questionId, [FromRoute] string answerId, [FromBody] CreateAnswerDto dto
    )
    {
        var answer = await context.Answers.FindAsync(answerId);
        if (answer is null) return NotFound();
        if (answer.QuestionId != questionId) return Forbid("Cannot update answer details");

        var sanitizer = new HtmlSanitizer();

        answer.Content = sanitizer.Sanitize(dto.Content);
        answer.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{questionId}/answers/{answerId}"), Authorize]
    public async Task<ActionResult> DeleteAnswer([FromRoute] string questionId, [FromRoute] string answerId)
    {
        var answer = await context.Answers.FindAsync(answerId);
        var question = await context.Questions.FindAsync(questionId);

        if (answer is null || question is null) return NotFound();
        if (answer.QuestionId != questionId || answer.Accepted) return BadRequest("Cannot delete this answer");

        context.Answers.Remove(answer);
        question.AnswerCount--;

        await context.SaveChangesAsync();

        await bus.PublishAsync(new AnswerCountUpdated(question.Id, question.AnswerCount));
        return NoContent();
    }

    [HttpPost("{questionId}/answers/{answerId}/accept"), Authorize]
    public async Task<ActionResult> AcceptAnswer([FromRoute] string questionId, [FromRoute] string answerId)
    {
        var answer = await context.Answers.FindAsync(answerId);
        var question = await context.Questions.FindAsync(questionId);

        if (answer is null || question is null) return NotFound();
        if (answer.QuestionId != questionId || question.HasAcceptedAnswer) return BadRequest("Cannot accept answer");

        answer.Accepted = true;
        question.HasAcceptedAnswer = true;

        await context.SaveChangesAsync();

        await bus.PublishAsync(new AnswerAccepted(question.Id));

        return NoContent();
    }
}