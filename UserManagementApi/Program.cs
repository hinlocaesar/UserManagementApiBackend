var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Create an in-memory collection to store users
var users = new List<User>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello World!");

// GET: Retrieve all users
app.MapGet("/api/users", () => users);

// GET: Retrieve a specific user by ID
app.MapGet("/api/users/{id}", (Guid id) => 
{
    var user = users.FirstOrDefault(u => u.Id == id);
    return user is null ? Results.NotFound() : Results.Ok(user);
});

// POST: Add a new user
app.MapPost("/api/users", (User user) =>
{
    user.Id = Guid.NewGuid();
    users.Add(user);
    return Results.Created($"/api/users/{user.Id}", user);
});

// PUT: Update an existing user's details
app.MapPut("/api/users/{id}", (Guid id, User updatedUser) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null) return Results.NotFound();
    
    user.Name = updatedUser.Name;
    user.Email = updatedUser.Email;
    // Update other properties as needed
    
    return Results.NoContent();
});

// DELETE: Remove a user by ID
app.MapDelete("/api/users/{id}", (Guid id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null) return Results.NotFound();
    
    users.Remove(user);
    return Results.NoContent();
});

app.Run();

// User model class
class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    // Add additional properties as needed
}