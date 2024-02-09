using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

class TaskManager
{
    List<string> tasks = new List<string>();

    public void AddTask(string task)
    {
        tasks.Add(task);
    }

    public List<string> GetTasks()
    {
        return tasks;
    }

    public void MarkTaskCompleted(int taskIndex)
    {
        if (taskIndex >= 0 && taskIndex < tasks.Count)
        {
            tasks.RemoveAt(taskIndex);
        }
    }
}

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<TaskManager>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Task Manager API");
            });

            endpoints.MapGet("/tasks", async context =>
            {
                var taskManager = context.RequestServices.GetRequiredService<TaskManager>();
                var tasks = taskManager.GetTasks();
                await context.Response.WriteAsJsonAsync(tasks);
            });

            endpoints.MapPost("/tasks", async context =>
            {
                var taskManager = context.RequestServices.GetRequiredService<TaskManager>();
                using var reader = new System.IO.StreamReader(context.Request.Body);
                var task = await reader.ReadToEndAsync();
                taskManager.AddTask(task);
                context.Response.StatusCode = 201; // Created
            });

            endpoints.MapDelete("/tasks/{taskIndex}", async context =>
            {
                var taskManager = context.RequestServices.GetRequiredService<TaskManager>();
                if (int.TryParse(context.Request.RouteValues["taskIndex"] as string, out int index))
                {
                    taskManager.MarkTaskCompleted(index);
                    context.Response.StatusCode = 204; // No Content
                }
                else
                {
                    context.Response.StatusCode = 400; // Bad Request
                }
            });
        });
    }
}

class Program
{
    static void Main()
    {
        CreateHostBuilder().Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args = null) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
