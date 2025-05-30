using TesteL2.Data;

namespace TesteL2;

public class JwtServices
{
    private readonly LojaContext _context;
    
    public readonly IConfiguration _configuration;
    
    public JwtServices(LojaContext lojaContext,IConfiguration configuration)
    {_context=lojaContext;
    _configuration=configuration;
    }
}