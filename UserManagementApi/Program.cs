using FluentValidation;
using UserManagementApi.Models;
using UserManagementApi.Validators;
// using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLogging(o => { });

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add FluentValidation
builder.Services.AddScoped<IValidator<User>, UserValidator>();

// Create an in-memory dictionary to store users for O(1) lookups
var users = new Dictionary<Guid, User>();

var app = builder.Build();

app.UseHttpLogging();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello World!");

// GET: Retrieve all users
app.MapGet("/api/users", (ILogger<Program> logger) => 
{
    try
    {
        return Results.Ok(users.Values);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error occurred while retrieving all users");
        return Results.Problem("An error occurred while processing your request.", statusCode: 500);
    }
});

// GET: Retrieve a specific user by ID
app.MapGet("/api/users/{id}", (Guid id, ILogger<Program> logger) => 
{
    try
    {
        if (users.TryGetValue(id, out var user))
        {
            return Results.Ok(user);
        }
        return Results.NotFound();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error occurred while retrieving user with ID {UserId}", id);
        return Results.Problem("An error occurred while processing your request.", statusCode: 500);
    }
});

// POST: Add a new user
app.MapPost("/api/users", (User user, IValidator<User> validator, ILogger<Program> logger) =>
{
    try
    {
        // Validate the user
        var validationResult = validator.Validate(user);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors.Select(e => new { Property = e.PropertyName, Error = e.ErrorMessage }));
        }
        
        user.Id = Guid.NewGuid();
        users[user.Id] = user;
        return Results.Created($"/api/users/{user.Id}", user);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error occurred while creating a new user");
        return Results.Problem("An error occurred while processing your request.", statusCode: 500);
    }
});

// PUT: Update an existing user's details
app.MapPut("/api/users/{id}", (Guid id, User updatedUser, IValidator<User> validator, ILogger<Program> logger) =>
{
    try
    {
        if (!users.TryGetValue(id, out var user))
            return Results.NotFound();
        
        // Validate the updated user
        var validationResult = validator.Validate(updatedUser);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors.Select(e => new { Property = e.PropertyName, Error = e.ErrorMessage }));
        }
        
        user.Name = updatedUser.Name;
        user.Email = updatedUser.Email;
        // Update other properties as needed
        users[id] = user; // Ensure the updated user is saved back to the dictionary
        
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error occurred while updating user with ID {UserId}", id);
        return Results.Problem("An error occurred while processing your request.", statusCode: 500);
    }
});

// DELETE: Remove a user by ID
app.MapDelete("/api/users/{id}", (Guid id, ILogger<Program> logger) =>
{
    try
    {
        if (!users.ContainsKey(id))
            return Results.NotFound();
        
        users.Remove(id);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error occurred while deleting user with ID {UserId}", id);
        return Results.Problem("An error occurred while processing your request.", statusCode: 500);
    }
});

app.Run();