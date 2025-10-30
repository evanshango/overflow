using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionService.Data;
using QuestionService.DTOs;
using QuestionService.Models;

namespace QuestionService.Controllers;

[ApiController, Route("/questions"), Produces("application/json"), Tags("Questions")]
public class QuestionsController(QuestionDbContext context) : ControllerBase
{
    [HttpPost(""), Authorize]
    public async Task<ActionResult<Question>> CreateQuestion([FromBody] CreateQuestionDto dto)
    {
        var validaTags = await context.Tags.Where(x => dto.Tags.Contains(x.Slug)).ToListAsync();

        var missing = dto.Tags.Except(validaTags.Select(x => x.Slug).ToList()).ToList();

        if (missing.Count != 0) return BadRequest($"Invalid tags: {string.Join(", ", missing)}");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var name = User.FindFirstValue("name");

        if (userId is null || name is null) return BadRequest("Cannot get user details");

        var question = new Question
        {
            Title = dto.Title,
            Content = dto.Content,
            TagSlugs = dto.Tags,
            AskerId = userId,
            AskerDisplayName = name
        };

        context.Questions.Add(question);

        var result = await context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Could not save changes to the DB");

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

    [HttpGet("{id}")]
    public async Task<ActionResult<Question>> GetQuestion([FromRoute] string id)
    {
        var question = await context.Questions.FindAsync(id);

        if (question is null) return NotFound();

        await context.Questions.Where(x => x.Id == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ViewCount,
                x => x.ViewCount + 1)
            );

        return question;
    }

    [HttpPut("{id}"), Authorize]
    public async Task<ActionResult> UpdateQuestion([FromRoute] string id, [FromBody] CreateQuestionDto dto)
    {
        var question = await context.Questions.FindAsync(id);

        if (question is null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId != question.AskerId) return Forbid();

        var validaTags = await context.Tags.Where(x => dto.Tags.Contains(x.Slug)).ToListAsync();

        var missing = dto.Tags.Except(validaTags.Select(x => x.Slug).ToList()).ToList();

        if (missing.Count != 0) return BadRequest($"Invalid tags: {string.Join(", ", missing)}");

        question.Title = dto.Title;
        question.Content = dto.Content;
        question.TagSlugs = dto.Tags;
        question.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}"), Authorize]
    public async Task<ActionResult> DeleteQuestion([FromRoute] string id)
    {
        var question = await context.Questions.FindAsync(id);

        if (question is null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId != question.AskerId) return Forbid();

        context.Questions.Remove(question);
        await context.SaveChangesAsync();
        return NoContent();
    }
}