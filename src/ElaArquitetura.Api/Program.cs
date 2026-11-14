using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using ElaArquitetura.Api;
using ElaArquitetura.Application.Interfaces.Auth;
using ElaArquitetura.Application.Interfaces.Repositories;
using ElaArquitetura.Application.UseCases.Auth;
using ElaArquitetura.Application.UseCases.Checklist;
using ElaArquitetura.Application.UseCases.Clientes;
using ElaArquitetura.Application.UseCases.Entregas;
using ElaArquitetura.Application.UseCases.Etapas;
using ElaArquitetura.Application.UseCases.Funcionarios;
using ElaArquitetura.Application.UseCases.Projetos;
using ElaArquitetura.Infrastructure.Auth;
using ElaArquitetura.Infrastructure.Persistence;
using ElaArquitetura.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ElaArquiteturaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Sem uma chave válida a API nem sobe (antes ela subia e só quebrava na hora do login).
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<LoginRateLimitOptions>()
    .Bind(builder.Configuration.GetSection(LoginRateLimitOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();
builder.Services.AddScoped<IProjetoRepository, ProjetoRepository>();
builder.Services.AddScoped<IEtapaRepository, EtapaRepository>();
builder.Services.AddScoped<ISubEtapaRepository, SubEtapaRepository>();
builder.Services.AddScoped<IChecklistItemRepository, ChecklistItemRepository>();
builder.Services.AddScoped<IEntregaRepository, EntregaRepository>();
builder.Services.AddScoped<IProjetoFuncionarioRepository, ProjetoFuncionarioRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

builder.Services.AddScoped<EmissorDeTokens>();
builder.Services.AddScoped<LoginUseCase>();
builder.Services.AddScoped<RefreshTokenUseCase>();
builder.Services.AddScoped<LogoutUseCase>();

builder.Services.AddScoped<CriarClienteUseCase>();
builder.Services.AddScoped<AtualizarClienteUseCase>();
builder.Services.AddScoped<BuscarClientesUseCase>();
builder.Services.AddScoped<ListarClientesConcluidosUseCase>();
builder.Services.AddScoped<ObterClientePorIdUseCase>();
builder.Services.AddScoped<ObterWhatsAppLinkUseCase>();
builder.Services.AddScoped<DesativarClienteUseCase>();

builder.Services.AddScoped<CriarFuncionarioUseCase>();
builder.Services.AddScoped<ListarFuncionariosUseCase>();
builder.Services.AddScoped<ObterFuncionarioPorIdUseCase>();
builder.Services.AddScoped<AtualizarFuncionarioUseCase>();
builder.Services.AddScoped<DesativarFuncionarioUseCase>();

builder.Services.AddScoped<AberturaDeProjeto>();
builder.Services.AddScoped<CriarProjetoUseCase>();
builder.Services.AddScoped<ListarProjetosUseCase>();
builder.Services.AddScoped<ObterProjetoPorIdUseCase>();
builder.Services.AddScoped<AtualizarProjetoUseCase>();
builder.Services.AddScoped<ExcluirProjetoUseCase>();
builder.Services.AddScoped<DefinirLinkDriveUseCase>();
builder.Services.AddScoped<ResponsavelDoProjeto>();
builder.Services.AddScoped<AvancarEtapaUseCase>();
builder.Services.AddScoped<ConcluirProjetoUseCase>();
builder.Services.AddScoped<ReabrirProjetoUseCase>();
builder.Services.AddScoped<AtribuirFuncionarioAoProjetoUseCase>();
builder.Services.AddScoped<RemoverFuncionarioDoProjetoUseCase>();

builder.Services.AddScoped<ConcluirChecklistItemUseCase>();
builder.Services.AddScoped<ReabrirChecklistItemUseCase>();
builder.Services.AddScoped<ListarChecklistDaEtapaAtualUseCase>();
builder.Services.AddScoped<ListarChecklistCompletaUseCase>();
builder.Services.AddScoped<RemoverChecklistItemUseCase>();
builder.Services.AddScoped<CriarChecklistItemUseCase>();
builder.Services.AddScoped<ProvisionadorChecklist>();

builder.Services.AddScoped<RegistrarEntregaUseCase>();
builder.Services.AddScoped<ListarEntregasDoProjetoUseCase>();

builder.Services.AddScoped<ListarEtapasUseCase>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

// Usa as mesmas JwtOptions (já validadas) que geram o token.
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((options, jwtOptions) =>
    {
        var jwt = jwtOptions.Value;
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
        };
    });

// Limita tentativas de login/refresh por IP, contra tentativa de senha em massa.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

        await context.HttpContext.Response.WriteAsJsonAsync(
            new { erros = new[] { "Muitas tentativas. Aguarde um pouco e tente novamente." } }, cancellationToken);
    };

    options.AddPolicy(LoginRateLimitOptions.PolicyName, httpContext =>
    {
        var limite = httpContext.RequestServices.GetRequiredService<IOptions<LoginRateLimitOptions>>().Value;
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";

        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = limite.PermitLimit,
            Window = TimeSpan.FromSeconds(limite.WindowSeconds),
            QueueLimit = 0
        });
    });
});

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
    options.AddPolicy("Desenvolvimento", policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
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

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ElaArquiteturaDbContext>();
        await dbContext.Database.MigrateAsync();
    }

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

// Libera o app Expo rodando no navegador (expo start --web) em ambiente local.
if (app.Environment.IsDevelopment())
    app.UseCors("Desenvolvimento");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}
