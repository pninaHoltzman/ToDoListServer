using TodoApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// הוספת DbContext
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDb"), 
        new MySqlServerVersion(new Version(8, 0, 41)))); 

// הוספת Cors
builder.Services.AddCors(option => option.AddPolicy("AllowAll", 
    p => p.AllowAnyOrigin() 
    .AllowAnyMethod() 
    .AllowAnyHeader()));

// הוספת Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "My API", Version = "v1" });
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// הפעלת Swagger רק אם אנחנו בסביבת פיתוח
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); 
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");  
    });
}

app.UseCors("AllowAll");

app.MapGet("/", () => "Hello World!");

app.MapGet("/items", async (ToDoDbContext db) =>
{
    return await db.Items.ToListAsync();
});

app.MapPost("/addTask", async (ToDoDbContext db, Item newTask) => 
{
    db.Items.Add(newTask);  
    Console.WriteLine($"Adding item: {newTask.Name}"); 

    await db.SaveChangesAsync(); 
    Console.WriteLine("Saved successfully!");

    return Results.Created($"/addTask/{newTask.Id}", newTask);
});
app.MapDelete("/deleteTask/{IdTask}", async (ToDoDbContext db,int IdTask) => { 
    
    var itemToDelete =await db.Items.FindAsync(IdTask);
if(itemToDelete==null)
return Results.NotFound();
db.Items.Remove(itemToDelete);
await db.SaveChangesAsync();
return Results.Ok($"/deleteTask/{IdTask}");

});
app.MapPatch("/apdatesTask/{IdTask}", async (ToDoDbContext db,int IdTask) => { 
    var itemToUpdate = await db.Items.FindAsync(IdTask);
    if(itemToUpdate==null)
    return Results.NotFound();
    itemToUpdate.IsComplete=!itemToUpdate.IsComplete;
    await db.SaveChangesAsync();
    return Results.Ok($"/apdatesTask/{IdTask}");
});
app.Run();

