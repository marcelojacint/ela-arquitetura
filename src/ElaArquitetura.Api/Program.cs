using System.Text;
using System.Text.Json.Serialization;
using ElaArquitetura.Api;
using ElaArquitetura.Application.Interfaces.Auth;
using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Application.UseCases.Auth;
using ElaArquitetura.Application.UseCases.Checklist;
using ElaArquitetura.Application.UseCases.Clientes;
using ElaArquitetura.Application.UseCases.Funcionarios;
using ElaArquitetura.Application.UseCases.Projetos;
using ElaArquitetura.Infrastructure.Auth;
using ElaArquitetura.Infrastructure.Persistence;
using ElaArquitetura.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ElaArquiteturaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();
builder.Services.AddScoped<IProjetoRepository, ProjetoRepository>();
builder.Services.AddScoped<IEtapaRepository, EtapaRepository>();
builder.Services.AddScoped<IChecklistItemRepository, ChecklistItemRepository>();
builder.Services.AddScoped<IEntregaRepository, EntregaRepository>();
builder.Services.AddScoped<IProjetoFuncionarioRepository, ProjetoFuncionarioRepository>();

builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.AddScoped<LoginUseCase>();

builder.Services.AddScoped<CriarClienteUseCase>();
builder.Services.AddScoped<AtualizarClienteUseCase>();
builder.Services.AddScoped<BuscarClientesUseCase>();
builder.Services.AddScoped<ObterClientePorIdUseCase>();

builder.Services.AddScoped<CriarFuncionarioUseCase>();
builder.Services.AddScoped<ListarFuncionariosUseCase>();

builder.Services.AddScoped<CriarProjetoUseCase>();
builder.Services.AddScoped<ListarProjetosUseCase>();
builder.Services.AddScoped<ObterProjetoPorIdUseCase>();
builder.Services.AddScoped<AvancarEtapaUseCase>();
builder.Services.AddScoped<ConcluirProjetoUseCase>();
builder.Services.AddScoped<ReabrirProjetoUseCase>();
builder.Services.AddScoped<AtribuirFuncionarioAoProjetoUseCase>();
builder.Services.AddScoped<RemoverFuncionarioDoProjetoUseCase>();

builder.Services.AddScoped<ConcluirChecklistItemUseCase>();
builder.Services.AddScoped<ReabrirChecklistItemUseCase>();
builder.Services.AddScoped<ListarChecklistDaEtapaAtualUseCase>();
builder.Services.AddScoped<CriarChecklistItemUseCase>();

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"] ?? string.Empty))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Éla Arquitetura API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe apenas o token JWT (sem o prefixo \"Bearer\")."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", document, null), new List<string>() }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await DevSeed.GarantirFuncionarioPadraoAsync(app.Services);
}
else
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { erro = "Ocorreu um erro inesperado." });
        });
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
