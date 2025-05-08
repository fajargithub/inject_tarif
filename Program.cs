using InjectServiceWorker;
using InjectServiceWorker.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Configure Quartz
        services.AddHostedService<Worker>();

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        //Oracle Database Connection
        var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
        //var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DockerConnection");
        services.AddDbContext<ApplicationDbContext>(options => options.UseOracle(connectionString));

        // Add your job to DI
        services.AddTransient<FileMoverJob>();

        // Add Quartz.NET services
        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();

            // Create a job and trigger
            var jobKey = new JobKey("FileMoverJob");
            q.AddJob<FileMoverJob>(opts => opts.WithIdentity(jobKey));

            //q.AddTrigger(opts => opts
            //    .ForJob(jobKey)
            //    .WithIdentity("FileMoverTrigger")
            //    .WithCronSchedule("0 0 0 * * ?")); // Every day at midnight

            //q.AddTrigger(opts => opts
            //    .ForJob(jobKey)
            //    .WithIdentity("FileMoverTrigger")
            //    .StartNow() // Start immediately
            //    .WithSimpleSchedule(x => x
            //        .WithIntervalInSeconds(30) // Run every 30 seconds
            //        .RepeatForever())); // Keep repeating

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("FileMoverTrigger")
                .StartNow() // Start immediately
                .WithSimpleSchedule(x => x
                    .WithRepeatCount(0))); // Do not repeat
        });
    })
    .Build();

await host.RunAsync();
