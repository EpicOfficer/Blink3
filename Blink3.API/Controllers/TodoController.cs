using Blink3.DataAccess.Entities;
using Blink3.DataAccess.Interfaces;
using Blink3.DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blink3.API.Controllers;

/// <summary>
///     Controller for performing CRUD operations on userTodo items.
/// </summary>
[SwaggerTag("All CRUD operations for todo items")]
public class TodoController(IUserTodoRepository todoRepository) : ApiControllerBase
{
    /// <summary>
    ///     Retrieves all userTodo items for the current user.
    /// </summary>
    /// <returns>A list of UserTodo objects representing the todos.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Returns all todo items",
        Description = "Returns a list of all of the current user's todo items",
        OperationId = "Todo.GetAll",
        Tags = ["Todo"]
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(IEnumerable<UserTodo>))]
    public async Task<ActionResult<IEnumerable<UserTodo>>> GetAllTodos()
    {
        IReadOnlyCollection<UserTodo> todos = await todoRepository.GetByUserIdAsync(UserId);
        return Ok(todos);
    }

    /// <summary>
    ///     Retrieves a specific userTodo item by its Id.
    /// </summary>
    /// <param name="id">The Id of the userTodo item.</param>
    /// <returns>The userTodo item with the specified Id.</returns>
    [HttpGet("{id:int}")]
    [SwaggerOperation(
        Summary = "Returns a specific todo item",
        Description = "Returns a todo item by Id",
        OperationId = "Todo.GetTodo",
        Tags = ["Todo"]
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(UserTodo))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Todo not found", typeof(ProblemDetails))]
    public async Task<ActionResult<UserTodo>> GetTodo(int id)
    {
        UserTodo? todo = await todoRepository.GetByIdAsync(id);
        ObjectResult? accessCheckResult = CheckAccess(todo?.UserId);
        return accessCheckResult ?? Ok(todo);
    }

    /// <summary>
    ///     Creates a new userTodo item for the current user.
    /// </summary>
    /// <param name="userTodoDto">The userTodo item to be created.</param>
    /// <returns>The created userTodo item.</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Creates a new todo item",
        Description = "Creates a new todo item for the current user",
        OperationId = "Todo.Create",
        Tags = ["Todo"]
    )]
    [SwaggerResponse(StatusCodes.Status201Created, "Created", typeof(UserTodo))]
    public async Task<ActionResult<UserTodo>> CreateTodo([FromBody] UserTodoDto userTodoDto)
    {
        UserTodo createdTodo = await todoRepository.AddAsync(userTodoDto.ToEntity(UserId));

        return CreatedAtAction(nameof(GetTodo), new { id = createdTodo.Id }, createdTodo);
    }

    /// <summary>
    ///     Updates the content of a specific userTodo item.
    /// </summary>
    /// <param name="id">The ID of the userTodo item to update.</param>
    /// <param name="todoDto">The updated userTodo item data.</param>
    /// <returns>
    ///     No content.
    /// </returns>
    [HttpPut("{id:int}")]
    [SwaggerOperation(
        Summary = "Updates a specific todo item",
        Description = "Updates the content of a specific todo item",
        OperationId = "Todo.Update",
        Tags = ["Todo"]
    )]
    [SwaggerResponse(StatusCodes.Status204NoContent, "No content")]
    public async Task<ActionResult> UpdateTodo(int id, [FromBody] UserTodoDto todoDto)
    {
        UserTodo? todo = await todoRepository.GetByIdAsync(id);
        ObjectResult? accessCheckResult = CheckAccess(todo?.UserId);
        if (accessCheckResult is not null) return accessCheckResult;

        await todoRepository.UpdateAsync(todoDto.ToEntity(UserId, id));

        return NoContent();
    }

    /// <summary>
    ///     Deletes a specific userTodo item.
    /// </summary>
    /// <param name="id">The ID of the userTodo item to delete.</param>
    /// <returns>Returns a <see cref="ActionResult" /> indicating the result of the operation.</returns>
    [HttpDelete("{id:int}")]
    [SwaggerOperation(
        Summary = "Deletes a specific todo item",
        Description = "Deletes a single todo item based on the Id",
        OperationId = "Todo.Delete",
        Tags = ["Todo"]
    )]
    [SwaggerResponse(StatusCodes.Status204NoContent, "No content")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Todo not found", typeof(ProblemDetails))]
    public async Task<ActionResult> DeleteTodo(int id)
    {
        UserTodo? todo = await todoRepository.GetByIdAsync(id);
        ObjectResult? accessCheckResult = CheckAccess(todo?.UserId);
        if (accessCheckResult is not null) return accessCheckResult;

        await todoRepository.DeleteAsync(todo!);

        return NoContent();
    }
}